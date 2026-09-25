using System.Text;
using ChatClient;
using Microsoft.Extensions.AI;
using OllamaSharp;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

var settings = ChatSettings.FromEnvironment();

ConsoleChat.Escrever("Iniciando MCP Server...", ConsoleColor.DarkGray);
await using var mcpClient = await McpConnector.ConectarAsync(
    settings,
    linha => { });

var tools = await mcpClient.ListToolsAsync();

IChatClient chatClient = new ChatClientBuilder(new OllamaApiClient(new Uri(settings.OllamaUrl), settings.Modelo))
    .UseFunctionInvocation()
    .Build();

var chat = new ConsoleChat(new ChatSession(chatClient, [.. tools]), settings);
chat.MostrarTools(tools);
await chat.ExecutarAsync();