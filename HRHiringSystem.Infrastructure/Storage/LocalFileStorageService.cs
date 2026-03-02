using HRHiringSystem.Application.Interfaces;

namespace HRHiringSystem.Infrastructure.Storage;

/// <summary>
/// Local file system storage service for development environments.
/// Stores uploaded files in a local directory and returns a local URL.
/// </summary>
public class LocalFileStorageService : IBlobStorageService
{
    private readonly string _basePath;
    private readonly string _baseUrl;

    public LocalFileStorageService(string basePath, string baseUrl)
    {
        _basePath = basePath;
        _baseUrl = baseUrl.TrimEnd('/');
        
        // Ensure base directory exists
        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
        }
    }

    public async Task<string> UploadFileAsync(string blobPath, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        // Create the full local file path
        var fullPath = Path.Combine(_basePath, blobPath.Replace('/', Path.DirectorySeparatorChar));
        
        // Ensure the directory exists
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Write the file
        using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
        await content.CopyToAsync(fileStream, cancellationToken);

        // Return a URL that can be used to access the file
        var url = $"{_baseUrl}/uploads/{blobPath.Replace('\\', '/')}";
        return url;
    }
}
