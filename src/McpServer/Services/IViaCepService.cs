using McpServer.Models;
 
namespace McpServer.Services;

public interface IViaCepService
{
    /// <summary>
    /// Busca o endereço de um CEP (8 dígitos, só números).
    /// Retorna null quando o CEP não existe.
    /// </summary>
    Task<ViaCepResponse?> BuscarEnderecoAsync(string cep, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca o CEP de um endereço.
    /// Retorna null quando o endereço não existe.
    /// </summary>
    Task<IReadOnlyList<ViaCepResponse>> BuscarCepPorEnderecoAsync(
        string uf, string cidade, string logradouro, CancellationToken cancellationToken = default);
}