using System.Text.Json.Nodes;
using Agentix.Core.Models;

namespace Agentix.Rag.Core.Interfaces;

/// <summary>
/// Simple tool interface for RAG operations.
/// This is a simplified version focused on the RAG use case.
/// </summary>
public interface ITool
{
    /// <summary>
    /// Gets the unique name of this tool.
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Gets the description of what this tool does.
    /// </summary>
    string Description { get; }
    
    /// <summary>
    /// Gets the category this tool belongs to.
    /// </summary>
    string Category { get; }
    
    /// <summary>
    /// Executes the tool with the given parameters.
    /// </summary>
    /// <param name="toolCall">The tool call containing parameters and context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The result of the tool execution</returns>
    Task<ToolResult> ExecuteAsync(ToolCall toolCall, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a call to execute a tool.
/// This extends the core ToolCall concept for RAG tools.
/// </summary>
public class ToolCall
{
    /// <summary>
    /// Gets or sets the unique identifier for this tool call.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Gets or sets the name of the tool to execute.
    /// </summary>
    public string ToolName { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the parameters for the tool execution.
    /// </summary>
    public JsonNode Parameters { get; set; } = new JsonObject();
    
    /// <summary>
    /// Gets or sets the context identifier for this tool call.
    /// </summary>
    public string ContextId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the user identifier who initiated this tool call.
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets when this tool call was made.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
} 