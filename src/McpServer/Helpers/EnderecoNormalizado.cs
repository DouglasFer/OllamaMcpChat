using System.Text.RegularExpressions;

namespace McpServer.Helpers;

/// <summary>
/// Converte o endereço do jeito que as pessoas escrevem para o formato que a ViaCEP espera.
/// </summary>
public static partial class EnderecoNormalizador
{
    // Chaves sem acento e em minúsculas, porque a entrada passa por Texto.Normalizar.
    private static readonly Dictionary<string, string> Estados = new()
    {
        ["acre"] = "AC", ["alagoas"] = "AL", ["amapa"] = "AP", ["amazonas"] = "AM",
        ["bahia"] = "BA", ["ceara"] = "CE", ["distrito federal"] = "DF", ["espirito santo"] = "ES",
        ["goias"] = "GO", ["maranhao"] = "MA", ["mato grosso"] = "MT", ["mato grosso do sul"] = "MS",
        ["minas gerais"] = "MG", ["para"] = "PA", ["paraiba"] = "PB", ["parana"] = "PR",
        ["pernambuco"] = "PE", ["piaui"] = "PI", ["rio de janeiro"] = "RJ", ["rio grande do norte"] = "RN",
        ["rio grande do sul"] = "RS", ["rondonia"] = "RO", ["roraima"] = "RR", ["santa catarina"] = "SC",
        ["sao paulo"] = "SP", ["sergipe"] = "SE", ["tocantins"] = "TO",
    };

    private static readonly HashSet<string> Siglas = [.. Estados.Values];

    private static readonly Dictionary<string, string> TiposLogradouro = new()
    {
        ["av"] = "Avenida", ["avn"] = "Avenida",
        ["r"] = "Rua",
        ["al"] = "Alameda",
        ["pc"] = "Praça", ["pca"] = "Praça",
        ["tv"] = "Travessa", ["trav"] = "Travessa",
        ["rod"] = "Rodovia",
        ["est"] = "Estrada", ["estr"] = "Estrada",
        ["lg"] = "Largo", ["lgo"] = "Largo",
    };
    
    /// <summary>"go", "GO", "Goiás" ou "goias" => "GO". Não reconhecido => null.</summary>
    public static string? NormalizarUf(string? uf)
    {
        var texto = Texto.Normalizar(uf);

        if (texto.Length == 2 && Siglas.Contains(texto.ToUpperInvariant()))
            return texto.ToUpperInvariant();

        return Estados.GetValueOrDefault(texto);
    }

    /// <summary>
    /// Só separa o número quando ele vem após vírgula ou "nº", porque há ruas com número
    /// </summary>
    public static (string Logradouro, string? Numero) NormalizarLogradouro(string logradouro)
    {
        var texto = logradouro.Trim();
        string? numero = null;

        if (NumeroNoFim().Match(texto) is { Success: true } match)
        {
            numero = match.Groups[1].Value;
            texto = texto[..match.Index].Trim();
        }

        var partes = texto.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        if (partes.Length == 2)
        {
            var abreviacao = Texto.Normalizar(partes[0]).TrimEnd('.');
            if (TiposLogradouro.TryGetValue(abreviacao, out var tipoCompleto))
                texto = $"{tipoCompleto} {partes[1]}";
        }

        return (texto, numero);
    }

    [GeneratedRegex(@"\s*(?:,|\bn[º°]|\bn\.|\bn[uú]mero)\s*(\d+)\s*$", RegexOptions.IgnoreCase)]
    private static partial Regex NumeroNoFim();
}