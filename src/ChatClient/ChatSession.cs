using System.Runtime.CompilerServices;
using Microsoft.Extensions.AI;

namespace ChatClient;

public sealed class ChatSession(IChatClient chatClient, IList<AITool> tools)
{
    // O system prompt define o comportamento do modelo e quando ele deve usar as tools.
    private readonly List<ChatMessage> _historico =
    [
        new(ChatRole.System, """
                             Você é um assistente prestativo e responde sempre em português do Brasil.
                             Você tem acesso a ferramentas externas:
                             - Para descobrir o endereço de um CEP, use a ferramenta de consulta de CEP.
                             - Para descobrir o CEP de um endereço, use a ferramenta de busca por endereço,
                               informando a UF como sigla (ex: SP) e a rua sem o número.
                             Ao usar uma ferramenta, responda com base apenas nos dados que ela retornou,
                             sem acrescentar informações externas.
                             Use as ferramentas SOMENTE quando o usuário informar um CEP ou um endereço concreto na pergunta atual.
                             Perguntas conceituais, como "o que é", "para que serve" ou "como funciona", NÃO usam ferramentas.
                             Nunca invente argumentos para uma ferramenta: use apenas dados que o usuário escreveu.
                             Se o usuário enviar apenas um número de 8 dígitos (com ou sem traço), trate-o como um CEP e consulte o endereço.
                             Sempre responda ao usuário com uma mensagem em texto. Nunca deixe a resposta vazia.
                             """)
    ];

    private readonly ChatOptions _options = new() { Tools = tools };

    public async IAsyncEnumerable<ChatResponseUpdate> EnviarAsync(
        string mensagem,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _historico.Add(new ChatMessage(ChatRole.User, mensagem));

        var updates = new List<ChatResponseUpdate>();

        await foreach (var update in chatClient.GetStreamingResponseAsync(_historico, _options, cancellationToken))
        {
            updates.Add(update);
            yield return update;
        }
        
        _historico.AddMessages(updates);
    }
}