using Spectrum.API.Dtos.External;
using Spectrum.API.Exceptions;

namespace Spectrum.API.Services.External
{
    public interface IGameIntegrationService
    {
        Task<IEnumerable<RawgGameDto>> SearchGamesAsync(GameQueyDto queryDto);
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
                    throw new SpectrumNotFoundException("resourceNotFound");
                }
                return response;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to get game details for external game ID: {ExternalGameId}", externalGameId);
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new SpectrumNotFoundException("resourceNotFound");
                }
                throw new SpectrumBusinessException("externalCatalogUnavailable");
            }
        }

        public async Task<IEnumerable<RawgGameDto>> SearchGamesAsync(GameQueyDto queryDto)
        {
            try
            {
                var queryParams = new Dictionary<string, string>
                {
                    ["key"] = _rawgApiKey,
                    ["search"] = queryDto.Search,
                    ["platforms"] = queryDto.Platforms,
                    ["genres"] = queryDto.Genres,
                    ["ordering"] = queryDto.Ordering,
                    ["page_size"] = queryDto.PageSize.ToString(),
                    ["page"] = queryDto.Page.ToString()
                };

                var queryString = string.Join("&", queryParams
                    .Where(p => !string.IsNullOrEmpty(p.Value))
                    .Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value!)}"));
                var response = await _httpClient.GetFromJsonAsync<RawgResponseDto>($"{GamesEndpoint}?{queryString}");
                return (IEnumerable<RawgGameDto>)(response ?? new RawgResponseDto { Results = new List<RawgGameDto>() });
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to search games with RAWG API");
                throw new SpectrumBusinessException("externalCatalogUnavailable");
            }
        }
    }
}
