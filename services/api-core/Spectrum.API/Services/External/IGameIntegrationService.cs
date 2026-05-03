using Spectrum.API.Dtos.External;
using Spectrum.API.Exceptions;
using Spectrum.API.Utilities;

namespace Spectrum.API.Services.External
{
    public interface IGameService
    {
        Task<PagedResult<RawgGameDto>> SearchGamesAsync(GameQueyDto queryDto);
        Task<RawgGameDto> GetGameDetailsAsync(int externalGameId);
    }

    public class GameService : IGameService
    {
        private readonly HttpClient _httpClient;
        private readonly string _rawgApiKey;
        private readonly ILogger<GameService> _logger;

        private const int DefaultPageSize = 20;
        private const string GamesEndpoint = "games";

        public GameService(HttpClient httpClient, IConfiguration configuration, ILogger<GameService> logger)
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
                if (response == null) throw new SpectrumNotFoundException("resourceNotFound");
                return response;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to get game details for external game ID: {ExternalGameId}", externalGameId);
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound) throw new SpectrumNotFoundException("resourceNotFound");
                throw new SpectrumBusinessException("externalCatalogUnavailable");
            }
        }

        public async Task<PagedResult<RawgGameDto>> SearchGamesAsync(GameQueyDto queryDto)
        {
            try
            {
                var queryParams = new Dictionary<string, string>
                {
                    ["key"] = _rawgApiKey,
                    ["search"] = queryDto.Search ?? "",
                    ["search_precise"] = "true",
                    ["platforms"] = queryDto.Platforms ?? "",
                    ["genres"] = queryDto.Genres ?? "",
                    ["ordering"] = queryDto.Ordering ?? "",
                    ["page_size"] = queryDto.PageSize?.ToString(),
                    ["page"] = queryDto.Page.ToString()
                };

                var queryString = string.Join("&", queryParams
                    .Where(p => !string.IsNullOrEmpty(p.Value))
                    .Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value!)}"));

                var response = await _httpClient.GetFromJsonAsync<RawgResponseDto>($"{GamesEndpoint}?{queryString}");

                return new PagedResult<RawgGameDto>
                {
                    Items = response?.Results ?? Enumerable.Empty<RawgGameDto>(),
                    TotalCount = response != null ? response.Count : 0,
                    Page = queryDto.Page,
                    PageSize = queryDto.PageSize ?? DefaultPageSize
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to search games with RAWG API");
                throw new SpectrumBusinessException("externalCatalogUnavailable");
            }
        }
    }
}
