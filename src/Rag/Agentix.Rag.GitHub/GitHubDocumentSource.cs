using System.Text;
using Agentix.Rag.Core.Interfaces;
using Agentix.Rag.Core.Models;
using Microsoft.Extensions.Logging;
using Octokit;

namespace Agentix.Rag.GitHub;

/// <summary>
/// Document source implementation for GitHub repositories.
/// Handles cloning, file extraction, and document creation from GitHub repos.
/// </summary>
internal class GitHubDocumentSource : IDocumentSource
{
    private readonly GitHubClient _githubClient;
    private readonly GitHubRAGOptions _options;
    private readonly ILogger<GitHubDocumentSource> _logger;

    public string Name => "GitHub";
    public string SourceType => "github";

    public GitHubDocumentSource(GitHubRAGOptions options, ILogger<GitHubDocumentSource> logger)
    {
        _options = options;
        _logger = logger;
        
        // Configure GitHub client
        _githubClient = new GitHubClient(new ProductHeaderValue("Agentix"));
        if (!string.IsNullOrEmpty(_options.AccessToken))
        {
            _githubClient.Credentials = new Credentials(_options.AccessToken);
        }
    }

    public async Task<bool> CanHandleAsync(SourceConfig sourceConfig)
    {
        if (sourceConfig.SourceType != "github") return false;
        
        var url = sourceConfig.Configuration.TryGetValue("url", out var urlValue) ? urlValue?.ToString() : null;
        return IsGitHubUrl(url);
    }

    public async Task<IEnumerable<Document>> LoadDocumentsAsync(SourceConfig sourceConfig, CancellationToken cancellationToken = default)
    {
        var url = sourceConfig.Configuration.TryGetValue("url", out var urlValue) ? urlValue?.ToString() : null;
        if (string.IsNullOrEmpty(url) || !IsGitHubUrl(url))
        {
            _logger.LogWarning("Invalid GitHub URL: {Url}", url);
            return Enumerable.Empty<Document>();
        }

        try
        {
            var (owner, repo) = ParseGitHubUrl(url);
            _logger.LogInformation("Loading documents from {Owner}/{Repo}", owner, repo);

            var documents = new List<Document>();
            
            // Get repository info
            var repository = await _githubClient.Repository.Get(owner, repo);
            var repoInfo = new RepositoryInfo
            {
                Name = repository.FullName,
                Description = repository.Description,
                DefaultBranch = repository.DefaultBranch,
                Language = repository.Language,
                Size = (int)Math.Min(repository.Size, int.MaxValue)
            };

            // Determine relevant file types based on repository language
            var fileExtensions = DetermineRelevantFileTypes(repoInfo);
            
            // Load files from repository
            var contents = await LoadRepositoryContents(_githubClient, owner, repo, fileExtensions, cancellationToken);
            documents.AddRange(contents);

            // Load issues (if they seem relevant)
            if (ShouldIndexIssues(repoInfo))
            {
                var issues = await LoadRepositoryIssues(_githubClient, owner, repo, cancellationToken);
                documents.AddRange(issues);
            }

            // Load pull requests (if they seem relevant)
            if (ShouldIndexPullRequests(repoInfo))
            {
                var prs = await LoadRepositoryPullRequests(_githubClient, owner, repo, cancellationToken);
                documents.AddRange(prs);
            }

            _logger.LogInformation("Loaded {Count} documents from {Owner}/{Repo}", documents.Count, owner, repo);
            return documents;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading documents from {Url}", url);
            return Enumerable.Empty<Document>();
        }
    }

    public async Task<SourceStatus> GetStatusAsync(SourceConfig sourceConfig)
    {
        var url = sourceConfig.Configuration.TryGetValue("url", out var urlValue) ? urlValue?.ToString() : null;
        
        try
        {
            if (!IsGitHubUrl(url))
            {
                return new SourceStatus
                {
                    SourceId = sourceConfig.Id,
                    SourceName = url ?? "Unknown",
                    Status = IndexingStatus.Error,
                    ErrorMessage = "Invalid GitHub URL"
                };
            }

            var (owner, repo) = ParseGitHubUrl(url);
            
            // Try to access the repository to check status
            var repository = await _githubClient.Repository.Get(owner, repo);
            
            return new SourceStatus
            {
                SourceId = sourceConfig.Id,
                SourceName = repository.FullName,
                Status = IndexingStatus.Ready,
                LastUpdated = repository.UpdatedAt.DateTime
            };
        }
        catch (NotFoundException)
        {
            return new SourceStatus
            {
                SourceId = sourceConfig.Id,
                SourceName = url ?? "Unknown",
                Status = IndexingStatus.Error,
                ErrorMessage = "Repository not found or access denied"
            };
        }
        catch (Exception ex)
        {
            return new SourceStatus
            {
                SourceId = sourceConfig.Id,
                SourceName = url ?? "Unknown",
                Status = IndexingStatus.Error,
                ErrorMessage = ex.Message
            };
        }
    }

    private bool IsGitHubUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return false;
        
        return url.StartsWith("https://github.com/", StringComparison.OrdinalIgnoreCase) ||
               url.StartsWith("http://github.com/", StringComparison.OrdinalIgnoreCase);
    }

    private (string owner, string repo) ParseGitHubUrl(string url)
    {
        var uri = new Uri(url);
        var segments = uri.AbsolutePath.Trim('/').Split('/');
        
        if (segments.Length < 2)
            throw new ArgumentException($"Invalid GitHub URL format: {url}");
            
        return (segments[0], segments[1]);
    }

    private string[] DetermineRelevantFileTypes(RepositoryInfo repoInfo)
    {
        var fileTypes = new List<string> { ".md", ".txt" }; // Always include docs
        
        // Add language-specific file types
        var language = repoInfo.Language?.ToLowerInvariant();
        switch (language)
        {
            case "c#":
                fileTypes.AddRange([".cs", ".csproj", ".sln", ".json"]);
                break;
            case "typescript":
                fileTypes.AddRange([".ts", ".tsx", ".js", ".jsx", ".json"]);
                break;
            case "javascript":
                fileTypes.AddRange([".js", ".jsx", ".json"]);
                break;
            case "python":
                fileTypes.AddRange([".py", ".yml", ".yaml"]);
                break;
            case "java":
                fileTypes.AddRange([".java", ".xml"]);
                break;
            case "go":
                fileTypes.AddRange([".go", ".mod"]);
                break;
            default:
                // Generic defaults for unknown languages
                fileTypes.AddRange([".json", ".yml", ".yaml"]);
                break;
        }
        
        return fileTypes.Distinct().ToArray();
    }

    private bool ShouldIndexIssues(RepositoryInfo repoInfo)
    {
        // Index issues for most repositories, but skip for documentation-only repos
        return !repoInfo.Name.Contains("doc", StringComparison.OrdinalIgnoreCase) &&
               !repoInfo.Description?.Contains("documentation", StringComparison.OrdinalIgnoreCase) == true;
    }

    private bool ShouldIndexPullRequests(RepositoryInfo repoInfo)
    {
        // Similar logic to issues
        return ShouldIndexIssues(repoInfo);
    }

    private async Task<IEnumerable<Document>> LoadRepositoryContents(
        GitHubClient client, 
        string owner, 
        string repo, 
        string[] fileExtensions,
        CancellationToken cancellationToken)
    {
        var documents = new List<Document>();
        
        try
        {
            // Get all contents recursively (use null for root directory)
            var allContents = await GetAllContentsRecursively(client, owner, repo, null, cancellationToken);
            
            foreach (var content in allContents)
            {
                if (cancellationToken.IsCancellationRequested) break;
                
                // Skip if not a relevant file type
                if (!fileExtensions.Any(ext => content.Name.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                    continue;
                
                // Skip if in excluded path
                if (_options.DefaultExcludePaths.Any(exclude => content.Path.StartsWith(exclude, StringComparison.OrdinalIgnoreCase)))
                    continue;
                
                // Skip if file is too large
                if (content.Size > _options.MaxFileSize)
                {
                    _logger.LogDebug("Skipping large file: {Path} ({Size} bytes)", content.Path, content.Size);
                    continue;
                }
                
                try
                {
                    var fileContent = await client.Repository.Content.GetRawContent(owner, repo, content.Path);
                    var textContent = Encoding.UTF8.GetString(fileContent);
                    
                    // Create document chunks for large files
                    var chunks = ChunkText(textContent, _options.ChunkSize, _options.ChunkOverlap);
                    
                    for (int i = 0; i < chunks.Length; i++)
                    {
                        var document = new Document
                        {
                            Id = $"{owner}/{repo}/{content.Path}#{i}",
                            Content = chunks[i],
                            Title = content.Name + (chunks.Length > 1 ? $" (Part {i + 1})" : ""),
                            Type = DetermineDocumentType(content.Name, textContent),
                            SourceId = $"{owner}/{repo}",
                            SourceName = $"{owner}/{repo}",
                            Url = content.HtmlUrl,
                            Path = content.Path,
                            LastModified = DateTime.UtcNow, // GitHub API doesn't provide file-level dates easily
                            Metadata = new Dictionary<string, object>
                            {
                                ["fileSize"] = content.Size,
                                ["chunkIndex"] = i,
                                ["totalChunks"] = chunks.Length,
                                ["language"] = DetectLanguage(content.Name)
                            }
                        };
                        
                        documents.Add(document);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Error loading file {Path}: {Error}", content.Path, ex.Message);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading repository contents for {Owner}/{Repo}", owner, repo);
        }
        
        return documents;
    }

    private async Task<IEnumerable<RepositoryContent>> GetAllContentsRecursively(
        GitHubClient client, 
        string owner, 
        string repo, 
        string? path,
        CancellationToken cancellationToken)
    {
        var allContents = new List<RepositoryContent>();
        
        try
        {
            var contents = await client.Repository.Content.GetAllContents(owner, repo, path);
            
            foreach (var content in contents)
            {
                if (cancellationToken.IsCancellationRequested) break;
                
                if (content.Type == ContentType.File)
                {
                    allContents.Add(content);
                }
                else if (content.Type == ContentType.Dir)
                {
                    // Recursively get directory contents
                    var subContents = await GetAllContentsRecursively(client, owner, repo, content.Path, cancellationToken);
                    allContents.AddRange(subContents);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Error getting contents for path {Path}: {Error}", path, ex.Message);
        }
        
        return allContents;
    }

    private async Task<IEnumerable<Document>> LoadRepositoryIssues(
        GitHubClient client, 
        string owner, 
        string repo,
        CancellationToken cancellationToken)
    {
        var documents = new List<Document>();
        
        try
        {
            var request = new RepositoryIssueRequest
            {
                State = ItemStateFilter.All,
                Filter = IssueFilter.All
            };
            
            var issues = await client.Issue.GetAllForRepository(owner, repo, request);
            
            foreach (var issue in issues.Take(50)) // Limit to recent issues
            {
                if (cancellationToken.IsCancellationRequested) break;
                
                var content = $"# {issue.Title}\n\n{issue.Body}";
                
                var document = new Document
                {
                    Id = $"{owner}/{repo}/issues/{issue.Number}",
                    Content = content,
                    Title = $"Issue #{issue.Number}: {issue.Title}",
                    Type = DocumentType.Issue,
                    SourceId = $"{owner}/{repo}",
                    SourceName = $"{owner}/{repo}",
                    Url = issue.HtmlUrl,
                    Path = $"issues/{issue.Number}",
                    LastModified = issue.UpdatedAt?.DateTime ?? issue.CreatedAt.DateTime,
                    Metadata = new Dictionary<string, object>
                    {
                        ["issueNumber"] = issue.Number,
                        ["state"] = issue.State.ToString(),
                        ["author"] = issue.User.Login,
                        ["labels"] = issue.Labels.Select(l => l.Name).ToArray()
                    }
                };
                
                documents.Add(document);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Error loading issues for {Owner}/{Repo}: {Error}", owner, repo, ex.Message);
        }
        
        return documents;
    }

    private async Task<IEnumerable<Document>> LoadRepositoryPullRequests(
        GitHubClient client, 
        string owner, 
        string repo,
        CancellationToken cancellationToken)
    {
        var documents = new List<Document>();
        
        try
        {
            var request = new PullRequestRequest
            {
                State = ItemStateFilter.All
            };
            
            var pullRequests = await client.PullRequest.GetAllForRepository(owner, repo, request);
            
            foreach (var pr in pullRequests.Take(30)) // Limit to recent PRs
            {
                if (cancellationToken.IsCancellationRequested) break;
                
                var content = $"# {pr.Title}\n\n{pr.Body}";
                
                var document = new Document
                {
                    Id = $"{owner}/{repo}/pulls/{pr.Number}",
                    Content = content,
                    Title = $"PR #{pr.Number}: {pr.Title}",
                    Type = DocumentType.PullRequest,
                    SourceId = $"{owner}/{repo}",
                    SourceName = $"{owner}/{repo}",
                    Url = pr.HtmlUrl,
                    Path = $"pulls/{pr.Number}",
                    LastModified = pr.UpdatedAt.DateTime,
                    Metadata = new Dictionary<string, object>
                    {
                        ["prNumber"] = pr.Number,
                        ["state"] = pr.State.ToString(),
                        ["author"] = pr.User.Login,
                        ["merged"] = pr.Merged
                    }
                };
                
                documents.Add(document);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Error loading pull requests for {Owner}/{Repo}: {Error}", owner, repo, ex.Message);
        }
        
        return documents;
    }

    private string[] ChunkText(string text, int chunkSize, int overlap)
    {
        if (text.Length <= chunkSize)
            return [text];
        
        var chunks = new List<string>();
        var start = 0;
        
        while (start < text.Length)
        {
            var end = Math.Min(start + chunkSize, text.Length);
            var chunk = text.Substring(start, end - start);
            chunks.Add(chunk);
            
            start = end - overlap;
            if (start >= text.Length) break;
        }
        
        return chunks.ToArray();
    }

    private DocumentType DetermineDocumentType(string fileName, string content)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        
        return extension switch
        {
            ".md" or ".txt" or ".rst" or ".adoc" => DocumentType.Documentation,
            ".json" or ".yml" or ".yaml" or ".xml" or ".toml" => DocumentType.Configuration,
            _ => DocumentType.Code
        };
    }

    private string DetectLanguage(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        
        return extension switch
        {
            ".cs" => "csharp",
            ".js" => "javascript",
            ".ts" => "typescript",
            ".py" => "python",
            ".java" => "java",
            ".go" => "go",
            ".rs" => "rust",
            ".cpp" or ".cc" => "cpp",
            ".c" => "c",
            ".h" => "c",
            ".md" => "markdown",
            ".json" => "json",
            ".yml" or ".yaml" => "yaml",
            ".xml" => "xml",
            _ => "text"
        };
    }

    private class RepositoryInfo
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? DefaultBranch { get; set; }
        public string? Language { get; set; }
        public int Size { get; set; }
    }
} 