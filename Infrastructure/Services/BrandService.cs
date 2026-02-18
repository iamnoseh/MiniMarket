using System.Net;
using Domain.DTOs.BrandDto;
using Domain.Entities;
using Domain.Responces;
using Infrastructure.Data;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Mapping;
using Serilog;

namespace Infrastructure.Services;

public class BrandService(DataContext context) : IBrandService
{
    public async Task<Responce<string>> CreateBrand(CreateBrandDto dto)
    {
        try
        {
            var newBrand = new Brand
            {
                Name = dto.Name,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            await context.Brands.AddAsync(newBrand);
            var res = await context.SaveChangesAsync();
            return res > 0
                ? new Responce<string>(HttpStatusCode.Created,"Brand created")
                : new Responce<string>(HttpStatusCode.BadRequest,"Brand not created");
        }
        catch (Exception e)
        {
            return new Responce<string>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    public async Task<Responce<string>> UpdateBrand(UpdateBrandDto dto)
    {
        try
        {
            Log.Information("Updating brand {BrandId}", dto.Id);
            var brand = await context.Brands.FirstOrDefaultAsync(x=> x.Id == dto.Id && !x.IsDeleted);
            if (brand == null) return new Responce<string>(HttpStatusCode.NotFound, "Brand not found");
            brand.Name = dto.Name;
            brand.UpdatedAt = DateTime.UtcNow;
            var res = await context.SaveChangesAsync();
            if (res > 0) Log.Information("Brand {BrandId} updated", dto.Id);
            return res > 0
                ? new Responce<string>(HttpStatusCode.OK, "Brand successfully updated")
                : new Responce<string>(HttpStatusCode.BadRequest, "Brand not updated");
        }
        catch (Exception e)
        {
            Log.Error(e, "Error updating Brand");
            return new Responce<string>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    public async Task<Responce<string>> DeleteBrand(int id)
    {
        try
        {
            Log.Information("Deleting brand {BrandId}", id);
            var brand = await context.Brands.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (brand == null) return new Responce<string>(HttpStatusCode.NotFound, "Brand not found");
            
            brand.IsDeleted = true;
            brand.UpdatedAt = DateTime.UtcNow;

            var res = await context.SaveChangesAsync();
            if (res > 0) Log.Information("Brand {BrandId} soft-deleted", id);
            else Log.Error("Brand {BrandId} soft-delete failed", id);

            return res > 0
                ? new Responce<string>(HttpStatusCode.OK, "Brand successfully deleted")
                : new Responce<string>(HttpStatusCode.BadRequest, "Brand not deleted");
        }
        catch (Exception e)
        {
            Log.Error(e, "Error deleting Brand");
            return new Responce<string>(HttpStatusCode.InternalServerError,e.Message);
        }
    }

    public async Task<Responce<GetBrandDto>> GetBrand(int id)
    {
        try
        {
            Log.Information("Getting brand {BrandId}", id);
            var brand = await context.Brands.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (brand == null) return new Responce<GetBrandDto>(HttpStatusCode.NotFound, "Brand not found");
            
            return new Responce<GetBrandDto>(brand.ToDto());
        }
        catch (Exception e)
        {
            Log.Error(e, "Error getting Brand");
            return new Responce<GetBrandDto>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    public async Task<Responce<List<GetBrandDto>>> GetBrands()
    {
        try
        {
            Log.Information("Getting brands");
            var brands = await context.Brands
                .Where(b => !b.IsDeleted)
                .ToListAsync();
            if(brands.Count == 0)  return new Responce<List<GetBrandDto>>(HttpStatusCode.NotFound,"Brands not found");
            
            var dtos = brands.Select(x => x.ToDto()).ToList();
            return new Responce<List<GetBrandDto>>(dtos);
        }
        catch (Exception e)
        {
            Log.Error(e, "Error getting Brands");
            return new Responce<List<GetBrandDto>>(HttpStatusCode.InternalServerError,e.Message);
        }
    }
}