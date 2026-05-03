using Spectrum.API.Dtos.External;
using Spectrum.API.Exceptions;
using Spectrum.API.Utilities;

namespace Spectrum.API.Services.External
{
    /// <summary>
    /// Defines the contract for interacting with the external video games catalog (e.g., RAWG API).
    /// </summary>
    public interface IGameService
    {
        /// <summary>
        /// Searches for video games based on specified filters and pagination parameters.
        /// </summary>
        Task<PagedResult<RawgGameDto>> SearchGamesAsync(GameQueryDto queryDto);

        /// <summary>
        /// Retrieves detailed information for a specific video game.
        /// </summary>
        Task<RawgGameDto> GetGameDetailsAsync(int externalGameId);
    }

    /// <summary>
    /// Service implementation for communicating with the RAWG Video Games Database API.
    /// </summary>
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

        /// <inheritdoc />
        public async Task<RawgGameDto> GetGameDetailsAsync(int externalGameId)
        {
            try
            {
                var requestUrl = $"{GamesEndpoint}/{externalGameId}?key={_rawgApiKey}";
                var response = await _httpClient.GetFromJsonAsync<RawgGameDto>(requestUrl);
                
                if (response == null)
                {
                    throw new SpectrumNotFoundException(Constants.ErrorMessages.ResourceNotFound);
                }
                return response;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to get game details for external game ID: {ExternalGameId}", externalGameId);

                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new SpectrumNotFoundException(Constants.ErrorMessages.ResourceNotFound);
                }
                throw new SpectrumBusinessException(Constants.ErrorMessages.ExternalCatalogUnavailable);
            }
        }

        /// <inheritdoc />
        public async Task<PagedResult<RawgGameDto>> SearchGamesAsync(GameQueryDto queryDto)
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
                    ["page_size"] = queryDto.PageSize?.ToString() ?? DefaultPageSize.ToString(),
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
                throw new SpectrumBusinessException(Constants.ErrorMessages.ExternalCatalogUnavailable);
            }
        }
    }
}