using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace HRHiringSystem.Application.Interfaces;

public interface IBlobStorageService
{
    /// <summary>
    /// Uploads a file stream to blob storage under the specified blob path and returns the public URL.
    /// </summary>
    Task<string> UploadFileAsync(string blobPath, Stream content, string contentType, CancellationToken cancellationToken = default);
}
