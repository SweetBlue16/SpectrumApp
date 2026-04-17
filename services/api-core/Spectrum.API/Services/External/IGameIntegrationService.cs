using Spectrum.API.Dtos.External;
using Spectrum.API.Exceptions;

namespace Spectrum.API.Services.External
{
    public interface IGameIntegrationService
    {
        Task<IEnumerable<RawgGameDto>> SearchGamesAsync(string query);
        Task<RawgGameDto> GetGameDetailsAsync(int externalGameId);
    }

    public class GameIntegrationService : IGameIntegrationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _rawgApiKey;
        private readonly ILogger<GameIntegrationService> _logger;

        private const int DefaultPageSize = 20;
        private const string GamesEndpoint = "games";

        public GameIntegrationService(HttpClient httpClient, IConfiguration configuration, ILogger<GameIntegrationService> logger)
        {
            _httpClient = httpClient;
            _rawgApiKey = configuration["RawgApi:ApiKey"] 
                ?? throw new ArgumentNullException("RawgApi:ApiKey is not configured.");
            _logger = logger;
        }

        public async Task<RawgGameDto> GetGameDetailsAsync(int externalGameId)
        {
            try
            {
                var requestUrl = $"{GamesEndpoint}/{externalGameId}?key={_rawgApiKey}";
                var response = await _httpClient.GetFromJsonAsync<RawgGameDto>(requestUrl);
                if (response == null)
                {
                    throw new SpectrumNotFoundException("External videogame", externalGameId);
                }
                return response;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to get game details for external game ID: {ExternalGameId}", externalGameId);
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new SpectrumNotFoundException($"External videogame", externalGameId);
                }
                throw new SpectrumBusinessException("Could not retrieve game details from external service. Please try again later.");
            }
        }

        public async Task<IEnumerable<RawgGameDto>> SearchGamesAsync(string query)
        {
            try
            {
                var requestUrl = $"{GamesEndpoint}?key={_rawgApiKey}&search={Uri.EscapeDataString(query)}&page_size={DefaultPageSize}";
                var response = await _httpClient.GetFromJsonAsync<RawgResponseDto>(requestUrl);
                return response?.Results ?? [];
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to search games with query: {Query}", query);
                throw new SpectrumBusinessException("Could not retrieve game data from external service. Please try again later.");
            }
        }
    }
}
