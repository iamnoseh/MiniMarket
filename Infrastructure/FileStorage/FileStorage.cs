using Microsoft.AspNetCore.Http;

namespace Infrastructure.FileStorage;

public class FileStorage(IImageStorageService imageStorageService) : IFileStorage
{
    public async Task<string> SaveFile(IFormFile file, string relativeFolder)
    {
        return await imageStorageService.SaveAsync(file, relativeFolder);
    }

    public async Task DeleteFile(string? relativeFolder)
    {
        await imageStorageService.DeleteAsync(relativeFolder);
    }
}
