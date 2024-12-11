using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GitHubRepoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RepositoriesController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        // Injeção de dependência do HttpClient
        public RepositoriesController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpGet("{language}")]
        public async Task<IActionResult> GetOldestRepositories(string language)
        {
            try
            {
                // Construção da URL para consulta à API do GitHub
                var url = BuildGitHubApiUrl(language);

                // Adiciona cabeçalho necessário para evitar erros de rate limit na API do GitHub
                ConfigureHttpClientHeaders();

                // Realiza a requisição à API do GitHub
                var response = await _httpClient.GetAsync(url);

                // Verifica se a requisição foi bem-sucedida
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, new { message = "Erro ao acessar a API do GitHub." });
                }

                // Processa a resposta da API
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var repositories = DeserializeGitHubResponse(jsonResponse);

                // Valida se a resposta contém repositórios
                if (repositories == null || repositories.Count == 0)
                {
                    return NotFound(new { message = "Nenhum repositório encontrado." });
                }

                // Formata os dados no formato do carrossel para o Blip
                var carouselResponse = FormatCarouselResponse(repositories);

                return Ok(carouselResponse);
            }
            catch (JsonException jsonEx)
            {
                // Erro durante a desserialização
                return StatusCode(500, new { message = "Erro ao processar a resposta da API.", error = jsonEx.Message });
            }
            catch (Exception ex)
            {
                // Captura outros erros genéricos
                return StatusCode(500, new { message = "Erro interno ao processar a requisição.", error = ex.Message });
            }
        }

        /// <summary>
        /// Monta a URL da API do GitHub para buscar os repositórios mais antigos.
        /// </summary>
        private string BuildGitHubApiUrl(string language)
        {
            return $"https://api.github.com/search/repositories?q=language:{language}+org:takenet&sort=created&order=asc&per_page=5";
        }

        /// <summary>
        /// Configura os cabeçalhos necessários no HttpClient.
        /// </summary>
        private void ConfigureHttpClientHeaders()
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "DotNet-API");
        }

        /// <summary>
        /// Desserializa a resposta da API do GitHub para o modelo de dados.
        /// </summary>
        private List<Repository> DeserializeGitHubResponse(string jsonResponse)
        {
            var gitHubResponse = JsonSerializer.Deserialize<GitHubApiResponse>(jsonResponse);
            return gitHubResponse?.Items?.OrderBy(repo => repo.CreatedAt).ToList();
        }

        /// <summary>
        /// Formata os dados no formato esperado pelo Blip.
        /// </summary>
        private object FormatCarouselResponse(List<Repository> repositories)
        {
            return new
            {
                items = repositories.Select(repo => new
                {
                    title = repo.Title, // Nome do repositório
                    imageUrl = repo.Owner?.ImageUrl, // URL do avatar do proprietário
                    text = repo.Text // Descrição do repositório
                }).ToList()
            };
        }

        // Modelo para mapear a resposta da API do GitHub
        public class GitHubApiResponse
        {
            [JsonPropertyName("items")]
            public List<Repository> Items { get; set; }
        }

        // Modelo de repositório retornado pela API do GitHub
        public class Repository
        {
            [JsonPropertyName("full_name")]
            public string Title { get; set; } // Nome completo do repositório

            [JsonPropertyName("description")]
            public string Text { get; set; } // Descrição do repositório

            [JsonPropertyName("created_at")]
            public DateTime CreatedAt { get; set; } // Data de criação do repositório

            [JsonPropertyName("owner")]
            public Owner Owner { get; set; } // Informações do proprietário
        }

        // Modelo do proprietário do repositório
        public class Owner
        {
            [JsonPropertyName("avatar_url")]
            public string ImageUrl { get; set; } // URL do avatar do proprietário
        }
    }
}
