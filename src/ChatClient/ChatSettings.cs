namespace ChatClient;

public sealed record ChatSettings(string OllamaUrl, string Modelo, string ProjetoServidor, string? ServidorDll)
{
    public static ChatSettings FromEnvironment() => new(
        OllamaUrl: Environment.GetEnvironmentVariable("OLLAMA_URL") ?? "http://localhost:11434",
        Modelo: Environment.GetEnvironmentVariable("OLLAMA_MODEL") ?? "qwen2.5:3b",
        ProjetoServidor: Environment.GetEnvironmentVariable("MCP_SERVER_PROJECT") ?? Path.Combine("src", "McpServer"),
        ServidorDll: Environment.GetEnvironmentVariable("MCP_SERVER_DLL"));
}