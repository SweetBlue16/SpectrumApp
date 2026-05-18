using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Spectrum.API.Dtos.Media;
using Spectrum.API.Exceptions;
using Spectrum.API.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Spectrum.API.Services.Storage
{
    /// <summary>
    /// Defines the contract for chunked video uploads using AWS S3 Multipart Upload.
    /// Manages session initialization, chunk streaming, and file assembly.
    /// </summary>
    public interface IVideoStorageService
    {
        /// <summary>
        /// Starts a multipart upload session for a video file.
        /// </summary>
        Task<MultipartInitResponseDto> StartVideoUploadAsync(IFormFile file, string folder);

        /// <summary>
        /// Validates and uploads a specific video chunk to an active session.
        /// </summary>
        Task<string> UploadVideoChunkAsync(IFormFile chunk, string uploadId, int partNumber, string keyName);

        /// <summary>
        /// Merges all uploaded chunks into the final video file.
        /// </summary>
        Task<string> CompleteVideoUploadAsync(CompleteUploadRequestDto request);

        /// <summary>
        /// Deletes a specific video file from the AWS S3 bucket using its public URL.
        /// </summary>
        /// <param name="videoUrl">The full public URL of the video to be deleted.</param>
        /// <returns>A task that represents the asynchronous delete operation.</returns>
        Task DeleteVideoAsync(string videoUrl);
    }

    /// <summary>
    /// Implementation of <see cref="IVideoStorageService"/> using AWS SDK.
    /// </summary>
    public class VideoStorageService : IVideoStorageService
    {
        private readonly IAmazonS3 s3Client;
        private readonly string bucketName;
        private readonly string region;

        public VideoStorageService(IConfiguration config)
        {
            var credentials = new Amazon.Runtime.BasicAWSCredentials(
                config["AWS:AccessKey"],
                config["AWS:SecretKey"]
            );

            region = config["AWS:Region"];
            var regionEndpoint = Amazon.RegionEndpoint.GetBySystemName(region);

            s3Client = new AmazonS3Client(credentials, regionEndpoint);
            bucketName = config["AWS:BucketName"];
        }

        /// <inheritdoc />
        public async Task<MultipartInitResponseDto> StartVideoUploadAsync(IFormFile file, string folder)
        {
            MediaValidationUtility.ValidateVideo(file, maxSizeMb: 61, maxDurationSeconds: 16);

            string uniqueName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            string key = $"{folder}/{uniqueName}";

            var request = new InitiateMultipartUploadRequest
            {
                BucketName = bucketName,
                Key = key,
            };

            var response = await s3Client.InitiateMultipartUploadAsync(request);

            return new MultipartInitResponseDto
            {
                UploadId = response.UploadId,
                KeyName = key
            };
        }

        /// <inheritdoc />
        public async Task<string> UploadVideoChunkAsync(IFormFile chunk, string uploadId, int partNumber, string keyName)
        {
            if (chunk.Length > 6 * 1024 * 1024)
            {
                throw new SpectrumFileValidationException("The chunk size exceeds the 5MB limit.");
            }

            using var stream = chunk.OpenReadStream();
            var request = new UploadPartRequest
            {
                BucketName = bucketName,
                Key = keyName,
                UploadId = uploadId,
                PartNumber = partNumber,
                PartSize = chunk.Length,
                InputStream = stream
            };

            var response = await s3Client.UploadPartAsync(request);
            return response.ETag;
        }

        /// <inheritdoc />
        public async Task<string> CompleteVideoUploadAsync(CompleteUploadRequestDto request)
        {
            var partEtags = request.Etags
                .Select(e => new PartETag(e.PartNumber, e.ETag))
                .ToList();

            var awsRequest = new CompleteMultipartUploadRequest
            {
                BucketName = bucketName,
                Key = request.KeyName,
                UploadId = request.UploadId,
                PartETags = partEtags
            };

            await s3Client.CompleteMultipartUploadAsync(awsRequest);

            return $"https://{bucketName}.s3.{region}.amazonaws.com/{request.KeyName}";
        }

        /// <inheritdoc />
        public async Task DeleteVideoAsync(string videoUrl)
        {
            try
            {
                var uri = new Uri(videoUrl);
                string keyName = uri.AbsolutePath.TrimStart('/');

                var request = new DeleteObjectRequest
                {
                    BucketName = bucketName,
                    Key = keyName
                };

                await s3Client.DeleteObjectAsync(request);
            }
            catch (Exception ex)
            {
                throw new SpectrumServiceUnavailableException($"Failed to delete video from AWS S3: {ex.Message}");
            }
        }
    }
}