namespace RaceDayAPI.Services
{
    /// <summary>
    /// Development fallback used when AzureStorage:ConnectionString is not set.
    /// Saves files under wwwroot/uploads/{container}/ and returns an absolute URL
    /// on the API host so the MVC pages can show the image.
    /// </summary>
    public class LocalBlobStorageService : IBlobStorageService
    {
        private const string UrlPrefix = "/uploads/";
        private readonly string _root;
        private readonly IHttpContextAccessor _http;
        private readonly ILogger<LocalBlobStorageService> _logger;

        public LocalBlobStorageService(IWebHostEnvironment env, IHttpContextAccessor http,
            ILogger<LocalBlobStorageService> logger)
        {
            var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
            _root = Path.GetFullPath(Path.Combine(webRoot, "uploads"));
            _http = http;
            _logger = logger;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string containerName)
        {
            if (file == null || file.Length == 0) throw new ArgumentException("File is empty.", nameof(file));

            var folder = Path.Combine(_root, containerName);
            Directory.CreateDirectory(folder);

            var name = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName).ToLowerInvariant()}";
            await using (var target = File.Create(Path.Combine(folder, name)))
            await using (var source = file.OpenReadStream())
                await source.CopyToAsync(target);

            var req = _http.HttpContext?.Request;
            var baseUrl = req == null ? string.Empty : $"{req.Scheme}://{req.Host}";
            return $"{baseUrl}{UrlPrefix}{containerName}/{name}";
        }

        public Task DeleteAsync(string? blobUrl)
        {
            if (string.IsNullOrWhiteSpace(blobUrl)) return Task.CompletedTask;
            try
            {
                var path = Uri.TryCreate(blobUrl, UriKind.Absolute, out var uri) ? uri.AbsolutePath : blobUrl;
                if (!path.StartsWith(UrlPrefix, StringComparison.OrdinalIgnoreCase)) return Task.CompletedTask;

                var relative = Uri.UnescapeDataString(path[UrlPrefix.Length..]).Replace('/', Path.DirectorySeparatorChar);
                var full = Path.GetFullPath(Path.Combine(_root, relative));
                // Never delete outside the uploads folder.
                if (full.StartsWith(_root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) && File.Exists(full))
                    File.Delete(full);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not delete old local file {Url}", blobUrl);
            }
            return Task.CompletedTask;
        }
    }
}
