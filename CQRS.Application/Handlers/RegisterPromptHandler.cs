namespace CQRS.Application.Handlers
{
    using CQRS.Application.Commands;
    using CQRS.Domain.Aggregates;
    using CQRS.Infrastructure.EventStore;
    using CQRS.Infrastructure.Redis;
    using CQRS.Infrastructure.SQL;
    using CQRS.Shared.Models;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Polly;
    using Polly.Retry;
    using System.Net.Http;
    using System.Net.Http.Json;

    public class RegisterPromptHandler
    {
        private readonly IEventStoreService _eventStore;
        private readonly IPromptReadModelRepository _readRepo;
        private readonly HttpClient _httpClient;
        private readonly HttpClient _httpClientAux;
        private readonly AzureOpenAIOptions _openAIOptions;
        private readonly ISqlPromptModelRepository _sqlRepo;
        private readonly ILogger<RegisterPromptHandler> _logger;

        private readonly AsyncRetryPolicy _retryPolicy;

        public RegisterPromptHandler(IEventStoreService eventStore, IPromptReadModelRepository readRepo,
            IHttpClientFactory factory, ISqlPromptModelRepository sqlRepo,
            IOptions<AzureOpenAIOptions> openAIOptions, HttpClient httpClientAux,
            ILogger<RegisterPromptHandler> logger)
        {
            _eventStore = eventStore;
            _readRepo = readRepo;
            _sqlRepo = sqlRepo;
            _httpClient = factory.CreateClient("AzureOpenAI");
            _openAIOptions = openAIOptions.Value;
            _httpClientAux = httpClientAux;
            _logger = logger;

            _retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(exception, $"Intento {retryCount} fallido. Reintentando en {timeSpan.TotalSeconds} segundos.");
                    });
        }

        public async Task Handle(RegisterPromptCommand cmd)
        {
            var prompt = PromptAggregate.Register(cmd.Id, cmd.Prompt);
            var imageBase64 = string.Empty;

            if (!string.IsNullOrEmpty(cmd.Prompt))
            {
                imageBase64 = await GetDalleImageUrl(cmd.Prompt);
            }

            await _eventStore.SaveAsync($"prompt-{prompt.Id}", prompt.Events);

            var readModel = new PromptReadModel(prompt.Id, prompt.Prompt, imageBase64);

            try
            {
                //SQL Server
                await _retryPolicy.ExecuteAsync(() => _sqlRepo.SaveAsync(readModel));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "| SQL ERROR");
                throw;
            }

            try
            {
                await _retryPolicy.ExecuteAsync(() => _readRepo.SaveAsync(readModel));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo guardar en Redis tras varios intentos. Ya está guardado en SQL Server.");
            }
        }

        async Task<string> GetDalleImageUrl(string prompt)
        {
            var url = $"openai/deployments/{_openAIOptions.Deployment}/images/generations?api-version={_openAIOptions.ApiVersion}";
            var requestBody = new
            {
                model = "dall-e-3",
                prompt = prompt,
                size = "1024x1024",
                style = "vivid",
                quality = "standard",
                n = 1
            };
            var response = _httpClient.PostAsJsonAsync(url, requestBody).Result;
            response.EnsureSuccessStatusCode();
            var dalleResponse = response.Content.ReadFromJsonAsync<DalleResponse>().Result;

            var imageUrl = dalleResponse?.data?.FirstOrDefault()?.url;

            if (string.IsNullOrEmpty(imageUrl))
                throw new Exception("No se pudo obtener la imagen generada");

            // Descargar la imagen
            var imageBytes = await _httpClientAux.GetByteArrayAsync(imageUrl);

            // Convertir a base64
            return Convert.ToBase64String(imageBytes);
        }

    }
}
