namespace RaceDay.Api.Services;

public interface IBlobService
{
    Task<string> UploadAsync(Stream content, string fileName, string contentType, string containerName = "raceday-images");
    Task DeleteAsync(string blobUrl, string containerName = "raceday-images");
}
