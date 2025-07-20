# Contributing to Agentix.Net

Thank you for your interest in contributing to Agentix.Net! This guide will help you get started with development and contributions.

## Development Setup

### Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022 or VS Code
- Git

### Getting Started

1. Fork the repository
2. Clone your fork:
   ```bash
   git clone https://github.com/yourusername/Agentix.Net.git
   cd Agentix.Net
   ```
3. Build the solution:
   ```bash
   dotnet build
   ```
4. Run tests:
   ```bash
   dotnet test
   ```

## Project Structure

```
Agentix.Net/
├── src/                          # Framework source code
│   ├── Core/
│   │   └── Agentix.Core/         # Core abstractions and interfaces
│   ├── Channels/                 # Communication channel adapters
│   │   └── Agentix.Channels.Console/
│   └── Providers/                # AI provider implementations
│       └── Agentix.Providers.Claude/
├── samples/                      # Sample applications
│   └── Agentix.Sample.Console/   # Console sample app
├── docs/                         # Documentation
└── tests/                        # Unit and integration tests
```

## Adding New Components

### Adding a Channel Adapter

1. Create a new project in `src/Channels/`:
   ```
   src/Channels/Agentix.Channels.YourChannel/
   ```
2. Implement the `IChannelAdapter` interface
3. Add project to solution file
4. Add reference to `Agentix.Core`
5. Create unit tests
6. Add sample usage to samples directory

**Example Channel Structure:**
```
src/Channels/Agentix.Channels.Slack/
├── Agentix.Channels.Slack.csproj
├── SlackChannelAdapter.cs
├── Models/
│   ├── SlackMessage.cs
│   └── SlackOptions.cs
├── Extensions/
│   └── ServiceCollectionExtensions.cs
└── README.md
```

**Required Implementation:**
```csharp
public class SlackChannelAdapter : IChannelAdapter
{
    public string Name => "slack";
    public string ChannelType => "slack";
    public bool IsRunning { get; private set; }
    
    public bool SupportsRichContent => true;
    public bool SupportsFileUploads => true;
    public bool SupportsInteractiveElements => true;
    
    // Implement all interface methods...
}
```

### Adding an AI Provider

1. Create a new project in `src/Providers/`:
   ```
   src/Providers/Agentix.Providers.YourProvider/
   ```
2. Implement the `IAIProvider` interface
3. Add project to solution file
4. Add reference to `Agentix.Core`
5. Create unit tests
6. Add sample usage

**Example Provider Structure:**
```
src/Providers/Agentix.Providers.OpenAI/
├── Agentix.Providers.OpenAI.csproj
├── OpenAIProvider.cs
├── Models/
│   ├── OpenAIRequest.cs
│   ├── OpenAIResponse.cs
│   └── OpenAIOptions.cs
├── Extensions/
│   └── ServiceCollectionExtensions.cs
└── README.md
```

**Required Implementation:**
```csharp
public class OpenAIProvider : IAIProvider
{
    public string Name => "openai";
    public AICapabilities Capabilities { get; private set; }
    
    public async Task<AIResponse> GenerateAsync(AIRequest request, CancellationToken cancellationToken = default)
    {
        // Implementation here...
    }
    
    public async Task<decimal> EstimateCostAsync(AIRequest request)
    {
        // Cost calculation based on token count and model pricing
    }
    
    public async Task<bool> HealthCheckAsync(CancellationToken cancellationToken = default)
    {
        // Provider health check
    }
}
```

### Service Registration Pattern

Each component should include an extension method for easy registration:

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOpenAIProvider(this IServiceCollection services, 
        Action<OpenAIOptions> configure)
    {
        var options = new OpenAIOptions();
        configure(options);
        
        services.AddSingleton(options);
        services.AddSingleton<IAIProvider, OpenAIProvider>();
        
        return services;
    }
}
```

## Code Standards

### Coding Style

- Follow Microsoft's C# coding conventions
- Use meaningful variable and method names
- Include XML documentation for public APIs
- Use async/await for asynchronous operations

### Testing

- Write unit tests for all public APIs
- Use integration tests for end-to-end scenarios
- Aim for >80% code coverage
- Mock external dependencies

### Documentation

- Update README files when adding new components
- Include code examples in documentation
- Document configuration options
- Add inline code comments for complex logic

## Pull Request Process

1. Create a feature branch from `main`
2. Make your changes following the coding standards
3. Add or update tests as needed
4. Update documentation
5. Ensure all tests pass
6. Submit a pull request with:
   - Clear description of changes
   - Link to any related issues
   - Screenshots/examples if applicable

### PR Checklist

- [ ] Code follows established patterns
- [ ] Tests added/updated and passing
- [ ] Documentation updated
- [ ] No breaking changes (or properly documented)
- [ ] Solution builds without warnings

## Issue Guidelines

When creating issues:

- Use clear, descriptive titles
- Provide steps to reproduce for bugs
- Include environment details (OS, .NET version, etc.)
- Use appropriate labels

### Issue Templates

**Bug Report:**
- Description of the issue
- Steps to reproduce
- Expected vs actual behavior
- Environment details

**Feature Request:**
- Description of the feature
- Use case/justification
- Proposed implementation approach

## Release Process

1. Update version numbers in project files
2. Update CHANGELOG.md
3. Create release notes
4. Tag the release
5. Publish NuGet packages

## Getting Help

- Check existing issues and discussions
- Review the design document in `docs/`
- Ask questions in GitHub Discussions
- Reach out to maintainers for guidance

## Code of Conduct

Be respectful, inclusive, and professional in all interactions. We're building this together!

## What We're Looking For

Priority areas for contributions:

- **New Providers**: OpenAI, Azure OpenAI, Google Gemini, etc.
- **New Channels**: Slack, Teams, WebAPI, Discord
- **Enhanced Features**: Context memory, tool calling, RAG integration
- **Documentation**: Examples, tutorials, API docs
- **Testing**: Unit tests, integration tests, performance tests

Thank you for contributing to Agentix.Net! 