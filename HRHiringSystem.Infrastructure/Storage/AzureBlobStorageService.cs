using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using HRHiringSystem.Application.Interfaces;

namespace HRHiringSystem.Infrastructure.Storage;

public class AzureBlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _client;
    private readonly string _containerName;

    public AzureBlobStorageService(BlobServiceClient client, string containerName = "jobapplications")
    {
        _client = client;
        _containerName = containerName;
    }

    public async Task<string> UploadFileAsync(string blobPath, System.IO.Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        var container = _client.GetBlobContainerClient(_containerName);
        await container.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);

        var blob = container.GetBlobClient(blobPath);

        var httpHeaders = new BlobHttpHeaders { ContentType = contentType };

        await blob.UploadAsync(content, httpHeaders, cancellationToken: cancellationToken);

        return blob.Uri.ToString();
    }
}
