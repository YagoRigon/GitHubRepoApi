using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using GitHubRepoAPI.Controllers;
;  // Certifique-se de que seu namespace está correto

var builder = WebApplication.CreateBuilder(args);

// Adiciona o suporte ao Swagger para documentação da API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Adiciona o serviço HttpClient para fazer chamadas HTTP externas
builder.Services.AddHttpClient();

// Adiciona os controladores (isso é necessário para a API REST)
builder.Services.AddControllers();

var app = builder.Build();

// Configura o pipeline de requisição
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
