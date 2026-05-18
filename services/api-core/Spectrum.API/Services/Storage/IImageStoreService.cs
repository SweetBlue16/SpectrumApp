using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Spectrum.API.Utilities;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Spectrum.API.Services.Storage
{
    /// <summary>
    /// Defines the contract for direct, single-part image uploads to AWS S3.
    /// Handles validation and storage for profile pictures and review images.
    /// </summary>
    public interface IImageStorageService
    {
        /// <summary>
        /// Validates and uploads an image file to the specified S3 folder.
        /// </summary>
        /// <param name="file">The image file (JPG/PNG).</param>
        /// <param name="folder">The target folder name (e.g., "photoProfiles").</param>
        /// <returns>The public URL of the uploaded image.</returns>
        Task<string> UploadImageAsync(IFormFile file, string folder);
    }

    /// <summary>
    /// Implementation of <see cref="IImageStorageService"/> using AWS SDK.
    /// </summary>
    public class ImageStorageService : IImageStorageService
    {
        private readonly IAmazonS3 s3Client;
        private readonly string bucketName;
        private readonly string region;

        public ImageStorageService(IConfiguration config)
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
        public async Task<string> UploadImageAsync(IFormFile file, string folder)
        {
            MediaValidationUtility.ValidateImage(file, maxSizeMb: 6);

            string uniqueName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            string key = $"{folder}/{uniqueName}";

            using var stream = file.OpenReadStream();
            var request = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = key,
                InputStream = stream,
            };

            await s3Client.PutObjectAsync(request);

            return $"https://{bucketName}.s3.{region}.amazonaws.com/{key}";
        }
    }
}