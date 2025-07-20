# Agentix Framework - Comprehensive Design Document

**Version**: 1.0  
**Date**: January 2025  
**Authors**: Development Team  
**Status**: Design Phase  

---

## 📋 Table of Contents

1. [Executive Summary](#executive-summary)
2. [Project Overview](#project-overview)
3. [Architecture Overview](#architecture-overview)
4. [Core Framework Design](#core-framework-design)
5. [Context Memory System](#context-memory-system)
6. [AI Provider Abstraction](#ai-provider-abstraction)
7. [Tool System Design](#tool-system-design)
8. [Channel Integration Layer](#channel-integration-layer)
9. [RAG Engine Architecture](#rag-engine-architecture)
10. [Security & Compliance](#security--compliance)
11. [Development Roadmap](#development-roadmap)
12. [Implementation Guidelines](#implementation-guidelines)
13. [Deployment Architecture](#deployment-architecture)
14. [Testing Strategy](#testing-strategy)
15. [Documentation Requirements](#documentation-requirements)

---

## Executive Summary

### Project Vision
Agentix is a modular .NET Core framework that democratizes AI integration for enterprise applications. It provides a unified interface for multiple AI providers, intelligent tool execution, and multi-channel communication, delivered as composable NuGet packages.

### Business Goals
- **Open Source Strategy**: Build community and validate market demand
- **SaaS Transition**: Leverage open source adoption for commercial SaaS offering
- **Enterprise Focus**: Target .NET-heavy enterprises with compliance and security needs
- **Developer Experience**: Make AI integration as simple as adding packages and configuration

### Technical Goals
- **Modular Architecture**: Pay-as-you-grow package model
- **Production Ready**: Enterprise-grade security, monitoring, and scalability
- **Developer Friendly**: Native .NET patterns, dependency injection, configuration
- **Channel Agnostic**: Work across Slack, Teams, Web, API, and custom channels

---

## Project Overview

### Market Opportunity
- **Target Market**: .NET enterprise developers and companies
- **Market Gap**: No comprehensive .NET-native AI framework exists
- **Competitive Advantage**: First-mover in .NET AI space, modular architecture
- **Revenue Model**: Open source → Community → SaaS → Enterprise

### Key Differentiators
1. **Native .NET Integration**: Built for .NET ecosystem, not ported from Python
2. **Modular Package System**: Start minimal, add capabilities as needed
3. **Enterprise Security**: Built-in compliance, audit trails, access controls
4. **Context Memory**: Intelligent conversation management across channels
5. **Tool Ecosystem**: Extensible tool system with built-in security

---

## Architecture Overview

### High-Level Architecture
```
┌─────────────────────────────────────────────────────────────────┐
│                    Client Applications                          │
├─────────────────────────────────────────────────────────────────┤
│  Slack Bot  │  Teams App  │  Web API  │  Console  │  Custom     │
├─────────────────────────────────────────────────────────────────┤
│                  Channel Adaptation Layer                       │
├─────────────────────────────────────────────────────────────────┤
│                     Agentix Core Engine                         │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ │
│  │   Context   │ │    Tool     │ │    RAG      │ │ Orchestration│ │
│  │   Manager   │ │  Executor   │ │   Engine    │ │   Engine    │ │
│  └─────────────┘ └─────────────┘ └─────────────┘ └─────────────┘ │
├─────────────────────────────────────────────────────────────────┤
│                   AI Provider Abstraction                       │
├─────────────────────────────────────────────────────────────────┤
│  OpenAI  │  Claude  │  Azure OpenAI  │  Gemini  │  Local Models │
├─────────────────────────────────────────────────────────────────┤
│                     Infrastructure Layer                        │
│  Redis  │  Database  │  Vector Store  │  Monitoring  │  Security │
└─────────────────────────────────────────────────────────────────┘
```

### Package Structure
```
Agentix.Core                    # Base abstractions and interfaces
├── Agentix.Configuration       # Configuration management
├── Agentix.Context            # Context memory system
│
├── Agentix.Providers.*        # AI Provider packages
│   ├── Agentix.Providers.OpenAI
│   ├── Agentix.Providers.Claude
│   ├── Agentix.Providers.AzureOpenAI
│   ├── Agentix.Providers.GoogleGemini
│   └── Agentix.Providers.Ollama
│
├── Agentix.Tools.*            # Tool packages
│   ├── Agentix.Tools.Core
│   ├── Agentix.Tools.Web
│   ├── Agentix.Tools.Database
│   ├── Agentix.Tools.GitHub
│   ├── Agentix.Tools.Jira
│   └── Agentix.Tools.Email
│
├── Agentix.Channels.*         # Channel integration packages
│   ├── Agentix.Channels.Slack
│   ├── Agentix.Channels.Teams
│   ├── Agentix.Channels.WebApi
│   └── Agentix.Channels.Console
│
├── Agentix.Rag.*             # RAG engine packages
│   ├── Agentix.Rag.Core
│   ├── Agentix.Rag.Pinecone
│   ├── Agentix.Rag.GitHub
│   └── Agentix.Rag.Confluence
│
└── Agentix.Advanced.*        # Advanced feature packages
    ├── Agentix.Orchestration
    ├── Agentix.Analytics
    └── Agentix.Security
```

---

## Core Framework Design

### Core Interfaces

#### IAIProvider Interface
```csharp
public interface IAIProvider
{
    string Name { get; }
    AICapabilities Capabilities { get; }
    
    Task<AIResponse> GenerateAsync(AIRequest request, CancellationToken cancellationToken = default);
    Task<AIResponse> GenerateWithToolsAsync(AIRequest request, IEnumerable<ITool> availableTools, CancellationToken cancellationToken = default);
    Task<AIResponse> GenerateWithContextAsync(AIRequest request, IConversationContext context, CancellationToken cancellationToken = default);
    
    Task<decimal> EstimateCostAsync(AIRequest request);
    Task<bool> HealthCheckAsync(CancellationToken cancellationToken = default);
}

public class AICapabilities
{
    public bool SupportsFunctionCalling { get; set; }
    public bool SupportsStreaming { get; set; }
    public bool SupportsVision { get; set; }
    public int MaxTokens { get; set; }
    public int ContextWindow { get; set; }
    public decimal CostPerInputToken { get; set; }
    public decimal CostPerOutputToken { get; set; }
    public IEnumerable<string> SupportedModels { get; set; } = new List<string>();
}
```

#### ITool Interface
```csharp
public interface ITool
{
    string Name { get; }
    string Description { get; }
    string Category { get; }
    JsonSchema Parameters { get; }
    
    Task<ToolResult> ExecuteAsync(ToolCall toolCall, CancellationToken cancellationToken = default);
    
    // Security and permissions
    bool RequiresAuth { get; }
    IEnumerable<string> RequiredScopes { get; }
    IEnumerable<string> AllowedRoles { get; }
    
    // Tool metadata
    TimeSpan EstimatedExecutionTime { get; }
    bool IsLongRunning { get; }
    int Priority { get; }
}

public abstract class AgentixTool : ITool
{
    public abstract string Name { get; }
    public abstract string Description { get; }
    public virtual string Category => "General";
    public abstract JsonSchema Parameters { get; }
    
    public virtual bool RequiresAuth => false;
    public virtual IEnumerable<string> RequiredScopes => Enumerable.Empty<string>();
    public virtual IEnumerable<string> AllowedRoles => new[] { "*" };
    
    public virtual TimeSpan EstimatedExecutionTime => TimeSpan.FromSeconds(5);
    public virtual bool IsLongRunning => EstimatedExecutionTime > TimeSpan.FromSeconds(30);
    public virtual int Priority => 0;
    
    public abstract Task<ToolResult> ExecuteAsync(ToolCall toolCall, CancellationToken cancellationToken);
    
    protected virtual void ValidateParameters(ToolCall toolCall)
    {
        // Base parameter validation logic
    }
}
```

#### IChannelAdapter Interface
```csharp
public interface IChannelAdapter
{
    string Name { get; }
    string ChannelType { get; }
    bool IsRunning { get; }
    
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
    
    Task<bool> CanHandleAsync(IncomingMessage message);
    Task<ChannelResponse> ProcessAsync(IncomingMessage message, CancellationToken cancellationToken = default);
    Task SendResponseAsync(AIResponse response, MessageContext context, CancellationToken cancellationToken = default);
    
    // Channel-specific capabilities
    bool SupportsRichContent { get; }
    bool SupportsFileUploads { get; }
    bool SupportsInteractiveElements { get; }
}
```

### Core Models

#### Message Models
```csharp
public class IncomingMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Content { get; set; }
    public string UserId { get; set; }
    public string UserName { get; set; }
    public string ChannelId { get; set; }
    public string ChannelName { get; set; }
    public string Channel { get; set; } // "slack", "teams", "webapi", etc.
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public MessageType Type { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
    
    // File attachments
    public IEnumerable<MessageAttachment>? Attachments { get; set; }
}

public class AIRequest
{
    public string Content { get; set; }
    public string SystemPrompt { get; set; } = DefaultPrompts.System;
    public string UserId { get; set; }
    public string ChannelId { get; set; }
    public string ContextId { get; set; }
    public IncomingMessage OriginalMessage { get; set; }
    
    public AIRequestOptions Options { get; set; } = new();
    public IEnumerable<ITool>? AvailableTools { get; set; }
}

public class AIResponse
{
    public string Content { get; set; }
    public bool Success { get; set; } = true;
    public string? ErrorMessage { get; set; }
    public ResponseType Type { get; set; } = ResponseType.Text;
    
    // Tool execution results
    public IEnumerable<ToolResult>? ToolResults { get; set; }
    
    // Usage and cost tracking
    public UsageMetrics Usage { get; set; } = new();
    public decimal EstimatedCost { get; set; }
    
    // Provider information
    public string ProviderId { get; set; }
    public string ModelUsed { get; set; }
    public TimeSpan ResponseTime { get; set; }
}

public class ToolCall
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ToolName { get; set; }
    public JsonNode Parameters { get; set; }
    public string ContextId { get; set; }
    public string UserId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class ToolResult
{
    public string ToolCallId { get; set; }
    public string ToolName { get; set; }
    public bool Success { get; set; }
    public object? Data { get; set; }
    public string? Message { get; set; }
    public string? ErrorMessage { get; set; }
    public TimeSpan ExecutionTime { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    // For long-running operations
    public string? OperationId { get; set; }
    public ToolExecutionStatus Status { get; set; } = ToolExecutionStatus.Completed;
}
```

### Service Registration and DI
```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAgentixCore(this IServiceCollection services, 
        Action<AgentixOptions>? configure = null)
    {
        var options = new AgentixOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);
        
        // Core services
        services.AddSingleton<IToolRegistry, ToolRegistry>();
        services.AddSingleton<IToolExecutor, ToolExecutor>();
        services.AddSingleton<IProviderRegistry, ProviderRegistry>();
        services.AddSingleton<IChannelRegistry, ChannelRegistry>();
        services.AddSingleton<IAgentixOrchestrator, AgentixOrchestrator>();
        
        // Default implementations
        services.AddSingleton<IContextResolver, DefaultContextResolver>();
        services.AddSingleton<IErrorHandler, DefaultErrorHandler>();
        services.AddSingleton<IRateLimiter, DefaultRateLimiter>();
        
        // Background services
        services.AddHostedService<AgentixBackgroundService>();
        
        return services;
    }
}
```

---

## Context Memory System

### Context Architecture
The context memory system provides intelligent conversation state management across different channels and sessions. This is a critical component that prevents the "what was the first one?" problem and enables natural, multi-turn conversations.

#### Context Layers
```
┌─────────────────────────────────────────┐
│           User Session Context          │  ← Long-term user preferences
├─────────────────────────────────────────┤
│         Conversation Context            │  ← Current conversation thread
├─────────────────────────────────────────┤
│           Request Context               │  ← Single request + tool results
└─────────────────────────────────────────┘
```

#### Context Scope Strategy
Different channels need different context boundaries:
- **Slack**: Context per channel + user combination
- **Teams**: Context per conversation thread
- **Web API**: Context per session ID
- **Console**: Context per application instance

### Context Interfaces

```csharp
// Primary context interface
public interface IConversationContext
{
    string ContextId { get; }
    string UserId { get; }
    string ChannelId { get; }
    string ChannelType { get; }
    DateTime CreatedAt { get; }
    DateTime LastActivity { get; }
    DateTime ExpiresAt { get; }
    
    // Message history management
    Task<IEnumerable<ContextMessage>> GetMessagesAsync(int maxCount = 10);
    Task AddMessageAsync(ContextMessage message);
    Task ClearMessagesAsync();
    
    // Key-value context data storage
    Task<T?> GetAsync<T>(string key) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
    Task<IEnumerable<string>> GetKeysAsync();
    
    // Tool results from current conversation
    Task<IEnumerable<ToolResult>> GetRecentToolResultsAsync(string? toolName = null, int maxCount = 5);
    Task AddToolResultAsync(ToolResult result);
    
    // Context lifecycle
    Task ExtendExpirationAsync(TimeSpan extension);
    Task RefreshActivityAsync();
    Task<bool> IsExpiredAsync();
    Task ClearAsync();
}

// Context storage abstraction
public interface IContextStore
{
    Task<IConversationContext?> GetContextAsync(string contextId);
    Task<IConversationContext> CreateContextAsync(string contextId, string userId, string channelId, string channelType);
    Task SaveContextAsync(IConversationContext context);
    Task DeleteContextAsync(string contextId);
    Task<IEnumerable<string>> GetUserContextsAsync(string userId);
    Task CleanupExpiredContextsAsync();
}

// Context resolution strategy
public interface IContextResolver
{
    string ResolveContextId(IncomingMessage message);
    TimeSpan GetDefaultExpiration(string channelType);
    int GetMaxMessageHistory(string channelType);
    bool ShouldCreateNewContext(IncomingMessage message);
}
```

### Context Message Model
```csharp
public class ContextMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Role { get; set; } // "user", "assistant", "system", "tool"
    public string Content { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? ToolName { get; set; } // If this message came from a tool
    public Dictionary<string, object> Metadata { get; set; } = new();
    
    // For token counting and cost tracking
    public int? InputTokens { get; set; }
    public int? OutputTokens { get; set; }
    public decimal? Cost { get; set; }
}

public class ContextData
{
    public string Key { get; set; }
    public object Value { get; set; }
    public DateTime SetAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public string Type { get; set; } // For deserialization
}
```

### Context Storage Implementations

#### Option 1: In-Memory Store (Development/Single Instance)
```csharp
public class InMemoryContextStore : IContextStore
{
    private readonly ConcurrentDictionary<string, ConversationContext> _contexts = new();
    private readonly Timer _cleanupTimer;
    private readonly ILogger<InMemoryContextStore> _logger;
    
    public InMemoryContextStore(ILogger<InMemoryContextStore> logger)
    {
        _logger = logger;
        // Cleanup expired contexts every 5 minutes
        _cleanupTimer = new Timer(CleanupExpired, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }
    
    public async Task<IConversationContext?> GetContextAsync(string contextId)
    {
        if (_contexts.TryGetValue(contextId, out var context) && !await context.IsExpiredAsync())
        {
            await context.RefreshActivityAsync();
            return context;
        }
        else if (_contexts.ContainsKey(contextId))
        {
            _contexts.TryRemove(contextId, out _);
        }
        return null;
    }
    
    public async Task<IConversationContext> CreateContextAsync(string contextId, string userId, string channelId, string channelType)
    {
        var context = new InMemoryConversationContext(contextId, userId, channelId, channelType);
        _contexts.TryAdd(contextId, context);
        return context;
    }
    
    // Additional implementation...
}
```

#### Option 2: Redis Store (Production/Distributed)
```csharp
public class RedisContextStore : IContextStore
{
    private readonly IDatabase _database;
    private readonly ILogger<RedisContextStore> _logger;
    private readonly ContextOptions _options;
    
    public RedisContextStore(IConnectionMultiplexer redis, 
                           ILogger<RedisContextStore> logger,
                           IOptions<ContextOptions> options)
    {
        _database = redis.GetDatabase();
        _logger = logger;
        _options = options.Value;
    }
    
    public async Task<IConversationContext?> GetContextAsync(string contextId)
    {
        var contextKey = GetContextKey(contextId);
        var messagesKey = GetMessagesKey(contextId);
        var dataKey = GetDataKey(contextId);
        
        // Load context metadata
        var contextData = await _database.HashGetAllAsync(contextKey);
        if (!contextData.Any()) return null;
        
        // Load messages (recent ones)
        var messages = await _database.ListRangeAsync(messagesKey, -10, -1); // Last 10 messages
        
        // Load context data
        var contextValues = await _database.HashGetAllAsync(dataKey);
        
        var context = new RedisConversationContext(contextId, contextData, messages, contextValues, _database, _options);
        
        if (await context.IsExpiredAsync())
        {
            await DeleteContextAsync(contextId);
            return null;
        }
        
        await context.RefreshActivityAsync();
        return context;
    }
    
    private string GetContextKey(string contextId) => $"agentix:context:{contextId}";
    private string GetMessagesKey(string contextId) => $"agentix:messages:{contextId}";
    private string GetDataKey(string contextId) => $"agentix:data:{contextId}";
    private string GetToolsKey(string contextId) => $"agentix:tools:{contextId}";
    
    // Additional implementation...
}
```

#### Option 3: Database Store (Enterprise/Audit Trail)
```csharp
public class DatabaseContextStore : IContextStore
{
    private readonly AgentixDbContext _dbContext;
    private readonly ILogger<DatabaseContextStore> _logger;
    
    public async Task<IConversationContext?> GetContextAsync(string contextId)
    {
        var context = await _dbContext.Conversations
            .Include(c => c.Messages.OrderByDescending(m => m.Timestamp).Take(20))
            .Include(c => c.ContextData.Where(d => d.ExpiresAt == null || d.ExpiresAt > DateTime.UtcNow))
            .FirstOrDefaultAsync(c => c.ContextId == contextId);
            
        return context != null ? new DatabaseConversationContext(context, _dbContext) : null;
    }
}

// Entity Framework models
public class ConversationEntity
{
    public string ContextId { get; set; }
    public string UserId { get; set; }
    public string ChannelId { get; set; }
    public string ChannelType { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastActivity { get; set; }
    public DateTime ExpiresAt { get; set; }
    
    public ICollection<MessageEntity> Messages { get; set; } = new List<MessageEntity>();
    public ICollection<ContextDataEntity> ContextData { get; set; } = new List<ContextDataEntity>();
}
```

### Context Resolution Strategies

#### Channel-Specific Context IDs
```csharp
public class DefaultContextResolver : IContextResolver
{
    public string ResolveContextId(IncomingMessage message)
    {
        return message.Channel switch
        {
            "slack" => $"slack:{message.ChannelId}:{message.UserId}",
            "teams" => message.Metadata.ContainsKey("conversationId") 
                      ? $"teams:{message.Metadata["conversationId"]}"
                      : $"teams:{message.ChannelId}:{message.UserId}",
            "webapi" => message.Metadata.ContainsKey("sessionId")
                       ? $"web:{message.Metadata["sessionId"]}"
                       : $"web:{message.UserId}",
            "console" => $"console:{Environment.MachineName}:{message.UserId}",
            _ => $"default:{message.Channel}:{message.UserId}"
        };
    }
    
    public TimeSpan GetDefaultExpiration(string channelType)
    {
        return channelType switch
        {
            "slack" => TimeSpan.FromHours(4),    // Slack conversations are shorter
            "teams" => TimeSpan.FromHours(8),    // Teams conversations can be longer
            "webapi" => TimeSpan.FromHours(1),   // Web sessions shorter by default
            "console" => TimeSpan.FromDays(1),   // Console sessions can be longer
            _ => TimeSpan.FromHours(2)
        };
    }
    
    public int GetMaxMessageHistory(string channelType)
    {
        return channelType switch
        {
            "slack" => 15,   // Slack moves fast
            "teams" => 25,   // Teams conversations can be more detailed
            "webapi" => 10,  // Keep web light
            "console" => 50, // Console users might have longer sessions
            _ => 20
        };
    }
    
    public bool ShouldCreateNewContext(IncomingMessage message)
    {
        // Create new context for explicit "new" commands
        var content = message.Content.ToLowerInvariant();
        return content.Contains("new conversation") || 
               content.Contains("start over") ||
               content.Contains("clear context");
    }
}
```

### Framework Integration

#### Service Registration
```csharp
public static class ContextExtensions
{
    public static IServiceCollection AddAgentixContext(this IServiceCollection services, 
        Action<ContextOptions>? configure = null)
    {
        var options = new ContextOptions();
        configure?.Invoke(options);
        
        services.AddSingleton(options);
        services.AddSingleton<IContextResolver, DefaultContextResolver>();
        
        // Register store based on configuration
        switch (options.Store)
        {
            case ContextStoreType.InMemory:
                services.AddSingleton<IContextStore, InMemoryContextStore>();
                break;
            case ContextStoreType.Redis:
                services.AddStackExchangeRedisCache(redisOptions =>
                {
                    redisOptions.Configuration = options.RedisConnectionString;
                });
                services.AddSingleton<IContextStore, RedisContextStore>();
                break;
            case ContextStoreType.Database:
                services.AddDbContext<AgentixDbContext>(db =>
                {
                    db.UseSqlServer(options.DatabaseConnectionString);
                });
                services.AddScoped<IContextStore, DatabaseContextStore>();
                break;
        }
        
        // Background service for cleanup
        services.AddHostedService<ContextCleanupService>();
        
        return services;
    }
}

public class ContextOptions
{
    public ContextStoreType Store { get; set; } = ContextStoreType.InMemory;
    public string? RedisConnectionString { get; set; }
    public string? DatabaseConnectionString { get; set; }
    public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromHours(4);
    public int MaxMessageHistory { get; set; } = 20;
    public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromMinutes(15);
}

public enum ContextStoreType
{
    InMemory,
    Redis,
    Database
}
```

#### Usage in AI Provider
```csharp
public class OpenAIProvider : IAIProvider
{
    private readonly IContextStore _contextStore;
    private readonly IContextResolver _contextResolver;
    
    public async Task<AIResponse> GenerateWithContextAsync(AIRequest request, CancellationToken cancellationToken)
    {
        // Resolve context for this request
        var contextId = _contextResolver.ResolveContextId(request.OriginalMessage);
        var context = await _contextStore.GetContextAsync(contextId) 
                     ?? await _contextStore.CreateContextAsync(contextId, request.UserId, request.ChannelId, request.OriginalMessage.Channel);
        
        // Add user message to context
        await context.AddMessageAsync(new ContextMessage
        {
            Role = "user",
            Content = request.Content,
            Metadata = { ["channel"] = request.ChannelId }
        });
        
        // Build OpenAI request with conversation history
        var messages = new List<OpenAIMessage>();
        
        // Add system message
        messages.Add(new OpenAIMessage { Role = "system", Content = request.SystemPrompt });
        
        // Add conversation history
        var history = await context.GetMessagesAsync();
        foreach (var msg in history.TakeLast(10)) // Last 10 for token management
        {
            messages.Add(new OpenAIMessage { Role = msg.Role, Content = msg.Content });
        }
        
        // Make OpenAI call
        var response = await CallOpenAIAsync(messages, request.AvailableTools, cancellationToken);
        
        // Save assistant response to context
        await context.AddMessageAsync(new ContextMessage
        {
            Role = "assistant",
            Content = response.Content,
            InputTokens = response.Usage.PromptTokens,
            OutputTokens = response.Usage.CompletionTokens,
            Cost = CalculateCost(response.Usage)
        });
        
        // Save any tool results
        foreach (var toolResult in response.ToolResults ?? Enumerable.Empty<ToolResult>())
        {
            await context.AddToolResultAsync(toolResult);
        }
        
        await _contextStore.SaveContextAsync(context);
        
        return response;
    }
}
```

### Security & Privacy

#### Data Retention Policies
```csharp
public class ContextRetentionPolicy
{
    public TimeSpan MessageRetention { get; set; } = TimeSpan.FromDays(30);
    public TimeSpan ToolResultRetention { get; set; } = TimeSpan.FromDays(7);
    public TimeSpan SensitiveDataRetention { get; set; } = TimeSpan.FromHours(2);
    
    public bool ShouldRetainMessage(ContextMessage message)
    {
        // Don't retain sensitive information long-term
        if (ContainsSensitiveData(message.Content))
        {
            return DateTime.UtcNow - message.Timestamp < SensitiveDataRetention;
        }
        
        return DateTime.UtcNow - message.Timestamp < MessageRetention;
    }
    
    private bool ContainsSensitiveData(string content)
    {
        // Simple heuristics - could be more sophisticated
        return content.Contains("password", StringComparison.OrdinalIgnoreCase) ||
               content.Contains("api key", StringComparison.OrdinalIgnoreCase) ||
               Regex.IsMatch(content, @"\b\d{4}-\d{4}-\d{4}-\d{4}\b"); // Credit card pattern
    }
}
```

#### Access Control
```csharp
public class ContextAccessControl
{
    public async Task<bool> CanAccessContextAsync(string contextId, string requestingUserId, string requestingRole)
    {
        var context = await _contextStore.GetContextAsync(contextId);
        if (context == null) return false;
        
        // Users can access their own contexts
        if (context.UserId == requestingUserId) return true;
        
        // Admins can access any context
        if (requestingRole == "admin") return true;
        
        // Team leads can access their team's contexts
        if (requestingRole == "team-lead" && await IsOnSameTeamAsync(context.UserId, requestingUserId))
            return true;
        
        return false;
    }
}
```

### Usage Examples

#### Smart Context Usage
```csharp
// Example conversation flow:
User: "Search for React security vulnerabilities"
Bot: [searches and shows results]
     [Context stores: search_results = [list of vulnerabilities]]

User: "Create a Jira ticket for the first one"
Bot: [accesses context.GetAsync<SearchResult[]>("search_results")]
     [creates ticket for first vulnerability]
     [Context stores: created_ticket = JiraTicket{...}]

User: "What was the ticket number?"
Bot: [accesses context.GetAsync<JiraTicket>("created_ticket")]
     "The ticket number is DEV-123"
```

#### Tool Result Referencing
```csharp
// Tools can reference previous results
public class JiraCreateTool : AgentixTool
{
    public override async Task<ToolResult> ExecuteAsync(ToolCall toolCall, CancellationToken cancellationToken)
    {
        var context = await _contextStore.GetContextAsync(toolCall.ContextId);
        
        // Check if user said "the first one" - look for recent search results
        if (toolCall.Parameters.ContainsKey("reference") && 
            toolCall.Parameters["reference"].ToString().Contains("first"))
        {
            var recentSearchResults = await context.GetRecentToolResultsAsync("web_search", 1);
            if (recentSearchResults.Any())
            {
                var searchData = recentSearchResults.First().Data as SearchResult[];
                var firstResult = searchData?.FirstOrDefault();
                // Use the first search result to create ticket
            }
        }
        
        // Regular ticket creation logic...
    }
}
```

#### Configuration Example
```csharp
// Startup configuration
services.AddAgentixCore()
    .AddAgentixContext(options =>
    {
        options.Store = ContextStoreType.Redis;
        options.RedisConnectionString = configuration.GetConnectionString("Redis");
        options.DefaultExpiration = TimeSpan.FromHours(8);
        options.MaxMessageHistory = 25;
    })
    .AddOpenAIProvider(options =>
    {
        options.ApiKey = configuration["OpenAI:ApiKey"];
        options.EnableContextualConversations = true; // New flag
    });
```

This design provides:
- **Flexible storage options** (memory, Redis, database)
- **Channel-appropriate context boundaries**
- **Security and privacy controls**
- **Smart tool result referencing**
- **Token and cost management**
- **Easy configuration and defaults**

The key is making context feel natural to users while giving developers control over storage, retention, and access patterns.

---

## AI Provider Abstraction

### Provider Registry
```csharp
public interface IProviderRegistry
{
    void RegisterProvider(IAIProvider provider);
    IAIProvider? GetProvider(string name);
    IEnumerable<IAIProvider> GetAllProviders();
    IAIProvider GetBestProvider(AIRequest request);
    IAIProvider GetFallbackProvider(IAIProvider failedProvider);
}

public class ProviderRegistry : IProviderRegistry
{
    private readonly ConcurrentDictionary<string, IAIProvider> _providers = new();
    private readonly ILogger<ProviderRegistry> _logger;
    private readonly ProviderOptions _options;
    
    public IAIProvider GetBestProvider(AIRequest request)
    {
        // Route based on request characteristics
        if (request.AvailableTools?.Any() == true && request.AvailableTools.Count() > 3)
        {
            // Complex tool usage - prefer GPT-4
            return GetProvider("openai-gpt4") ?? GetProvider("claude-3-sonnet") ?? _providers.Values.First();
        }
        
        if (request.Content.Length < 100)
        {
            // Simple query - use cheaper model
            return GetProvider("openai-gpt3.5") ?? GetProvider("claude-3-haiku") ?? _providers.Values.First();
        }
        
        // Default routing
        return GetProvider(_options.DefaultProvider) ?? _providers.Values.First();
    }
}
```

### OpenAI Provider Implementation
```csharp
public class OpenAIProvider : IAIProvider
{
    private readonly OpenAIClient _client;
    private readonly OpenAIOptions _options;
    private readonly ILogger<OpenAIProvider> _logger;
    
    public string Name => "openai";
    public AICapabilities Capabilities { get; private set; }
    
    public OpenAIProvider(IOptions<OpenAIOptions> options, ILogger<OpenAIProvider> logger)
    {
        _options = options.Value;
        _logger = logger;
        _client = new OpenAIClient(_options.ApiKey);
        
        Capabilities = new AICapabilities
        {
            SupportsFunctionCalling = true,
            SupportsStreaming = true,
            SupportsVision = _options.DefaultModel.Contains("vision"),
            MaxTokens = GetMaxTokensForModel(_options.DefaultModel),
            ContextWindow = GetContextWindowForModel(_options.DefaultModel),
            CostPerInputToken = GetInputCostForModel(_options.DefaultModel),
            CostPerOutputToken = GetOutputCostForModel(_options.DefaultModel),
            SupportedModels = new[] { "gpt-4", "gpt-4-turbo", "gpt-3.5-turbo" }
        };
    }
    
    public async Task<AIResponse> GenerateWithContextAsync(AIRequest request, 
                                                          IConversationContext context, 
                                                          CancellationToken cancellationToken = default)
    {
        var messages = new List<ChatMessage>();
        
        // Add system message
        if (!string.IsNullOrEmpty(request.SystemPrompt))
        {
            messages.Add(new ChatMessage(ChatRole.System, request.SystemPrompt));
        }
        
        // Add conversation history
        var history = await context.GetMessagesAsync(_options.MaxHistoryMessages);
        foreach (var msg in history.Where(m => m.Role != "system"))
        {
            var role = msg.Role switch
            {
                "user" => ChatRole.User,
                "assistant" => ChatRole.Assistant,
                "tool" => ChatRole.Tool,
                _ => ChatRole.User
            };
            messages.Add(new ChatMessage(role, msg.Content));
        }
        
        // Add current user message
        messages.Add(new ChatMessage(ChatRole.User, request.Content));
        
        var chatOptions = new ChatCompletionsOptions
        {
            DeploymentName = _options.DefaultModel,
            Messages = messages,
            Temperature = _options.Temperature,
            MaxTokens = _options.MaxTokens,
            FunctionCall = request.AvailableTools?.Any() == true ? FunctionCall.Auto : FunctionCall.None
        };
        
        // Add function definitions for tools
        if (request.AvailableTools?.Any() == true)
        {
            foreach (var tool in request.AvailableTools)
            {
                chatOptions.Functions.Add(ConvertToOpenAIFunction(tool));
            }
        }
        
        var response = await _client.GetChatCompletionsAsync(chatOptions, cancellationToken);
        
        // Save assistant message to context
        await context.AddMessageAsync(new ContextMessage
        {
            Role = "assistant",
            Content = response.Value.Choices[0].Message.Content ?? "",
            InputTokens = response.Value.Usage.PromptTokens,
            OutputTokens = response.Value.Usage.CompletionTokens,
            Metadata = { ["model"] = _options.DefaultModel, ["provider"] = Name }
        });
        
        return new AIResponse
        {
            Content = response.Value.Choices[0].Message.Content ?? "",
            Success = true,
            ProviderId = Name,
            ModelUsed = _options.DefaultModel,
            Usage = new UsageMetrics
            {
                InputTokens = response.Value.Usage.PromptTokens,
                OutputTokens = response.Value.Usage.CompletionTokens,
                TotalTokens = response.Value.Usage.TotalTokens
            },
            EstimatedCost = CalculateCost(response.Value.Usage),
            ResponseTime = TimeSpan.FromMilliseconds(response.GetRawResponse().Headers.TryGetValue("x-response-time", out var time) ? double.Parse(time) : 0)
        };
    }
    
    private FunctionDefinition ConvertToOpenAIFunction(ITool tool)
    {
        return new FunctionDefinition
        {
            Name = tool.Name,
            Description = tool.Description,
            Parameters = BinaryData.FromString(tool.Parameters.ToString())
        };
    }
    
    private decimal CalculateCost(CompletionsUsage usage)
    {
        return (usage.PromptTokens * Capabilities.CostPerInputToken) + 
               (usage.CompletionTokens * Capabilities.CostPerOutputToken);
    }
}
```

---

## Tool System Design

### Tool Registry and Execution
```csharp
public interface IToolRegistry
{
    void RegisterTool<T>() where T : class, ITool;
    void RegisterTool(ITool tool);
    void RegisterTool(string name, ITool tool);
    
    ITool? GetTool(string name);
    IEnumerable<ITool> GetToolsByCategory(string category);
    IEnumerable<ITool> GetAllTools();
    IEnumerable<ITool> GetToolsForUser(string userId, string[] userRoles);
    
    bool IsToolAvailable(string name);
    void UnregisterTool(string name);
}

public interface IToolExecutor
{
    Task<ToolResult> ExecuteToolAsync(ToolCall toolCall, CancellationToken cancellationToken = default);
    Task<ToolResult> ExecuteToolAsync(string toolName, JsonNode parameters, string contextId, string userId, CancellationToken cancellationToken = default);
    
    // For long-running operations
    Task<string> StartLongRunningToolAsync(ToolCall toolCall, CancellationToken cancellationToken = default);
    Task<ToolExecutionStatus> GetToolStatusAsync(string operationId);
    Task<ToolResult> GetToolResultAsync(string operationId);
}

public class ToolExecutor : IToolExecutor
{
    private readonly IToolRegistry _toolRegistry;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ToolExecutor> _logger;
    private readonly ToolExecutionOptions _options;
    private readonly ConcurrentDictionary<string, Task<ToolResult>> _longRunningOperations = new();
    
    public async Task<ToolResult> ExecuteToolAsync(ToolCall toolCall, CancellationToken cancellationToken = default)
    {
        var tool = _toolRegistry.GetTool(toolCall.ToolName);
        if (tool == null)
        {
            return new ToolResult
            {
                ToolCallId = toolCall.Id,
                ToolName = toolCall.ToolName,
                Success = false,
                ErrorMessage = $"Tool '{toolCall.ToolName}' not found"
            };
        }
        
        // Security check
        if (!await CanExecuteToolAsync(tool, toolCall.UserId))
        {
            return new ToolResult
            {
                ToolCallId = toolCall.Id,
                ToolName = toolCall.ToolName,
                Success = false,
                ErrorMessage = "Insufficient permissions to execute this tool"
            };
        }
        
        // Rate limiting check
        if (!await _rateLimiter.AllowToolExecutionAsync(toolCall.UserId, toolCall.ToolName))
        {
            return new ToolResult
            {
                ToolCallId = toolCall.Id,
                ToolName = toolCall.ToolName,
                Success = false,
                ErrorMessage = "Rate limit exceeded for this tool"
            };
        }
        
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Create timeout cancellation token
            using var timeoutCts = new CancellationTokenSource(_options.DefaultTimeout);
            using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
            
            var result = await tool.ExecuteAsync(toolCall, combinedCts.Token);
            result.ToolCallId = toolCall.Id;
            result.ExecutionTime = stopwatch.Elapsed;
            
            _logger.LogInformation("Tool {ToolName} executed successfully in {Duration}ms", 
                                 tool.Name, stopwatch.ElapsedMilliseconds);
            
            return result;
        }
        catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested)
        {
            _logger.LogWarning("Tool {ToolName} execution timed out after {Timeout}", 
                             tool.Name, _options.DefaultTimeout);
            
            return new ToolResult
            {
                ToolCallId = toolCall.Id,
                ToolName = toolCall.ToolName,
                Success = false,
                ErrorMessage =