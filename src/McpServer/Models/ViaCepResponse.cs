using System.Text.Json.Serialization;

namespace McpServer.Models;

public class ViaCepResponse
{
    [JsonPropertyName("cep")]         public string? Cep { get; init; }
    [JsonPropertyName("logradouro")]  public string? Logradouro { get; init; }
    [JsonPropertyName("complemento")] public string? Complemento { get; init; }
    [JsonPropertyName("bairro")]      public string? Bairro { get; init; }
    [JsonPropertyName("localidade")]  public string? Localidade { get; init; }
    [JsonPropertyName("uf")]          public string? Uf { get; init; }
    [JsonPropertyName("estado")]      public string? Estado { get; init; }
    [JsonPropertyName("regiao")]      public string? Regiao { get; init; }
    [JsonPropertyName("ddd")]         public string? Ddd { get; init; }
    
    [JsonPropertyName("erro")]        public System.Text.Json.JsonElement? Erro { get; init; }
 
    [JsonIgnore]
    public bool NaoEncontrado => Erro is not null;
}