using System;
using System.Collections.Generic;
using System.IO;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;
using OpenAI;
using Microsoft.Extensions.AI;

string? openAiToken = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
if (string.IsNullOrWhiteSpace(openAiToken))
{
    Console.WriteLine("Please set the OPENAI_API_KEY environment variable.");
    return;
}

var chatClient = new OpenAIClient(openAiToken)
    .GetChatClient("gpt-4.1")
    .AsIChatClient()
    .AsBuilder()
    .UseFunctionInvocation()
    .Build();

// Load mcp.json from the current directory. It roughly matches VSCode's MCP format.
// See https://code.visualstudio.com/docs/copilot/chat/mcp-servers#_configuration-format
var mcpJson = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mcp.json"));

McpRoot? mcpConfig = McpRoot.Deserialize(mcpJson);
if (mcpConfig?.Mcp?.Servers is null)
{
    Console.WriteLine("No servers found in MCP config.");
    return;
}

List<IMcpClient> clients = new();
foreach ((string serverName, Server server) in mcpConfig.Mcp.Servers)
{
    Console.WriteLine($"Adding server '{serverName}' [{server.ActualType.ToString().ToLower()}]...");
    // TODO: handle Inputs.
    switch (server.ActualType)
    {
        case ServerType.Sse:
        case ServerType.Http:
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(server.Url, nameof(server.Url));
                var clientTransport = new SseClientTransport(
                    new SseClientTransportOptions
                    {
                        Name = serverName,
                        Endpoint = new Uri(server.Url),
                    });

                // If it fails here - have you launched the MyMcpServers?
                clients.Add(await McpClientFactory.CreateAsync(clientTransport));
                break;
            }

        case ServerType.Stdio:
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(server.Command, nameof(server.Command));
                var clientTransport = new StdioClientTransport(
                    new StdioClientTransportOptions()
                    {
                        Name = serverName,
                        Command = server.Command,
                        Arguments = server.Args,
                        EnvironmentVariables = server.Env,
                    });
                clients.Add(await McpClientFactory.CreateAsync(clientTransport));
                break;
            }
    }
}

List<McpClientTool> allTools = new();
foreach (var client in clients)
    allTools.AddRange(await client.ListToolsAsync());

string prompt = "Short explanation of the problem described in https://www.youtube.com/watch?v=iSNsgj1OCLA. " +
                "Also, print the current price of MSFT stock";

Console.WriteLine($"\n\nPrompt: {prompt}\n\nAnswer: ");

var response = chatClient.GetStreamingResponseAsync(prompt, new() { Tools = [.. allTools] });
await foreach (var update in response)
{
    foreach (var updateContent in update.Contents)
    {
        switch (updateContent)
        {
            case TextContent textContent:
                Console.Write(textContent.Text);
                break;
            case FunctionCallContent funcCall:
                Console.Write($"\n ** calling func {funcCall.Name}(...) **\n");
                break;
        }
    }
}

Console.WriteLine("\n\nDone. Press any key to quit...");
Console.ReadKey();
