using System.Net;
using Domain.DTOs.BrandDto;
using Domain.Entities;
using Domain.Responces;
using Infrastructure.Data;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

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
            var brand = await context.Brands.FirstOrDefaultAsync(x=> x.Id == dto.Id);
            if (brand == null) return new Responce<string>(HttpStatusCode.NotFound, "Brand not found");
            brand.Name = dto.Name;
            brand.UpdatedAt = DateTime.UtcNow;
            var res = await context.SaveChangesAsync();
            return res > 0
                ? new Responce<string>(HttpStatusCode.OK, "Brand successfully updated")
                : new Responce<string>(HttpStatusCode.BadRequest, "Brand not updated");
        }
        catch (Exception e)
        {
            return new Responce<string>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    public async Task<Responce<string>> DeleteBrand(int id)
    {
        try
        {
            var brand = await context.Brands.FirstOrDefaultAsync(x => x.Id == id);
            if (brand == null) return new Responce<string>(HttpStatusCode.NotFound, "Brand not found");
            context.Brands.Remove(brand);
            var res = await context.SaveChangesAsync();
            return res > 0
                ? new Responce<string>(HttpStatusCode.OK, "Brand successfully deleted")
                : new Responce<string>(HttpStatusCode.BadRequest, "Brand not deleted");
        }
        catch (Exception e)
        {
            return new Responce<string>(HttpStatusCode.InternalServerError,e.Message);
        }
    }

    public async Task<Responce<GetBrandDto>> GetBrand(int id)
    {
        try
        {
            var brand = await context.Brands.FirstOrDefaultAsync(x => x.Id == id);
            if (brand == null) return new Responce<GetBrandDto>(HttpStatusCode.NotFound, "Brand not found");
            var dto = new GetBrandDto()
            {
                Id = brand.Id,
                Name = brand.Name,
                CreatedAt = brand.CreatedAt,
                UpdatedAt = brand.UpdatedAt
            };
            return new Responce<GetBrandDto>(dto);
        }
        catch (Exception e)
        {
            return new Responce<GetBrandDto>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    public async Task<Responce<List<GetBrandDto>>> GetBrands()
    {
        try
        {
            var brands = await context.Brands.ToListAsync();
            if(brands.Count == 0)  return new Responce<List<GetBrandDto>>(HttpStatusCode.NotFound,"Brands not found");
            var dtos = brands.Select(x => new GetBrandDto()
            {
                Id = x.Id,
                Name = x.Name,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            }).ToList();
            return new Responce<List<GetBrandDto>>(dtos);
        }
        catch (Exception e)
        {
            return new Responce<List<GetBrandDto>>(HttpStatusCode.InternalServerError,e.Message);
        }
    }
}