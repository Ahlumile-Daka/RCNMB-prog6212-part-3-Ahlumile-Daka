using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace RaceDay.Api.Services;

public class BlobService : IBlobService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<BlobService> _logger;

    public BlobService(IConfiguration config, ILogger<BlobService> logger)
    {
        var connectionString = config["AzureStorage:ConnectionString"];
        if (string.IsNullOrWhiteSpace(connectionString) ||
            connectionString.StartsWith("YOUR_", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "AzureStorage:ConnectionString is not configured. Set it with user-secrets or the " +
                "AzureStorage__ConnectionString environment variable (see README).");
        }

        _blobServiceClient = new BlobServiceClient(connectionString);
        _logger = logger;
    }

    private async Task<BlobContainerClient> GetContainerAsync(string containerName)
    {
        var container = _blobServiceClient.GetBlobContainerClient(containerName);
        try
        {
            // Public read at blob level so <img src="..."> works in the MVC site.
            await container.CreateIfNotExistsAsync(PublicAccessType.Blob);
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == "PublicAccessNotPermitted")
        {
            _logger.LogWarning("Storage account blocks anonymous access; creating private container {Container}. " +
                               "Images will not display until 'Allow Blob anonymous access' is enabled.", containerName);
            await container.CreateIfNotExistsAsync(PublicAccessType.None);
        }
        return container;
    }

    public async Task<string> UploadAsync(Stream content, string fileName, string contentType, string containerName = "raceday-images")
    {
        var container = await GetContainerAsync(containerName);

        var uniqueName = $"{Guid.NewGuid():N}_{Path.GetFileName(fileName)}";
        var blob = container.GetBlobClient(uniqueName);

        await blob.UploadAsync(content, new BlobHttpHeaders { ContentType = contentType });

        _logger.LogInformation("Uploaded blob {Name} to {Container}", uniqueName, containerName);
        return blob.Uri.ToString();
    }

    public async Task DeleteAsync(string blobUrl, string containerName = "raceday-images")
    {
        if (string.IsNullOrWhiteSpace(blobUrl)) return;
        try
        {
            var uri = new Uri(blobUrl);
            var blobName = Path.GetFileName(uri.LocalPath);
            var container = _blobServiceClient.GetBlobContainerClient(containerName);
            await container.DeleteBlobIfExistsAsync(blobName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete blob {Url}", blobUrl);
        }
    }
}
