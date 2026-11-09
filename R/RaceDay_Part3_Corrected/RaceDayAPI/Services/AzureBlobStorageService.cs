using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDayAPI.Services
{
    public class AzureBlobStorageService : IBlobStorageService
    {
        private readonly string _connectionString;
        private readonly ILogger<AzureBlobStorageService> _logger;

        public AzureBlobStorageService(IConfiguration configuration, ILogger<AzureBlobStorageService> logger)
        {
            _connectionString = configuration["AzureStorage:ConnectionString"] ?? string.Empty;
            _logger = logger;
        }

        private BlobServiceClient CreateClient()
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
                throw new InvalidOperationException(
                    "Azure Blob Storage is not configured. Set AzureStorage:ConnectionString (user secrets or environment variable).");
            return new BlobServiceClient(_connectionString);
        }

        public async Task<string> UploadFileAsync(IFormFile file, string containerName)
        {
            if (file == null || file.Length == 0) throw new ArgumentException("File is empty.", nameof(file));

            var container = CreateClient().GetBlobContainerClient(containerName);
            // Public read at blob level so <img src> on the event page can load the image.
            await container.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var blob = container.GetBlobClient($"{Guid.NewGuid()}{Path.GetExtension(file.FileName).ToLowerInvariant()}");
            await using var stream = file.OpenReadStream();
            await blob.UploadAsync(stream, new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType }
            });
            return blob.Uri.ToString();
        }

        public async Task DeleteAsync(string? blobUrl)
        {
            if (string.IsNullOrWhiteSpace(blobUrl) || string.IsNullOrWhiteSpace(_connectionString)) return;
            try
            {
                var uri = new Uri(blobUrl);
                var client = CreateClient();
                // Only delete blobs that belong to this storage account.
                if (!string.Equals(uri.Host, client.Uri.Host, StringComparison.OrdinalIgnoreCase)) return;
                var parts = uri.AbsolutePath.Trim('/').Split('/', 2);
                if (parts.Length != 2) return;
                await client.GetBlobContainerClient(parts[0]).DeleteBlobIfExistsAsync(Uri.UnescapeDataString(parts[1]));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not delete old blob {Url}", blobUrl);
            }
        }
    }
}
