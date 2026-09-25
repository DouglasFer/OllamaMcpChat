using System.Net.Http.Json;
using McpServer.Models;
using Microsoft.Extensions.Logging;

namespace McpServer.Services;

public class ViaCepService(HttpClient httpClient, ILogger<ViaCepService> logger) : IViaCepService
{
    public async Task<ViaCepResponse?> BuscarEnderecoAsync(string cep, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Consultando ViaCEP para o CEP {Cep}", cep);
 
        var resposta = await httpClient.GetFromJsonAsync<ViaCepResponse>($"ws/{cep}/json/", cancellationToken);
 
        if (resposta is null || resposta.NaoEncontrado)
        {
            logger.LogInformation("CEP {Cep} não encontrado", cep);
            return null;
        }
 
        return resposta;
    }
    
    public async Task<IReadOnlyList<ViaCepResponse>> BuscarCepPorEnderecoAsync(
        string uf, string cidade, string logradouro, CancellationToken cancellationToken = default)
    {
        var caminho = $"ws/{Uri.EscapeDataString(uf)}/{Uri.EscapeDataString(cidade)}/{Uri.EscapeDataString(logradouro)}/json/";

        logger.LogInformation("Consultando ViaCEP por endereço: {Logradouro}, {Cidade}/{Uf}", logradouro, cidade, uf);
        
        var resposta = await httpClient.GetFromJsonAsync<List<ViaCepResponse>>(caminho, cancellationToken);
        
        if (resposta is null || resposta.Count == 0)
        {
            logger.LogInformation("Endereço não encontrado");
            return null;
        }
 
        return resposta;
    }
}