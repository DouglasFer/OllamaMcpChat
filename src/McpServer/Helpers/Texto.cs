using System.Globalization;
using System.Text;

namespace McpServer.Helpers;

public static class Texto
{
    public static string Normalizar(string? texto)
    {
        var decomposto = (texto ?? string.Empty).Normalize(NormalizationForm.FormD);
        var semAcento = decomposto.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark);
        return string.Concat(semAcento).Trim().ToLowerInvariant();
    }
}