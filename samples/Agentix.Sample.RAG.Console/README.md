# Agentix RAG Console Sample

This sample demonstrates how to build an AI assistant that can search and understand GitHub repositories using Agentix.Net's RAG (Retrieval Augmented Generation) capabilities with **zero-cost local embeddings**.

## What This Sample Shows

- ✅ Simple GitHub repository integration (3 lines of config!)
- ✅ AI-powered code search across multiple repositories  
- ✅ Natural language queries about your codebase
- ✅ **Zero-cost local embeddings** (no external embedding API required!)
- ✅ Conversation context with repository results
- ✅ Configuration from files, environment, or command line
- ✅ Real-world error handling and setup validation

## Quick Start

### 1. Get API Keys

You'll need **only two** API keys:

**🤖 Claude API Key**
- Get from: [Anthropic Console](https://console.anthropic.com/)
- Used for: AI conversations and code understanding

**🐙 GitHub Token**
- Get from: [GitHub Settings](https://github.com/settings/tokens)
- Scopes needed: `repo` (for private repos) or `public_repo` (for public repos)
- Used for: Accessing repository content

**✨ No OpenAI API Key Required!**
- Uses local ONNX models for embeddings
- Zero ongoing costs for text embeddings
- Works completely offline after initial model download

### 2. Run the Sample

**Option A: Environment Variables (Recommended)**
```bash
set CLAUDE_API_KEY=your-claude-api-key
set GITHUB_TOKEN=ghp_your-github-token
dotnet run
```

**Option B: Command Line**
```bash
dotnet run --claude-key your-claude-key --github-token ghp_your-token --repo https://github.com/your-org/repo
```

**Option C: Configuration File**
Edit `appsettings.json` with your keys and repositories, then:
```bash
dotnet run
```

### 3. Try These Queries

Once running, try asking:
- `"How do we handle authentication in our API?"`
- `"Show me error handling patterns"`
- `"Find examples of unit tests"`
- `"What logging frameworks do we use?"`
- `"How is dependency injection configured?"`
- `"Find database connection code"`

## Configuration

### appsettings.json
```json
{
  "Claude": {
    "ApiKey": "your-claude-api-key"
  },
  "GitHub": {
    "Token": "ghp_your-github-token",
    "Repositories": [
      "https://github.com/your-org/backend-api",
      "https://github.com/your-org/frontend-app",
      "https://github.com/your-org/documentation"
    ]
  }
}
```

### Environment Variables
```bash
# Required for AI conversations
CLAUDE_API_KEY=your-claude-api-key

# Required for repository access
GITHUB_TOKEN=ghp_your-github-token

# No OpenAI key needed - local embeddings!
```

### Command Line Arguments
```bash
dotnet run \
  --claude-key your-claude-key \
  --github-token ghp_your-token \
  --repo https://github.com/org/repo1 \
  --repo https://github.com/org/repo2
```

## How It Works

1. **🔄 Repository Indexing**: System automatically downloads and processes repository content
2. **🧠 Smart Analysis**: Auto-detects file types and excludes irrelevant directories
3. **🤖 Local Embeddings**: Creates vector embeddings using local ONNX models (zero cost!)
4. **💾 In-Memory Storage**: Stores embeddings for fast searching (no external dependencies)
5. **🔍 Semantic Search**: AI uses `github_search` tool to find relevant code
6. **💬 Natural Conversation**: Maintains context across questions for follow-up queries

### Local Embeddings Benefits
- ✅ **Zero ongoing costs** - no API calls after model download
- ✅ **Privacy-first** - all processing stays local
- ✅ **No rate limits** - process unlimited text
- ✅ **Offline capable** - works without internet after setup
- ✅ **Fast** - ~10-50ms per embedding on modern hardware

## What Gets Indexed

The system automatically indexes relevant content:

### Code Files
Based on repository's primary language:
- **C#**: `.cs`, `.csproj`, `.sln`, `.json`
- **TypeScript**: `.ts`, `.tsx`, `.js`, `.jsx`, `.json`
- **Python**: `.py`, `.yml`, `.yaml`
- **Java**: `.java`, `.xml`
- **Go**: `.go`, `.mod`

### Documentation
Always included:
- `.md`, `.txt`, `.rst`, `.adoc` files
- README files, documentation directories

### Issues & Pull Requests
- Recent issues and pull requests for context
- Automatically excluded for documentation-only repositories

### Automatic Exclusions
- `node_modules/`, `bin/`, `obj/`, `.git/`, `dist/`, `build/`
- Files larger than 50KB
- Binary files and generated content

## Example Conversations

### Finding Authentication Code
```
You: How do we handle authentication in our API?

AI: I'll search your repositories for authentication-related code.

[AI automatically uses github_search tool]

Based on your codebase, here's how authentication is handled:

In your backend-api repository, I found the main authentication logic in 
`Controllers/AuthController.cs`. The system uses JWT tokens with these key components:

1. **Login Endpoint** (`/api/auth/login`)
   - Validates user credentials against the database
   - Generates JWT tokens with user claims
   - Returns access and refresh tokens

2. **JWT Middleware** (`Middleware/JwtAuthenticationMiddleware.cs`)
   - Validates tokens on protected routes
   - Extracts user claims for authorization

3. **Token Service** (`Services/TokenService.cs`)
   - Handles token generation and validation
   - Configures expiration times and signing keys

[View AuthController.cs on GitHub](https://github.com/your-org/backend-api/blob/main/Controllers/AuthController.cs)
```

### Follow-up Questions
```
You: Are there any security issues with that JWT implementation?

AI: [References previous search results from conversation context]

Looking at the JWT implementation I found earlier, here are some security considerations:

1. **Token Expiration**: The tokens are set to expire after 1 hour, which is good
2. **Refresh Token Rotation**: The system properly rotates refresh tokens
3. **Secure Storage**: Tokens are stored as HTTP-only cookies

However, I notice one potential improvement in the TokenService.cs...
[Continues with specific analysis]
```

## Troubleshooting

### Common Issues

**"Repository not found or access denied"**
- ✅ Verify GitHub token has access to the repository
- ✅ For private repos, ensure token has `repo` scope
- ✅ Check repository URL format

**"Model download failed"**
- ✅ Ensure internet connectivity for initial model download (~25MB)
- ✅ Check disk space (models cached in `~/.agentix/models/`)
- ✅ Verify no firewall blocking HTTPS to huggingface.co

**"No relevant results found"**
- ⏳ Repository might still be indexing (check logs)
- 🔍 Try different search terms or more specific queries
- 📁 Verify repository has supported file types

**"Repository indexing failed"**
- 🔑 Check GitHub token permissions
- 📊 Monitor rate limits in logs
- 🌐 Ensure internet connectivity

### Performance Notes

- **First Run**: Initial setup downloads embedding model (~25MB, one-time)
- **Model Loading**: ~2-3 seconds on first embedding generation (then cached)
- **Memory Usage**: ~150MB for embedding model + 6KB per document chunk
- **Embedding Speed**: Typically 10-50ms per text chunk
- **Search Speed**: Typically 100-500ms for most queries
- **Costs**: Zero ongoing costs after initial setup!

## Next Steps

### Extending the Sample

1. **Add More Repositories**: Simply add URLs to configuration
2. **Custom System Prompt**: Modify the prompt for domain-specific assistance
3. **Different AI Provider**: Try with OpenAI instead of Claude
4. **Production Deployment**: Use Redis vector store for scalability

### Integration Ideas

1. **Slack Bot**: Use `Agentix.Channels.Slack` for team access
2. **Web Interface**: Add `Agentix.Channels.WebApi` for web UI
3. **CI/CD Integration**: Query codebase from build scripts
4. **Documentation Generator**: Auto-generate docs from code understanding

## Architecture

This sample demonstrates the full Agentix.Net stack with local embeddings:

```
User Query → Console Channel → Agentix Core → Claude Provider
                ↓
GitHub RAG Tool → RAG Engine → GitHub API + Local ONNX Embeddings
                ↓
Vector Search → Document Results → Formatted Response
```

## Local Embedding Details

### Models Used
- **Default**: all-MiniLM-L6-v2 (23MB, 384 dimensions)
- **High Quality**: all-mpnet-base-v2 (438MB, 768 dimensions)

### Model Management
- **Cache Location**: `%USERPROFILE%\.agentix\models\` (Windows) or `~/.agentix/models/` (Linux/Mac)
- **Auto-Download**: Models download automatically on first use
- **Offline Operation**: Works completely offline after initial download

### Performance Benchmarks
| Operation | Time | Memory |
|-----------|------|---------|
| Model load | ~2-3s | ~150MB |
| Single embedding | ~10-50ms | - |
| Batch (10 texts) | ~100-200ms | - |

## Code Structure

```
Program.cs              # Main application setup and configuration
appsettings.json        # Default configuration template
appsettings.Development.json # Development overrides
README.md               # This file - setup and usage guide
```

## Dependencies

This sample uses these Agentix.Net packages:
- `Agentix.Core` - Core framework
- `Agentix.Providers.Claude` - Claude AI integration
- `Agentix.Channels.Console` - Console interface
- `Agentix.Context.InMemory` - Conversation memory
- `Agentix.Rag.GitHub` - GitHub repository search
- `Agentix.Rag.Embeddings.Local` - Local ONNX embeddings (zero cost!)
- `Agentix.Rag.InMemory` - In-memory vector store

## License

This sample is part of the Agentix.Net project and is licensed under the MIT License. 