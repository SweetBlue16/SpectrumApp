/*using MetadataExtractor;
using MetadataExtractor.Formats.QuickTime;
using MetadataExtractor.Formats.Mp4;
using Microsoft.AspNetCore.Http;
using Spectrum.API.Exceptions;
using System.IO;
using System.Linq;

namespace Spectrum.API.Utilities
{
    /// <summary>
    /// Utility class for validating media files such as images and videos.
    /// </summary>
    public static class MediaValidationUtility
    {
        /// <summary>
        /// Validates if the uploaded file is a valid image (JPG, PNG) and does not exceed the maximum size.
        /// </summary>
        /// <param name="file">The uploaded file.</param>
        /// <param name="maxSizeMb">The maximum allowed size in megabytes.</param>
        public static void ValidateImage(IFormFile file, int maxSizeMb)
        {
            long maxSizeBytes = maxSizeMb * 1024 * 1024;
            
            if (file.Length > maxSizeBytes)
            {
                throw new SpectrumFileValidationException($"The image exceeds the maximum allowed size of {maxSizeMb} MB.");
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
            {
                throw new SpectrumFileValidationException("Invalid image format. Only JPG and PNG are allowed.");
            }
        }

        /// <summary>
        /// Validates if the uploaded file is a valid video (MP4, MOV), does not exceed the maximum size, and its duration is within the limit.
        /// </summary>
        /// <param name="file">The uploaded file.</param>
        /// <param name="maxSizeMb">The maximum allowed size in megabytes.</param>
        /// <param name="maxDurationSeconds">The maximum allowed duration in seconds.</param>
        public static void ValidateVideo(IFormFile file, int maxSizeMb, int maxDurationSeconds)
        {
            long maxSizeBytes = maxSizeMb * 1024 * 1024;
            
            if (file.Length > maxSizeBytes)
            {
                throw new SpectrumFileValidationException($"The video exceeds the maximum allowed size of {maxSizeMb} MB.");
            }

            var allowedExtensions = new[] { ".mp4", ".mov" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
            {
                throw new SpectrumFileValidationException("Invalid video format. Only MP4 and MOV are allowed.");
            }

            using var fileStream = file.OpenReadStream();
            var metadataDirectories = ImageMetadataReader.ReadMetadata(fileStream);

            double durationInSeconds = 0;

            if (fileExtension == ".mp4")
            {
                var mp4Directory = metadataDirectories.OfType<Mp4Directory>().FirstOrDefault();
                if (mp4Directory != null && 
                    mp4Directory.TryGetInt64(Mp4Directory.TagDuration, out long rawDuration) && 
                    mp4Directory.TryGetInt64(Mp4Directory.TagTimeScale, out long timeScale))
                {
                    durationInSeconds = (double)rawDuration / timeScale;
                }
            }
            else if (fileExtension == ".mov")
            {
                var movDirectory = metadataDirectories.OfType<QuickTimeMovieHeaderDirectory>().FirstOrDefault();
                if (movDirectory != null && 
                    movDirectory.TryGetInt64(QuickTimeMovieHeaderDirectory.TagDuration, out long rawDuration) && 
                    movDirectory.TryGetInt64(QuickTimeMovieHeaderDirectory.TagTimeScale, out long timeScale))
                {
                    durationInSeconds = (double)rawDuration / timeScale;
                }
            }

            if (durationInSeconds > maxDurationSeconds || durationInSeconds <= 0)
            {
                throw new SpectrumFileValidationException($"The video must have a valid duration and cannot exceed {maxDurationSeconds} seconds.");
            }
        }
    }
}*/