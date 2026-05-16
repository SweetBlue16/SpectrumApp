using System.Collections.Generic;

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
        public string UploadId { get; set; } = string.Empty;
        public string KeyName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid GameId { get; set; }

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