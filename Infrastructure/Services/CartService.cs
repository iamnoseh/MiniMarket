using System.Net;
using System.Security.Claims;
using Domain.DTOs.CartItemDto;
using Domain.Entities;
using Domain.Responces;
using Infrastructure.Data;
using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Infrastructure.Services;

public class CartService(DataContext context) : ICartService
{
   public async Task<Responce<string>> AddToCart(CreateCartItemDto create, int userId)
{
    try
    {
        Log.Information("Adding product {ProductId} to cart for user {UserId}", create.ProductId, userId);
        if (create.Quantity < 1)
            return new Responce<string>(HttpStatusCode.BadRequest, "Quantity must be greater than 0");
        
        var cartItem = await context.CartItems.FirstOrDefaultAsync(
            x => x.UserId == userId && x.ProductId == create.ProductId);

        if (cartItem != null)
        {
            cartItem.Quantity += create.Quantity;
            cartItem.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
            return new Responce<string>(HttpStatusCode.OK, "Product quantity updated successfully");
        }
        
        var newCartItem = new CartItem
        {
            UserId = userId,
            ProductId = create.ProductId,
            Quantity = create.Quantity,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        await context.CartItems.AddAsync(newCartItem);
        var result = await context.SaveChangesAsync();

        return result > 0
            ? new Responce<string>(HttpStatusCode.Created, "CartItem successfully added")
            : new Responce<string>(HttpStatusCode.BadRequest, "Cart item could not be added");
    }
    catch (Exception e)
    {
        Log.Error("Error in AddToCart");
        return new Responce<string>(HttpStatusCode.InternalServerError, e.Message);
    }
}

    public async Task<Responce<string>> UpdateCart(UpdateCartItemDto update, int userId)
    {
        try
        {
            Log.Information("Updating product {ProductId} in cart for user {UserId}", update.ProductId, userId);
            
            var updatedCart = await context.CartItems.FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == update.ProductId);
            if (updatedCart == null) return new Responce<string>(HttpStatusCode.NotFound, "CartItem not found");
            updatedCart.Quantity = update.Quantity;
            updatedCart.UpdatedAt = DateTime.UtcNow;
            var res = await context.SaveChangesAsync();
            if (res > 0)
            {
                Log.Information("Updating cart");
            }
            else
            {
                Log.Fatal("Failed to update cart");
            }
            return res > 0
                ? new Responce<string>(HttpStatusCode.OK, "CartItem successfully updated")
                : new Responce<string>(HttpStatusCode.BadRequest, "Error");
        }
        catch (Exception e)
        {
            Log.Error("Error in  UpdateCart");
            return new Responce<string>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    public async Task<Responce<string>> DeleteCart(int id, int userId)
    {
        try
        {
            Log.Information("Deleting cart item {Id} for user {UserId}", id, userId);
            
            var deletedCart = await context.CartItems.FirstOrDefaultAsync(x => x.UserId == userId && x.Id == id);
            
            if (deletedCart == null) return new Responce<string>(HttpStatusCode.NotFound, "CartItem not found");
            context.CartItems.Remove(deletedCart);
            var res = await context.SaveChangesAsync();
            if (res > 0)
            {
                Log.Information("Deleting cart");
            }
            else
            {
                Log.Fatal("Failed to delete cart");
            }
            return res > 0
                ? new Responce<string>(HttpStatusCode.OK, "CartItem successfully deleted")
                : new Responce<string>(HttpStatusCode.BadRequest, "Error");
        }
        catch (Exception e)
        {
            Log.Error("Error in  DeleteCart");
            return new Responce<string>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    public async Task<Responce<List<GetCartItemDto>>> GetCartItem(int userId)
    {
        try
        {
            Log.Information("Getting cart for user {UserId}", userId);
            
            var cartItems = await context.CartItems
                .Where(x => x.UserId == userId)
                .ToListAsync();
            if(cartItems.Count == 0) return new Responce<List<GetCartItemDto>>(HttpStatusCode.NotFound, "CartItem not found");
            var dtos = cartItems.Select(x => new GetCartItemDto()
            {
                Id = x.Id,
                UserId = x.UserId,
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            }).ToList();
            return new Responce<List<GetCartItemDto>>(dtos);
        }
        catch (Exception e)
        {
            Log.Error("Error in GetCartItem");
            return new Responce<List<GetCartItemDto>>(HttpStatusCode.InternalServerError, e.Message);
        }
    }
}
