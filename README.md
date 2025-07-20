# Agentix.Net

A modular .NET framework for building AI-powered applications with multiple providers and communication channels.

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
│   └── agentix_design_document.md
├── Agentix.sln                   # Solution file
└── README.md                     # This file
```

## Quick Start

### Prerequisites

- .NET 8.0 SDK
- Claude API key (for the sample)

### Running the Console Sample

1. Clone the repository
2. Navigate to the samples directory:
   ```bash
   cd samples/Agentix.Sample.Console
   ```
3. Run with your Claude API key:
   ```bash
   dotnet run -- --api-key YOUR_CLAUDE_API_KEY
   ```
   Or set environment variable:
   ```bash
   set CLAUDE_API_KEY=YOUR_API_KEY
   dotnet run
   ```

## Framework Components

### Core Framework (`src/Core/Agentix.Core`)

The core framework provides:
- Abstractions for AI providers (`IAIProvider`)
- Channel adapter interfaces (`IChannelAdapter`)
- Orchestration services (`IAgentixOrchestrator`)
- Registry services for providers and channels
- Common models and utilities

### Channels (`src/Channels/`)

Channel adapters enable communication through different platforms:
- **Console**: Command-line interface ✅
- **Slack**: Slack bot integration (planned)
- **Teams**: Microsoft Teams integration (planned)
- **WebAPI**: HTTP REST API (planned)

### Providers (`src/Providers/`)

AI provider implementations:
- **Claude**: Anthropic Claude API ✅
- **OpenAI**: OpenAI GPT models (planned)
- **Azure OpenAI**: Azure OpenAI Service (planned)
- **Ollama**: Local model hosting (planned)

## Adding New Components

### Adding a Channel

1. Create a new project in `src/Channels/`:
   ```
   src/Channels/Agentix.Channels.YourChannel/
   ```
2. Implement `IChannelAdapter` interface
3. Add project to solution
4. Add reference to `Agentix.Core`

See [src/Channels/README.md](src/Channels/README.md) for detailed guidance.

### Adding a Provider

1. Create a new project in `src/Providers/`:
   ```
   src/Providers/Agentix.Providers.YourProvider/
   ```
2. Implement `IAIProvider` interface
3. Add project to solution
4. Add reference to `Agentix.Core`

See [src/Providers/README.md](src/Providers/README.md) for detailed guidance.

## Building the Solution

```bash
dotnet build
```

## Testing

```bash
dotnet test
```

## Documentation

- [Design Document](docs/agentix_design_document.md) - Comprehensive architecture and design
- [Channel Development Guide](src/Channels/README.md)
- [Provider Development Guide](src/Providers/README.md)
- [Sample Applications Guide](samples/README.md)

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes following the established patterns
4. Ensure tests pass
5. Submit a pull request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Roadmap

- [ ] OpenAI Provider
- [ ] Slack Channel Adapter
- [ ] Teams Channel Adapter
- [ ] Web API Channel
- [ ] Context Memory System
- [ ] Tool/Function Calling Support
- [ ] RAG Engine Integration
- [ ] Multi-provider Routing

## Architecture Goals

- **Modular**: Pay-as-you-grow package model
- **Production Ready**: Enterprise-grade security and scalability
- **Developer Friendly**: Native .NET patterns and dependency injection
- **Channel Agnostic**: Work across multiple communication platforms