using System.Text.Json;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using System.Text.Encodings.Web;

namespace ChatClient;

public sealed class ConsoleChat(ChatSession sessao, ChatSettings settings)
{
    public void MostrarTools(IEnumerable<McpClientTool> tools)
    {
        Escrever("Tools disponíveis:", ConsoleColor.Cyan);
        foreach (var tool in tools)
            Escrever($"  • {tool.Name}: {tool.Description}", ConsoleColor.Cyan);
    }

    public async Task ExecutarAsync()
    {
        Escrever($"\nChat pronto (modelo: {settings.Modelo}). Digite 'sair' para encerrar.", ConsoleColor.White);

        while (LerPergunta() is { } pergunta)
        {
            try
            {
                await ExibirRespostaAsync(pergunta);
            }
            catch (HttpRequestException ex)
            {
                Escrever($"\nNão consegui falar com o Ollama em {settings.OllamaUrl}. Ele está rodando? ({ex.Message})",
                         ConsoleColor.Red);
            }
        }
    }
    
    private static string? LerPergunta()
    {
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("\nVocê: ");
            Console.ResetColor();

            var entrada = Console.ReadLine();
            if (entrada is null || entrada.Trim().Equals("sair", StringComparison.OrdinalIgnoreCase))
                return null;
            if (!string.IsNullOrWhiteSpace(entrada))
                return entrada;
        }
    }

    private async Task ExibirRespostaAsync(string pergunta)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.Write("IA: ");
        Console.ResetColor();

        var respondeu = false;

        await foreach (var update in sessao.EnviarAsync(pergunta))
        {
            foreach (var chamada in update.Contents.OfType<FunctionCallContent>())
            {
                respondeu = true;
                Escrever($"\n🔧 Modelo chamou a tool '{chamada.Name}' com {JsonSerializer.Serialize(chamada.Arguments, JsonLegivel)}",
                    ConsoleColor.Yellow);
            }

            if (!string.IsNullOrEmpty(update.Text))
            {
                respondeu = true;
                Console.Write(update.Text);
            }
        }

        if (!respondeu)
            Escrever("(O modelo não respondeu. Tente reformular, ex.: \"qual o endereço do CEP 01001-000?\")", ConsoleColor.DarkGray);

        Console.WriteLine();
    }

    public static void Escrever(string texto, ConsoleColor cor)
    {
        Console.ForegroundColor = cor;
        Console.WriteLine(texto);
        Console.ResetColor();
    }
    
    private static readonly JsonSerializerOptions JsonLegivel = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
}