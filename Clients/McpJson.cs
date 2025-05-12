using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

// The JSON scheme roughly matches VSCode's MCP format.
// See https://code.visualstudio.com/docs/copilot/chat/mcp-servers#_configuration-format

public record McpRoot(McpConfiguration? Mcp)
{
    public static McpRoot? Deserialize(string content) => 
        JsonSerializer.Deserialize<McpRoot>(content, 
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            });
}

public record McpConfiguration(
    Dictionary<string, Server>? Servers,
    List<McpInput>? Inputs);

public enum ServerType
{
    Unspecified,
    Stdio,
    Sse,
    Http
}

public record Server(
    ServerType Type,
    string? Id,
    string? Description,
    string? Command,
    string[] Args,
    Dictionary<string, string> Env,
    string? Url)
{
    public ServerType ActualType =>
        // Not sure if this is correct (how to detect http when type is not specified?)
        Type is ServerType.Unspecified
            ? string.IsNullOrWhiteSpace(Url) ? ServerType.Stdio : ServerType.Sse
            : Type;
}


public record McpInput(
    string? Name,
    string? Id,
    string? Description,
    string? Type,
    string? Default,
    bool Password);