# Agentix Samples

This directory contains sample applications demonstrating how to use the Agentix framework with different channels and providers.

## Existing Samples

- **Agentix.Sample.Console** - Command-line application demonstrating console channel with Claude provider

## Running Samples

### Console Sample

```bash
cd samples/Agentix.Sample.Console
dotnet run -- --api-key YOUR_CLAUDE_API_KEY
```

Or set environment variable:
```bash
set CLAUDE_API_KEY=YOUR_API_KEY
dotnet run
```

## Creating New Samples

To create a new sample application:

1. Create a new project directory: `samples/Agentix.Sample.{SampleName}/`
2. Add project references to the Agentix components you want to demonstrate
3. Create a sample application showing integration patterns
4. Add your project to the solution file
5. Document setup and usage in the project README

### Example Sample Structure

```
samples/Agentix.Sample.Web/
├── Agentix.Sample.Web.csproj
├── Program.cs
├── Controllers/
│   └── ChatController.cs
├── Views/
│   └── Chat/
└── README.md
```

### Sample Naming Convention

- `Agentix.Sample.Console` - Console applications
- `Agentix.Sample.Web` - Web applications  
- `Agentix.Sample.Slack` - Slack bot examples
- `Agentix.Sample.Teams` - Teams bot examples
- `Agentix.Sample.MultiProvider` - Multiple provider examples

## Planned Samples

- **Agentix.Sample.Web** - ASP.NET Core web application with WebAPI channel
- **Agentix.Sample.Slack** - Slack bot implementation
- **Agentix.Sample.Teams** - Microsoft Teams bot
- **Agentix.Sample.MultiProvider** - Demonstrating multiple AI providers
- **Agentix.Sample.CustomChannel** - Custom channel implementation example 