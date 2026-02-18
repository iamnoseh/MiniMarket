using System.Net;
using Domain.DTOs.ProductDto;
using Domain.Entities;
using Domain.Filters;
using Domain.Responces;
using Infrastructure.Data;
using Infrastructure.FileStorage;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Extensions;
using Infrastructure.Mapping;
using Serilog;

namespace Infrastructure.Services;

public class ProductService(DataContext context, 
    IFileStorage file) : IProductService
{
    public async Task<Responce<string>> CreateProduct(CreateProductDto create)
    {
        try
        { 
            Log.Information("Creating a new product");
            var newProduct = new Product()
            {
                Name = create.Name,
                Description = create.Description,
                Price = create.Price,
                Quantity = create.Quantity,
                CategoryId = create.CategoryId,
                BrandId = create.BrandId,
                AverageRating = 0,
                RatingCount = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false,
            };
            if (create.ImageUrl != null)
            {
                newProduct.ImageUrl = await file.SaveFile(create.ImageUrl,"Image");
            }
            await context.Products.AddAsync(newProduct);
            var res =  await context.SaveChangesAsync();
            if (res > 0) Log.Information("Product {ProductId} created", newProduct.Id);
            else Log.Warning("Product creation failed");

            return res > 0
                ? new Responce<string>(HttpStatusCode.Created,"Product created")
                : new Responce<string>(HttpStatusCode.BadRequest,"Product not created");
        }
        catch (Exception e)
        {
            Log.Error(e, "Error in CreateProduct");
            return new Responce<string>(HttpStatusCode.InternalServerError,e.Message);
        }
    }

    public async Task<Responce<string>> UpdateProduct(UpdateProductDto update)
    {
        try
        {
            Log.Information("Updating a new product");
            var product = await context.Products.FirstOrDefaultAsync(x=> x.Id == update.Id);
            if (product == null) return new Responce<string>(HttpStatusCode.NotFound,"Product not found");
            if (update.ImageUrl != null)
            {
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    await file.DeleteFile(product.ImageUrl);         
                }
                product.ImageUrl = await file.SaveFile(update.ImageUrl!,"Image");
            }
            product.Name = update.Name;
            product.Description = update.Description;
            product.Price = update.Price;
            product.Quantity = update.Quantity;
            product.CategoryId = update.CategoryId;
            product.UpdatedAt = DateTime.UtcNow;
            var res = await context.SaveChangesAsync();
            if (res > 0) Log.Information("Product {ProductId} updated", update.Id);
            else Log.Warning("Product {ProductId} update failed", update.Id);

            return res > 0
                ? new Responce<string>(HttpStatusCode.OK,"Product updated")
                : new Responce<string>(HttpStatusCode.BadRequest,"Product not updated");
        }
        catch (Exception e)
        {
            Log.Error(e, "Error in UpdateProduct");
            return new Responce<string>(HttpStatusCode.InternalServerError,e.Message);
        }
    }

    public async Task<Responce<string>> DeleteProduct(int id)
    {
        try
        {
            Log.Information("Deleting product {ProductId}", id);
            var product = await context.Products.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (product == null) return new Responce<string>(HttpStatusCode.NotFound, "Product not found");
            
            product.IsDeleted = true;
            product.UpdatedAt = DateTime.UtcNow;
            
            var res = await context.SaveChangesAsync();
            if (res > 0)
            {
                Log.Information("Product {ProductId} soft-deleted", id);
            }
            else
            {
                Log.Error("Product {ProductId} soft-delete failed", id);
            }
            return res > 0
                ? new Responce<string>(HttpStatusCode.OK, "Product deleted")
                : new Responce<string>(HttpStatusCode.BadRequest, "Product could not be deleted");
        }
        catch (Exception e)
        {
            Log.Error(e, "Error in DeleteProduct");
            return new Responce<string>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    public async Task<Responce<GetProductDto>> GetProductById(int id)
    {
        try
        {
            var product = await context.Products
                .Include(x=>x.Brand)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (product == null) return new Responce<GetProductDto>(HttpStatusCode.NotFound,"Product not found");
            
            return new Responce<GetProductDto>(product.ToDto());
        }
        catch (Exception e)
        {
            Log.Error(e, "Error in GetProductById");
            return new Responce<GetProductDto>(HttpStatusCode.InternalServerError,e.Message);
        }
    }

    public async Task<PaginationResponce<List<GetProductDto>>> GetProducts(ProductFilter filter)
    {
        try
        {
            Log.Information("Getting products");
            var query = context.Products.Where(p => !p.IsDeleted)
                .Include(x=>x.Brand)
                .AsQueryable();
            if (filter.Id.HasValue)
            {
                query = query.Where(x => x.Id == filter.Id);
            }

            if (!string.IsNullOrEmpty(filter.Name))
            {
                query = query.Where(x => x.Name.Contains(filter.Name));
            }

            if (!string.IsNullOrEmpty(filter.Description))
            {
                query = query.Where(x => x.Description.Contains(filter.Description));
            }

            if (filter.Price.HasValue)
            {
                query = query.Where(x => x.Price >= filter.Price);
            }

            if (filter.Quantity.HasValue)
            {
                query = query.Where(x => x.Quantity >= filter.Quantity);
            }

            if (filter.CategoryId.HasValue)
            {
                query = query.Where(x => x.CategoryId == filter.CategoryId);
            }

            if (filter.AverageRating.HasValue)
            {
                query = query.Where(x => x.AverageRating >= filter.AverageRating);
            }

            if (filter.RatingCount.HasValue)
            {
                query = query.Where(x => x.RatingCount == filter.RatingCount);
            }
            query = query.Where(x=> x.IsDeleted == false);
            var (products, total) = await query.ToPagedListAsync(filter.PageNumber, filter.PageSize);
            
            if(products.Count == 0) return new PaginationResponce<List<GetProductDto>>(HttpStatusCode.NotFound, "Product not found");
            
            var dtos = products.Select(x => x.ToDto()).ToList();
            return new PaginationResponce<List<GetProductDto>>(dtos, total, filter.PageNumber, filter.PageSize);
        }
        catch (Exception e)
        {
            Log.Error(e, "Error in GetProducts");
            return new PaginationResponce<List<GetProductDto>>(HttpStatusCode.InternalServerError,e.Message);
        }
    }
}