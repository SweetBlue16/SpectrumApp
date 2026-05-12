using Spectrum.API.Dtos.External;
using Spectrum.API.Models;
using Spectrum.API.Utilities;
using System.Text.Json;

namespace Spectrum.API.Services.Cache
{
    /// <summary>
    /// Defines the contract for managing the in-memory video game catalog.
    /// Provides methods for initialization, fast searching, and direct retrieval.
    /// </summary>
    public interface IGameMemoryCache
    {
        /// <summary>
        /// Loads the game catalog from a JSON snapshot into memory.
        /// </summary>
        /// <param name="filePath">The physical path to the JSON snapshot file.</param>
        void Initialize(string filePath);

        /// <summary>
        /// Performs advanced filtering, ordering, and pagination on the in-memory catalog.
        /// </summary>
        /// <param name="query">Data transfer object containing all search and filter criteria.</param>
        /// <returns>A paged result containing the subset of games matching the criteria.</returns>
        PagedResult<Game> Search(GameQueryDto query);

        /// <summary>
        /// Retrieves a specific game by its external RAWG numeric identifier.
        /// </summary>
        /// <param name="rawgId">The numeric ID assigned by the RAWG API.</param>
        /// <returns>The matching game if found; otherwise, null.</returns>
        Game? GetByRawgId(int rawgId);
    }

    /// <summary>
    /// Implementation of the in-memory cache that handles a high-volume game catalog 
    /// using LINQ to Objects for rapid data processing.
    /// </summary>
    public class GameMemoryCache : IGameMemoryCache
    {
        /// <summary>
        /// The master list of games loaded from the snapshot.
        /// This acts as the "source of truth" during the application's lifecycle.
        /// </summary>
        private List<Game> _catalog = new();

        /// <inheritdoc />
        public void Initialize(string filePath)
        {
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                _catalog = JsonSerializer.Deserialize<List<Game>>(json) ?? new();
            }
        }

        /// <inheritdoc />
        public Game? GetByRawgId(int rawgId)
            => _catalog.FirstOrDefault(g => g.RawgId == rawgId);

        /// <inheritdoc />
        public PagedResult<Game> Search(GameQueryDto query)
        {
            var filtered = _catalog.AsQueryable();

            if (!string.IsNullOrEmpty(query.Search))
            {
                filtered = filtered.Where(g => g.Title.Contains(query.Search, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(query.Genres))
            {
                var genreIds = query.Genres.Split(',')
                                           .Select(s => int.TryParse(s, out int id) ? id : (int?)null)
                                           .Where(id => id.HasValue)
                                           .Select(id => id!.Value)
                                           .ToList();

                filtered = filtered.Where(g => g.GenreIds.Any(id => genreIds.Contains(id)));
            }

            if (!string.IsNullOrEmpty(query.Platforms))
            {
                var platformIds = query.Platforms.Split(',')
                                                 .Select(s => int.TryParse(s, out int id) ? id : (int?)null)
                                                 .Where(id => id.HasValue)
                                                 .Select(id => id!.Value)
                                                 .ToList();

                filtered = filtered.Where(g => g.PlatformIds.Any(id => platformIds.Contains(id)));
            }

            filtered = query.Ordering switch
            {
                "name" => filtered.OrderBy(g => g.Title),
                "-name" => filtered.OrderByDescending(g => g.Title),
                "released" => filtered.OrderBy(g => g.ReleaseDate),
                "-released" => filtered.OrderByDescending(g => g.ReleaseDate),
                "rating" => filtered.OrderBy(g => g.InternalRating), 
                _ => filtered.OrderByDescending(g => g.InternalRating)
            };

            var total = filtered.Count();
            var pageSize = query.PageSize ?? 20;

            var items = filtered.Skip((query.Page - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();

            return new PagedResult<Game>
            {
                Items = items,
                TotalCount = total,
                Page = query.Page,
                PageSize = pageSize
            };
        }
    }
}