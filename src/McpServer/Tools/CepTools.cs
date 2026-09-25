using System.ComponentModel;
using System.Text;
using McpServer.Helpers;
using McpServer.Services;
using Microsoft.Extensions.Logging;
using McpServer.Models;
using ModelContextProtocol.Server;
using System.Net;

namespace McpServer.Tools;

[McpServerToolType]
public sealed class CepTools(IViaCepService viaCepService, ILogger<CepTools> logger)
{
    [McpServerTool(Name = "busca_cep")]
    [Description("Recebe um CEP que o usuário JÁ informou (8 dígitos) e retorna o endereço completo. Nunca invente um CEP para usar esta ferramenta.")]
    public async Task<string> ConsultaCep(
        [Description("CEP com 8 digitos, pode vir com ou sem traço, ex: 00000-000 ou 00000000")]
        string cep,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("[MCP] Consulta CEP {cep}", cep);

        if (string.IsNullOrWhiteSpace(cep))
            return "Cep não pode ser vazio";

        var cepLimpo = new string(cep.Where(char.IsDigit).ToArray());

        if (cepLimpo.Length != 8)
            return $"CEP inválido: '{cep}'. Um CEP precisa ter exatamente 8 dígitos.";

        try
        {
            var endereco = await viaCepService.BuscarEnderecoAsync(cepLimpo, cancellationToken);

            if (endereco is null)
                return $"o CEP {cepLimpo} não foi encontrado na base da ViaCep.";

            logger.LogInformation("[MCP] consulta cep retornou o endereço de {cidade}/{UF}", endereco.Localidade,
                endereco.Uf);
            return FormataEndereco(endereco);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "[MCP] Falha ao consultar a ViaCEP");
            return "Não foi possível consultar a ViaCEP no momento.";
        }
    }

    [McpServerTool(Name = "busca_cep_por_endereco")]
    [Description("Recebe rua, cidade e estado e retorna o CEP. Use quando o usuário perguntar qual é o CEP de um endereço.")]
    public async Task<string> BuscarCepPorEndereco(
        [Description("Sigla do estado com 2 letras. OBRIGATÓRIO: se o usuário não disser o estado, deduza pela cidade (ex: Goiânia → GO, São Paulo → SP, Curitiba → PR)")]
        string uf,
        [Description("Nome do MUNICÍPIO, nunca o bairro. ex: Goiânia, São Paulo")]
        string cidade,
        [Description("Nome da rua ou avenida, SEM o número da casa, ex: Via de Acesso 5")]
        string logradouro,
        [Description("Número do imóvel, somente se o usuário informou, ex: 14")]
        string numero = "",
        [Description("Bairro, somente se o usuário informou. ex: Jardim Liberdade, Fazenda São Domingos")]
        string bairro = "",
        CancellationToken cancellationToken = default)
    {
        var ufFinal = EnderecoNormalizador.NormalizarUf(uf) ?? uf;
        var (logradouroFinal, numeroNoLogradouro) = EnderecoNormalizador.NormalizarLogradouro(logradouro ?? "");
        if (string.IsNullOrWhiteSpace(numero))
            numero = numeroNoLogradouro ?? "";

        logger.LogInformation("[MCP] Busca CEP por endereço: {Logradouro}, {Cidade}/{Uf}", logradouroFinal, cidade, ufFinal);

        var erroValidacao = ValidarEndereco(ufFinal, cidade, logradouroFinal);
        if (erroValidacao is not null)
            return erroValidacao;

        try
        {
            var resultados = await viaCepService.BuscarCepPorEnderecoAsync(
                ufFinal.Trim().ToUpperInvariant(), cidade.Trim(), logradouroFinal.Trim(), cancellationToken);

            logger.LogInformation("[MCP] Busca por endereço retornou {Total} resultado(s)", resultados.Count);

            if (resultados.Count == 0)
                return $"Nenhum CEP encontrado para '{logradouroFinal}', {cidade}/{ufFinal}. Confira a grafia da rua e da cidade.";

            if (resultados.Count == 1)
                return FormataEndereco(resultados[0]);

            var candidatos = FiltrarPorBairro(resultados, bairro);
            candidatos = FiltrarPorNumero(candidatos, ExtrairNumero(numero));
            
            if (candidatos.Count == 1)
                return FormataEndereco(candidatos[0]);

            const int limite = 10;
            var lista = string.Join("\n", candidatos.Take(limite).Select(FormataResumo));

            return $"""
                Foram encontrados {candidatos.Count} CEPs para esse endereço.
                Ruas longas têm um CEP por faixa de numeração, indicada entre parênteses.
                Se o usuário informou o número, escolha o CEP da faixa correspondente.
                Se não informou, mostre as opções ou pergunte o número ou o bairro.

                {lista}
                """;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
        {
            logger.LogWarning(ex, "[MCP] ViaCEP rejeitou a busca por endereço");
            return "A ViaCEP rejeitou a busca. Confira se a UF tem 2 letras e se cidade e rua estão corretas.";
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            logger.LogWarning(ex, "[MCP] Falha ao consultar a ViaCEP");
            return "Não foi possível consultar a ViaCEP no momento. Tente novamente.";
        }
    }

    private static string FormataEndereco(ViaCepResponse endereco)
    {
        (string Rotulo, string? Valor)[] campos =
        [
            ("CEP", endereco.Cep),
            ("Logradouro", endereco.Logradouro),
            ("Complemento", endereco.Complemento),
            ("Bairro", endereco.Bairro),
            ("Cidade", $"{endereco.Localidade} - {endereco.Uf}"),
            ("Estado", endereco.Estado),
            ("DDD", endereco.Ddd),
        ];

        return string.Join("\n", campos
            .Where(c => !string.IsNullOrWhiteSpace(c.Valor))
            .Select(c => $"{c.Rotulo}: {c.Valor}"));
    }

    private static string? ValidarEndereco(string uf, string cidade, string logradouro)
    {
        if (string.IsNullOrWhiteSpace(uf) || uf.Trim().Length != 2 || !uf.Trim().All(char.IsLetter))
            return $"UF inválida: '{uf}'. Use a sigla do estado com 2 letras, ex: SP.";

        if (string.IsNullOrWhiteSpace(cidade) || cidade.Trim().Length < 3)
            return "Informe o nome da cidade com pelo menos 3 caracteres.";

        if (string.IsNullOrWhiteSpace(logradouro) || logradouro.Trim().Length < 3)
            return "Informe o nome da rua com pelo menos 3 caracteres.";

        return null;
    }

    private static string FormataResumo(ViaCepResponse endereco)
    {
        var faixa = string.IsNullOrWhiteSpace(endereco.Complemento) ? "" : $" ({endereco.Complemento})";
        return $"- {endereco.Cep}: {endereco.Logradouro}{faixa}, bairro {endereco.Bairro}";
    }

    private static List<ViaCepResponse> FiltrarPorBairro(IReadOnlyList<ViaCepResponse> enderecos, string? bairro)
    {
        if (string.IsNullOrWhiteSpace(bairro) || enderecos.Count <= 1)
            return [.. enderecos];

        var alvo = Texto.Normalizar(bairro);
        var filtrados = enderecos.Where(e => Texto.Normalizar(e.Bairro).Contains(alvo)).ToList();
        
        return filtrados.Count > 0 ? filtrados : [.. enderecos];
    }

    private static List<ViaCepResponse> FiltrarPorNumero(List<ViaCepResponse> enderecos, int? numero)
    {
        if (numero is null || enderecos.Count <= 1)
            return enderecos;

        var pontuados = enderecos
            .Select(e => (Endereco: e, Pontos: FaixaNumeracao.Pontuar(e.Complemento, numero.Value)))
            .Where(x => x.Pontos > FaixaNumeracao.Nenhuma)
            .ToList();

        if (pontuados.Count == 0)
            return enderecos;
        
        var melhor = pontuados.Max(x => x.Pontos);
        return pontuados.Where(x => x.Pontos == melhor).Select(x => x.Endereco).ToList();
    }
    
    private static int? ExtrairNumero(string? numero) =>
        int.TryParse(string.Concat((numero ?? string.Empty).Where(char.IsDigit)), out var n) ? n : null;
}