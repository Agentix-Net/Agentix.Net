# Agentix AI Providers

This directory contains AI provider implementations that integrate different AI services and models with the Agentix framework.

## Existing Providers

- **Agentix.Providers.Claude** - Anthropic Claude API integration

## Adding a New Provider

To add a new AI provider (e.g., OpenAI, Azure OpenAI, Google Gemini):

1. Create a new project directory following the naming convention: `Agentix.Providers.{ProviderName}`
2. Implement the `IAIProvider` interface from `Agentix.Core`
3. Add your project to the solution file
4. Add project reference to `Agentix.Core`
5. Include necessary NuGet packages for the AI service

### Example Project Structure

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

### Example Implementation

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

Each provider should include an extension method for easy registration:

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

## Planned Providers

- **Agentix.Providers.OpenAI** - OpenAI GPT models (GPT-4, GPT-3.5, etc.)
- **Agentix.Providers.AzureOpenAI** - Azure OpenAI Service integration
- **Agentix.Providers.GoogleGemini** - Google Gemini API integration
- **Agentix.Providers.Ollama** - Local model hosting with Ollama
- **Agentix.Providers.HuggingFace** - Hugging Face model integration
- **Agentix.Providers.Cohere** - Cohere API integration 