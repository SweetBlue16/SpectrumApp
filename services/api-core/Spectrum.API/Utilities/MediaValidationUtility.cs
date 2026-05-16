using MetadataExtractor;
using MetadataExtractor.Formats.QuickTime;
using Microsoft.AspNetCore.Http;
using Spectrum.API.Exceptions;
using System.IO;
using System.Linq;

namespace Spectrum.API.Utilities
{
    /// <summary>
    /// Utility class for validating media files such as images and videos.
    /// Provides specialized methods for size, extension, and duration checks.
    /// </summary>
    public static class MediaValidationUtility
    {
        /// <summary>
        /// Orchestrates the validation for image files.
        /// </summary>
        public static void ValidateImage(IFormFile file, int maxSizeMb)
        {
            ValidateFileSize(file, maxSizeMb, "image");
            ValidateFileExtension(file, new[] { ".jpg", ".jpeg", ".png" }, "image");
        }

        /// <summary>
        /// Orchestrates the validation for video files, including duration metadata.
        /// </summary>
        public static void ValidateVideo(IFormFile file, int maxSizeMb, int maxDurationSeconds)
        {
            ValidateFileSize(file, maxSizeMb, "video");
            ValidateFileExtension(file, new[] { ".mp4", ".mov" }, "video");
            ValidateVideoDuration(file, maxDurationSeconds);
        }

        private static void ValidateFileSize(IFormFile file, int maxSizeMb, string type)
        {
            long maxSizeBytes = maxSizeMb * 1024 * 1024;
            if (file.Length > maxSizeBytes)
            {
                throw new SpectrumFileValidationException($"The {type} exceeds the maximum allowed size of {maxSizeMb} MB.");
            }
        }

        private static void ValidateFileExtension(IFormFile file, string[] allowedExtensions, string type)
        {
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                string joinedExtensions = string.Join(" and ", allowedExtensions.Select(e => e.Replace(".", "").ToUpper()));
                throw new SpectrumFileValidationException($"Invalid {type} format. Only {joinedExtensions} are allowed.");
            }
        }

        private static void ValidateVideoDuration(IFormFile file, int maxDurationSeconds)
        {
            double durationInSeconds = GetVideoDuration(file);

            if (durationInSeconds > maxDurationSeconds || durationInSeconds <= 0)
            {
                throw new SpectrumFileValidationException($"The video must have a valid duration and cannot exceed {maxDurationSeconds} seconds.");
            }
        }

        private static double GetVideoDuration(IFormFile file)
        {
            using var fileStream = file.OpenReadStream();
            var metadataDirectories = ImageMetadataReader.ReadMetadata(fileStream);

            var movieHeader = metadataDirectories.OfType<QuickTimeMovieHeaderDirectory>().FirstOrDefault();

            if (movieHeader != null &&
                movieHeader.TryGetInt64(QuickTimeMovieHeaderDirectory.TagDuration, out long rawDuration) &&
                movieHeader.TryGetInt64(QuickTimeMovieHeaderDirectory.TagTimeScale, out long timeScale))
            {
                return (double)rawDuration / timeScale;
            }

            return 0;
        }
    }
}