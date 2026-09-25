using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using McpServer.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);
builder.Logging.AddFilter("System.Net.Http", LogLevel.Warning);

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

builder.Services.AddHttpClient<IViaCepService, ViaCepService>(client =>
{
    client.BaseAddress = new Uri("https://viacep.com.br/");
    client.Timeout = TimeSpan.FromSeconds(10);
});

await builder.Build().RunAsync();
