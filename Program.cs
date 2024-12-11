using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using GitHubRepoAPI.Controllers;
;  // Certifique-se de que seu namespace est� correto

var builder = WebApplication.CreateBuilder(args);

// Adiciona o suporte ao Swagger para documenta��o da API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Adiciona o servi�o HttpClient para fazer chamadas HTTP externas
builder.Services.AddHttpClient();

// Adiciona os controladores (isso � necess�rio para a API REST)
builder.Services.AddControllers();

var app = builder.Build();

// Configura o pipeline de requisi��o
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Habilita redirecionamento HTTPS
app.UseHttpsRedirection();

// Mapeia as rotas para os controladores da API
app.MapControllers();

// Inicia o servidor
app.Run();
