namespace RaceDayAPI.Services
{
    public interface IBlobStorageService
    {
        /// <summary>Uploads the file to the named container and returns its public URL.</summary>
        /// <exception cref="InvalidOperationException">Azure storage is not configured.</exception>
        Task<string> UploadFileAsync(IFormFile file, string containerName);

        /// <summary>Best-effort delete of a blob previously returned by UploadFileAsync.</summary>
        Task DeleteAsync(string? blobUrl);
    }
}
