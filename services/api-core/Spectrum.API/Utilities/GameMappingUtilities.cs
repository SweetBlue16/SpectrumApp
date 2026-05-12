namespace Spectrum.API.Utilities
{
    using Spectrum.API.Dtos.External;
    using Spectrum.API.Models;

    /// <summary>
    /// Provides helper methods to transform external data structures into internal models.
    /// </summary>
    public static class GameMappingUtilities
    {
        /// <summary>
        /// Maps a RawgGameDto object to a Game model instance.
        /// </summary>
        /// <param name="externalGame">The data received from the RAWG API.</param>
        /// <returns>A new instance of the Game model with mapped properties.</returns>
        public static Game MapToInternalModel(RawgGameDto externalGame)
        {
            return new Game
            {
                Id = Guid.NewGuid(),
                RawgId = externalGame.Id,
                Title = externalGame.Name,
                ReleaseDate = string.IsNullOrEmpty(externalGame.Released)
                    ? null
                    : DateTime.Parse(externalGame.Released),
                CoverImageUrl = externalGame.BackgroundImage,
                InternalRating = 0.0,
                GenreIds = externalGame.Genres?.Select(g => g.Id).ToList() ?? new List<int>(),
                PlatformIds = externalGame.ParentPlatforms?.Select(p => p.Platform.Id).ToList() ?? new List<int>()
            };
        }
    }
}