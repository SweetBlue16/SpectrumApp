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
        /// Searches for video games based on specified filters, keywords, and pagination parameters.
        /// </summary>
        /// <param name="queryDto">The data transfer object containing search, filter, and pagination criteria.</param>
        /// <returns>A collection of <see cref="RawgGameDto"/> matching the search criteria. Returns an empty collection if no results are found.</returns>
        /// <exception cref="SpectrumBusinessException">Thrown if the external catalog service is unavailable or unreachable.</exception>
        Task<IEnumerable<RawgGameDto>> SearchGamesAsync(GameQueryDto queryDto);

        /// <summary>
        /// Retrieves extensive and detailed information for a specific video game using its external identifier.
        /// </summary>
        /// <param name="externalGameId">The unique numeric identifier assigned by the external RAWG provider.</param>
        /// <returns>A <see cref="RawgGameDto"/> containing the detailed information of the requested game.</returns>
        /// <exception cref="SpectrumNotFoundException">Thrown if the requested game does not exist in the external catalog.</exception>
        /// <exception cref="SpectrumBusinessException">Thrown if the external catalog service is unavailable.</exception>
        Task<RawgGameDto> GetGameDetailsAsync(int externalGameId);
    }

    /// <summary>
    /// Service implementation for communicating with the external RAWG Video Games Database API.
    /// </summary>
    public class GameService : IGameService
    {
        /// <summary>
        /// HTTP client configured with the RAWG base address.
        /// </summary>
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Private API key stored in secure configuration.
        /// </summary>
        private readonly string _rawgApiKey;

        private readonly ILogger<GameService> _logger;

        private const int DefaultPageSize = 20;
        private const string GamesEndpoint = "games";

        /// <summary>
        /// Initializes a new instance of the <see cref="GameService"/> class.
        /// </summary>
        /// <param name="httpClient">The HTTP client configured with the RAWG base address.</param>
        /// <param name="configuration">The application configuration to securely retrieve the API key.</param>
        /// <param name="logger">The logger instance for tracking external API requests and errors.</param>
        /// <exception cref="ArgumentNullException">Thrown if the RAWG API key is not configured in the application settings.</exception>
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
        public async Task<IEnumerable<RawgGameDto>> SearchGamesAsync(GameQueryDto queryDto)
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
                    ["page_size"] = queryDto.PageSize.ToString() ?? DefaultPageSize.ToString(),
                    ["page"] = queryDto.Page.ToString()
                };

                var queryString = string.Join("&", queryParams
                    .Where(p => !string.IsNullOrEmpty(p.Value))
                    .Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value!)}"));
                var response = await _httpClient.GetFromJsonAsync<RawgResponseDto>($"{GamesEndpoint}?{queryString}");
                //return (IEnumerable<RawgGameDto>)(response ?? new RawgResponseDto { Results = new List<RawgGameDto>() });
                return response?.Results ?? Enumerable.Empty<RawgGameDto>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to search games with RAWG API");
                throw new SpectrumBusinessException(Constants.ErrorMessages.ExternalCatalogUnavailable);
            }
        }
    }
}
