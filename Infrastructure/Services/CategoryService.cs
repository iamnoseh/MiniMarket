using System.Net;
using Domain.DTOs.CategoryDto;
using Domain.Entities;
using Domain.Responces;
using Infrastructure.Data;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Mapping;
using Serilog;

namespace Infrastructure.Services;

public class CategoryService(DataContext context) : ICategoryService
{
    public async Task<Responce<string>> UpdateCategory(UpdateCategoryDto dto)
    {
        try
        {
            Log.Information("Updating category {CategoryId}", dto.Id);
            var update = await context.Categories.FirstOrDefaultAsync(c => c.Id == dto.Id && !c.IsDeleted);
            if(update ==  null) return new Responce<string>(HttpStatusCode.NotFound,"Category not found");
            update.Name = dto.Name;
            update.Description = dto.Description;
            update.UpdatedAt = DateTime.UtcNow;
            var res = await context.SaveChangesAsync();
            if (res > 0) Log.Information("Category {CategoryId} updated", dto.Id);
            return res > 0
                ? new Responce<string>(HttpStatusCode.OK,"Category updated")
                : new Responce<string>(HttpStatusCode.BadRequest,"Category not updated");
        }
        catch (Exception e)
        {
            Log.Error(e, "Error updating Category");
            return new Responce<string>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    public async Task<Responce<string>> DeleteCategory(int id)
    {
        try
        {
            Log.Information("Deleting category {CategoryId}", id);
            var delete = await context.Categories.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
            if(delete == null) return new Responce<string>(HttpStatusCode.NotFound,"Category not found");
            
            delete.IsDeleted = true;
            delete.UpdatedAt = DateTime.UtcNow;

            var res = await context.SaveChangesAsync();
            if (res > 0) Log.Information("Category {CategoryId} soft-deleted", id);
            else Log.Error("Category {CategoryId} soft-delete failed", id);

            return res > 0
                ? new Responce<string>(HttpStatusCode.OK,"Category deleted")
                : new Responce<string>(HttpStatusCode.BadRequest,"Category could not be deleted");
        }
        catch (Exception e)
        {
            Log.Error(e, "Error deleting Category");
            return new Responce<string>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    public async Task<Responce<string>> CreateCategory(CreateCategoryDto category)
    {
        try
        {
            Log.Information("Creating category");
            var newCategory = new Category()
            {
                Name = category.Name,
                Description = category.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
             await context.Categories.AddAsync(newCategory);
             var res = await context.SaveChangesAsync();
             if (res > 0)
             {
                 Log.Information("Category created");
             }
             else
             {
                 Log.Fatal("Failed to create category");
             }
             return res > 0
                 ? new Responce<string>(HttpStatusCode.Created,"Category created")
                 : new Responce<string>(HttpStatusCode.BadRequest,"Category not created");
        }
        catch (Exception e)
        {
            Log.Error("Error in CreateCategory");
            return new Responce<string>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    public async Task<Responce<List<GetCategoryDto>>> GetCategory()
    {
        try
        {
            Log.Information("Getting categories");
            var categories = await context.Categories
                .Where(c => !c.IsDeleted)
                .ToListAsync();
            if (categories.Count == 0) return new Responce<List<GetCategoryDto>>(HttpStatusCode.NotFound, "Category not found");
            
            var dtos = categories.Select(c => c.ToDto()).ToList();
            return new Responce<List<GetCategoryDto>>(dtos);
        }
        catch (Exception e)
        {
            Log.Error(e, "Error in GetCategory");
            return new Responce<List<GetCategoryDto>>(HttpStatusCode.InternalServerError, e.Message);
        }
    }
}