using System.Text.Json;
using System.Text.Json.Nodes;
using Agentix.Core.Models;
using Agentix.Rag.Core.Interfaces;
using Agentix.Rag.Core.Models;
using Microsoft.Extensions.Logging;

namespace Agentix.Rag.GitHub;

/// <summary>
/// Tool for searching GitHub repositories using natural language queries.
/// Integrates with the Agentix RAG engine to provide AI-powered code search.
/// </summary>
public class GitHubSearchTool : ITool
{
    private readonly IRAGEngine _ragEngine;
    private readonly ILogger<GitHubSearchTool> _logger;

    public string Name => "github_search";
    public string Description => "Search GitHub repositories for code, documentation, and issues using natural language queries";
    public string Category => "Development";

    public GitHubSearchTool(IRAGEngine ragEngine, ILogger<GitHubSearchTool> logger)
    {
        _ragEngine = ragEngine;
        _logger = logger;
    }

    public async Task<ToolResult> ExecuteAsync(ToolCall toolCall, CancellationToken cancellationToken = default)
    {
        try
        {
            // Extract parameters
            var query = toolCall.Parameters["query"]?.ToString();
            if (string.IsNullOrWhiteSpace(query))
            {
                return new ToolResult
                {
                    ToolCallId = toolCall.Id,
                    ToolName = Name,
                    Success = false,
                    ErrorMessage = "Query parameter is required and cannot be empty"
                };
            }

            var maxResults = 5;
            if (toolCall.Parameters["max_results"] is JsonNode maxResultsNode)
            {
                if (maxResultsNode.AsValue().TryGetValue<int>(out var parsedMaxResults))
                {
                    maxResults = Math.Clamp(parsedMaxResults, 1, 20);
                }
            }

            _logger.LogInformation("Searching repositories for: '{Query}' (max results: {MaxResults})", query, maxResults);

            // Check if RAG engine is ready
            var isReady = await _ragEngine.IsReadyAsync();
            if (!isReady)
            {
                _logger.LogWarning("RAG engine not ready for search");
                return new ToolResult
                {
                    ToolCallId = toolCall.Id,
                    ToolName = Name,
                    Success = false,
                    ErrorMessage = "Repository indexing is still in progress. Please try again in a few moments."
                };
            }

            // Perform the search
            var searchResult = await _ragEngine.SearchAsync(query, maxResults, cancellationToken);

            if (searchResult.Documents.Length == 0)
            {
                _logger.LogInformation("No results found for query: '{Query}'", query);
                return new ToolResult
                {
                    ToolCallId = toolCall.Id,
                    ToolName = Name,
                    Success = true,
                    Message = $"No relevant results found for '{query}'. Try a different search term or check if repositories are properly indexed.",
                    Data = new
                    {
                        query = query,
                        results = Array.Empty<object>(),
                        total_results = 0,
                        query_time_ms = (int)searchResult.QueryTime.TotalMilliseconds
                    }
                };
            }

            // Format results for AI consumption
            var formattedResults = searchResult.Documents.Select(doc => new
            {
                title = doc.Title,
                repository = doc.Repository,
                file_path = doc.FilePath,
                content_preview = doc.Content,
                similarity_score = Math.Round(doc.Similarity, 3),
                document_type = doc.Type.ToString().ToLowerInvariant(),
                url = doc.Url,
                last_modified = doc.LastModified.ToString("yyyy-MM-dd")
            }).ToArray();

            // Create success message
            var message = CreateSuccessMessage(query, searchResult);

            _logger.LogInformation("Found {Count} results for query: '{Query}' in {Duration}ms", 
                                 searchResult.Documents.Length, query, searchResult.QueryTime.TotalMilliseconds);

            return new ToolResult
            {
                ToolCallId = toolCall.Id,
                ToolName = Name,
                Success = true,
                Message = message,
                Data = new
                {
                    query = query,
                    results = formattedResults,
                    total_results = searchResult.TotalResults,
                    query_time_ms = (int)searchResult.QueryTime.TotalMilliseconds,
                    repositories_searched = formattedResults.Select(r => r.repository).Distinct().ToArray()
                }
            };
        }
        catch (Exception ex)
        {
            var queryForLog = toolCall.Parameters["query"]?.AsValue().GetValue<string>() ?? "unknown";
            _logger.LogError(ex, "Error executing GitHub search for query: '{Query}'", queryForLog);

            return new ToolResult
            {
                ToolCallId = toolCall.Id,
                ToolName = Name,
                Success = false,
                ErrorMessage = $"Search failed: {ex.Message}"
            };
        }
    }

    private static string CreateSuccessMessage(string query, RAGResult searchResult)
    {
        var results = searchResult.Documents;
        var repositories = results.Select(r => r.Repository).Distinct().ToArray();

        var message = $"Found {results.Length} relevant result{(results.Length == 1 ? "" : "s")} for '{query}'";
        
        if (repositories.Length > 1)
        {
            message += $" across {repositories.Length} repositories ({string.Join(", ", repositories)})";
        }
        else if (repositories.Length == 1)
        {
            message += $" in {repositories[0]}";
        }

        message += ":";

        // Add top results summary
        var topResults = results.Take(3);
        foreach (var result in topResults)
        {
            var relevanceDesc = result.Similarity switch
            {
                >= 0.9 => "Highly relevant",
                >= 0.8 => "Very relevant", 
                >= 0.7 => "Relevant",
                _ => "Potentially relevant"
            };

            message += $"\n\n• **{result.Title}** ({relevanceDesc})\n";
            message += $"  Repository: {result.Repository}\n";
            message += $"  Path: {result.FilePath}\n";
            
            if (!string.IsNullOrEmpty(result.Url))
            {
                message += $"  URL: {result.Url}\n";
            }
            
            // Add content preview (first 200 chars)
            var preview = result.Content.Length > 200 
                ? result.Content.Substring(0, 200).Trim() + "..."
                : result.Content.Trim();
            
            message += $"  Preview: {preview}";
        }

        if (results.Length > 3)
        {
            message += $"\n\n... and {results.Length - 3} more result{(results.Length - 3 == 1 ? "" : "s")}";
        }

        return message;
    }
} 