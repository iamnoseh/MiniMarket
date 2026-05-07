using Microsoft.AspNetCore.Http;

namespace Infrastructure.FileStorage;

public interface IImageStorageService
{
    Task<string> SaveAsync(IFormFile file, string folder, CancellationToken cancellationToken = default);
    Task DeleteAsync(string? imageUrlOrPath);
    Task<string> ReplaceAsync(string? oldImageUrlOrPath, IFormFile newFile, string folder, CancellationToken cancellationToken = default);
    string BuildImageUrl(string relativePath);
    string NormalizeImageUrl(string? imageUrlOrPath);
}
