using Microsoft.AspNetCore.Http;
using Serilog;

namespace Infrastructure.FileStorage;

public class FileStorage(string rootPath) : IFileStorage
{
    private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    public async Task<string> SaveFile(IFormFile file, string relativeFolder)
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

        try
        {
            var path = Path.Combine(rootPath, "wwwroot", relativeFolder);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var fileName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(path, fileName);
            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);
            return Path.Combine(relativeFolder, fileName).Replace("\\", "/");
        }
        catch (Exception e)
        {
            Log.Error(e, "Error while saving file to storage");
            throw;
        }
    }

    public Task DeleteFile(string? relativeFolder)
    {
        if (string.IsNullOrWhiteSpace(relativeFolder))
        {
            return Task.CompletedTask;
        }

        try
        {
            var safeRelativePath = relativeFolder.Replace("/", Path.DirectorySeparatorChar.ToString());
            var path = Path.Combine(rootPath, "wwwroot", safeRelativePath);

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            return Task.CompletedTask;
        }
        catch (Exception e)
        {
            Log.Error(e, "Error while deleting file from storage");
            throw;
        }
    }
}