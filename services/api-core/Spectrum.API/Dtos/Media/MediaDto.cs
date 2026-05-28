using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Spectrum.API.Dtos.Media
{
    /// <summary>
    /// Response DTO containing the necessary identifiers to start uploading video chunks.
    /// </summary>
    public class MultipartInitResponseDto
    {
        public string UploadId { get; set; } = string.Empty;
        public string KeyName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request DTO for completing a multipart upload, containing all chunk identifiers and database record metadata.
    /// </summary>
    public class CompleteUploadRequestDto
    {
        [Required]
        public string UploadId { get; set; } = string.Empty;

        [Required]
        public string KeyName { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Range(1, int.MaxValue)]
        public int GameId { get; set; }

        [MinLength(1)]
        public List<PartEtagDto> Etags { get; set; } = new();
    }

    /// <summary>
    /// Represents a single chunk's sequence number and its AWS ETag identifier.
    /// </summary>
    public class PartEtagDto
    {
        public int PartNumber { get; set; }
        public string ETag { get; set; } = string.Empty;
    }
}
