using ModelContextProtocol.Client;

namespace ChatClient;

public static class McpConnector
{
    public static Task<McpClient> ConectarAsync(ChatSettings settings, Action<string> onLogServidor)
    {
        string[] argumentos = settings.ServidorDll is { Length: > 0 } dll
            ? [dll]
            : ["run", "--project", settings.ProjetoServidor, "--no-build"];
        
        var transport = new StdioClientTransport(new StdioClientTransportOptions
        {
            Name = "CepServer",
            Command = "dotnet",
            Arguments = argumentos,
            StandardErrorLines = onLogServidor,
        });

        return McpClient.CreateAsync(transport);
    }
}