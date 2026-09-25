using System.Text.RegularExpressions;

namespace McpServer.Helpers;

/// <summary>
/// Interpreta o "complemento" da ViaCEP, como "até 610 - lado par", "de 612 a 1510 - lado par",
/// </summary>
public static partial class FaixaNumeracao
{
    public const int Nenhuma = 0;
    public const int Faixa = 1;
    public const int Exato = 2;

    public static int Pontuar(string? complemento, int numero)
    {
        var texto = Texto.Normalizar(complemento);
        if (texto.Length == 0)
            return Nenhuma;

        if (int.TryParse((string?)texto, out var unico))
            return unico == numero ? Exato : Nenhuma;

        if (texto.Contains("lado par") && numero % 2 != 0) return Nenhuma;
        if (texto.Contains("lado impar") && numero % 2 == 0) return Nenhuma;

        if (DeAte().Match(texto) is { Success: true } deAte)
            return numero >= Grupo(deAte, 1) && numero <= Grupo(deAte, 2) ? Faixa : Nenhuma;

        if (DeAoFim().Match(texto) is { Success: true } deAoFim)
            return numero >= Grupo(deAoFim, 1) ? Faixa : Nenhuma;

        if (Ate().Match(texto) is { Success: true } ate)
            return numero <= Grupo(ate, 1) ? Faixa : Nenhuma;
        
        return texto.StartsWith("lado") ? Faixa : Nenhuma;
    }

    private static int Grupo(Match match, int indice) => int.Parse(match.Groups[indice].Value);

    [GeneratedRegex(@"de (\d+) a (\d+)")] private static partial Regex DeAte();
    [GeneratedRegex(@"de (\d+) ao fim")]  private static partial Regex DeAoFim();
    [GeneratedRegex(@"ate (\d+)")]        private static partial Regex Ate();
}