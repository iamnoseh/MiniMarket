using Microsoft.AspNetCore.Http;
using Serilog;

namespace Infrastructure.FileStorage;

public class FileStorage(string rootPath) : IFileStorage
{
    public async Task<string> SaveFile(IFormFile file, string relativeFolder)
    {
        try
        {
            var path = Path.Combine(rootPath,"wwwroot", relativeFolder);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var fullPath = Path.Combine(path, fileName);
            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);
            
            Log.Information("File {FileName} saved to {Folder}", fileName, relativeFolder);
            return Path.Combine(relativeFolder, fileName).Replace("\\", "/");
        }
        catch (Exception e)
        {
            Log.Error(e, "Error saving file to {Folder}", relativeFolder);
            throw;
        }
    }
    public Task DeleteFile(string? relativeFolder)
    {
        try
        {
            if (string.IsNullOrEmpty(relativeFolder)) return Task.CompletedTask;
            
            var path = Path.Combine(rootPath, "wwwroot", relativeFolder.Replace("/", Path.DirectorySeparatorChar.ToString()));
            if (File.Exists(path))
            {
                File.Delete(path);
                Log.Information("File {FilePath} deleted", relativeFolder);
            }
            return Task.CompletedTask;
        }
        catch (Exception e)
        {
            Log.Error(e, "Error deleting file {FilePath}", relativeFolder);
            throw;
        }
    }
}