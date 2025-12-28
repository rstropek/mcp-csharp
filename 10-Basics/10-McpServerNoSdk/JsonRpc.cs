using System.Text.Json;
using System.Text.Json.Serialization;

namespace McpServerNoSdk;

/// <summary>
/// Represents a JSON-RPC message.
/// </summary>
/// <remarks>
/// See also https://modelcontextprotocol.io/specification/2025-11-25/basic#messages
/// </remarks>
record JsonRpcMessage(
    [property: JsonPropertyName("jsonrpc")] string JsonRpc = "2.0",
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] object? Id = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Method = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] JsonElement? Params = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] object? Result = null,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] JsonRpcError? Error = null
);

record JsonRpcError(
    int Code,
    string Message
);
