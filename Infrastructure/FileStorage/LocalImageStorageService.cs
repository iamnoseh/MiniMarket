using Microsoft.AspNetCore.Http;
using Serilog;

namespace Infrastructure.FileStorage;

public class LocalImageStorageService : IImageStorageService
{
    private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;
    private readonly string _webRootPath;
    private readonly string _uploadsRootPath;

    public LocalImageStorageService(string contentRootPath)
    {
        _webRootPath = Path.GetFullPath(Path.Combine(contentRootPath, "wwwroot"));
        _uploadsRootPath = Path.Combine(_webRootPath, "uploads");
    }

    public async Task<string> SaveAsync(IFormFile file, string folder, CancellationToken cancellationToken = default)
    {
        ValidateFile(file);

        var safeFolder = NormalizeSafeRelativePath(folder);
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var directoryPath = Path.Combine(_uploadsRootPath, safeFolder);
        Directory.CreateDirectory(directoryPath);

        var fullPath = Path.GetFullPath(Path.Combine(directoryPath, fileName));
        EnsurePathIsInsideRoot(fullPath, _uploadsRootPath);

        await using var stream = new FileStream(fullPath, FileMode.CreateNew);
        await file.CopyToAsync(stream, cancellationToken);

        return BuildImageUrl($"uploads/{safeFolder}/{fileName}");
    }

    public async Task<string> ReplaceAsync(
        string? oldImageUrlOrPath,
        IFormFile newFile,
        string folder,
        CancellationToken cancellationToken = default)
    {
        var newImageUrl = await SaveAsync(newFile, folder, cancellationToken);
        await DeleteAsync(oldImageUrlOrPath);
        return newImageUrl;
    }

    public Task DeleteAsync(string? imageUrlOrPath)
    {
        foreach (var relativePath in GetDeleteCandidates(imageUrlOrPath))
        {
            try
            {
                var safeRelativePath = NormalizeSafeRelativePath(relativePath);
                var fullPath = Path.GetFullPath(Path.Combine(_webRootPath, safeRelativePath));
                EnsurePathIsInsideRoot(fullPath, _webRootPath);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch (Exception e) when (e is ArgumentException or IOException or UnauthorizedAccessException)
            {
                Log.Warning(e, "Could not delete image {ImagePath}", imageUrlOrPath);
            }
        }

        return Task.CompletedTask;
    }

    public string BuildImageUrl(string relativePath)
    {
        return NormalizeImageUrl(relativePath);
    }

    public string NormalizeImageUrl(string? imageUrlOrPath)
    {
        var path = ExtractPath(imageUrlOrPath);
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        path = TrimWebRootPrefix(path);
        path = path.TrimStart('/');

        if (!path.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase))
        {
            path = $"uploads/{path}";
        }

        return $"/{NormalizeSafeRelativePath(path)}";
    }

    private static void ValidateFile(IFormFile file)
    {
        if (file is null)
        {
            throw new ArgumentNullException(nameof(file));
        }

        if (file.Length == 0)
        {
            throw new ArgumentException("File is empty.", nameof(file));
        }

        if (file.Length > MaxFileSizeBytes)
        {
            throw new ArgumentException("File is too large.", nameof(file));
        }

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedImageExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException("File type is not allowed.", nameof(file));
        }
    }

    private static IEnumerable<string> GetDeleteCandidates(string? imageUrlOrPath)
    {
        var path = ExtractPath(imageUrlOrPath);
        if (string.IsNullOrWhiteSpace(path))
        {
            yield break;
        }

        path = TrimWebRootPrefix(path).TrimStart('/');

        if (path.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase))
        {
            yield return path;
            yield break;
        }

        yield return $"uploads/{path}";
        yield return path;
    }

    private static string ExtractPath(string? imageUrlOrPath)
    {
        if (string.IsNullOrWhiteSpace(imageUrlOrPath))
        {
            return string.Empty;
        }

        var path = imageUrlOrPath.Trim().Replace('\\', '/');
        if (Uri.TryCreate(path, UriKind.Absolute, out var uri))
        {
            path = uri.AbsolutePath;
        }

        var queryIndex = path.IndexOfAny(['?', '#']);
        if (queryIndex >= 0)
        {
            path = path[..queryIndex];
        }

        return Uri.UnescapeDataString(path);
    }

    private static string TrimWebRootPrefix(string path)
    {
        path = path.TrimStart('/');
        return path.StartsWith("wwwroot/", StringComparison.OrdinalIgnoreCase)
            ? path["wwwroot/".Length..]
            : path;
    }

    private static string NormalizeSafeRelativePath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            throw new ArgumentException("Path cannot be empty.", nameof(relativePath));
        }

        var normalized = relativePath.Trim().Replace('\\', '/').Trim('/');
        var segments = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length == 0)
        {
            throw new ArgumentException("Path cannot be empty.", nameof(relativePath));
        }

        foreach (var segment in segments)
        {
            if (segment is "." or ".." || segment.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                throw new ArgumentException("Path is not safe.", nameof(relativePath));
            }
        }

        return string.Join('/', segments);
    }

    private static void EnsurePathIsInsideRoot(string fullPath, string rootPath)
    {
        var normalizedRoot = Path.GetFullPath(rootPath).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!fullPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Path is outside of the storage root.", nameof(fullPath));
        }
    }
}
