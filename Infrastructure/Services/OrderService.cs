using System.Net;
using Domain.DTOs.OrderDto;
using Domain.DTOs.OrderItemDto;
using Domain.Entities;
using Domain.Filters;
using Domain.Responces;
using Infrastructure.Data;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Extensions;
using Infrastructure.Mapping;
using Serilog;

namespace Infrastructure.Services;

public class OrderService(DataContext context): IOrderService
{
    public async Task<Responce<string>> CreateOrder(CreateOrderDto create, int userId)
    {
        try
        {
            Log.Information("Creating order");
            var cartItems = context.CartItems
                .Where(c => c.UserId == userId);
            
            if(!cartItems.Any()) return new Responce<string>(HttpStatusCode.BadRequest,"CartItems not found");
            var order = new Order()
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                Status = create.Status,
                Address = create.Address,
                PaymentMethod = create.PaymentMethod,
                TotalAmount = cartItems.Sum(c => c.Product!.Price * c.Quantity)
            };
            await context.Orders.AddAsync(order);
            var res = await context.SaveChangesAsync();
            if (res > 0) Log.Information("Order created for user {UserId}", userId);
            else Log.Warning("Order creation failed for user {UserId}", userId);

            return res > 0
                ? new Responce<string>(HttpStatusCode.OK,"Order created")
                : new Responce<string>(HttpStatusCode.BadRequest,"Order could not be created");
        }
        catch (Exception e)
        {
            Log.Error(e, "Error in CreateOrder");
           return new Responce<string>(HttpStatusCode.InternalServerError,e.Message);
        }
    }

    public async Task<Responce<string>> UpdateStatusOrder(UpdateOrderDto update)
    {
        try
        {
            Log.Information("Updating order");
            var update1 = await context.Orders.FirstOrDefaultAsync(x => x.Id == update.Id);
            if (update1 == null) return new Responce<string>(HttpStatusCode.NotFound, "Order not found");
            update1.Status = update.Status;
            update1.UpdatedAt = DateTime.UtcNow;
            var res = await context.SaveChangesAsync();
            if (res > 0) Log.Information("Order {OrderId} status updated to {Status}", update.Id, update.Status);
            else Log.Warning("Order {OrderId} status update failed", update.Id);

            return res > 0
                ? new Responce<string>(HttpStatusCode.OK, "Order updated")
                : new Responce<string>(HttpStatusCode.BadRequest, "Order could not be updated");
        }
        catch (Exception e)
        {
            Log.Error(e, "Error in UpdateStatusOrder");
            return new Responce<string>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    public async Task<Responce<string>> DeleteOrder(int id)
    {
        try
        {
            Log.Information("Deleting order {OrderId}", id);
            var order = await context.Orders.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (order == null) return new Responce<string>(HttpStatusCode.NotFound, "Order not found");
            
            order.IsDeleted = true;
            order.UpdatedAt = DateTime.UtcNow;
            
            var res = await context.SaveChangesAsync();
            if (res > 0) Log.Information("Order {OrderId} soft-deleted", id);
            else Log.Error("Order {OrderId} soft-delete failed", id);

            return res > 0
                ? new Responce<string>(HttpStatusCode.OK, "Order deleted")
                : new Responce<string>(HttpStatusCode.BadRequest, "Order could not be deleted");
        }
        catch (Exception e)
        {
            Log.Error(e, "Error in DeleteOrder");
            return new Responce<string>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    public async Task<PaginationResponce<List<GetOrderDto>>> GetOrders(OrderFilter filter)
    {
        try
        {
            Log.Information("Getting orders");
            var query = context.Orders
                .Where(o => !o.IsDeleted)
                .Include(x => x.OrderItems!)
                .ThenInclude(x => x.Product)
                .AsQueryable();
            if (filter.Id.HasValue)
            {
                query = query.Where(x => x.Id == filter.Id);
            }

            if (!string.IsNullOrEmpty(filter.Address))
            {
                query = query.Where(x => x.Address.Contains(filter.Address));
            }

            if (filter.UserId.HasValue)
            {
                query = query.Where(x => x.UserId == filter.UserId);
            }
            
            if (filter.TotalAmount.HasValue)
            {
                query = query.Where(x => x.TotalAmount == filter.TotalAmount);
            }

            if (filter.PaymentMethod.HasValue)
            {
                query = query.Where(x => x.PaymentMethod == filter.PaymentMethod);
            }

            if (filter.Status.HasValue)
            {
                query = query.Where(x => x.Status == filter.Status);
            }

            var (orders, total) = await query.ToPagedListAsync(filter.PageNumber, filter.PageSize);
            
            if(orders.Count == 0) return new PaginationResponce<List<GetOrderDto>>(HttpStatusCode.NotFound, "Order not found");
            
            var dtos = orders.Select(x => x.ToDto()).ToList();
            return new PaginationResponce<List<GetOrderDto>>(dtos, total, filter.PageNumber, filter.PageSize);
        }
        catch (Exception e)
        {
            Log.Error(e, "Error in GetOrders");
            return new PaginationResponce<List<GetOrderDto>>(HttpStatusCode.InternalServerError, e.Message);
        }
    }
    public async Task<Responce<List<GetOrderDto>>> GetOrdersByUserId(int userId)
    {
        try
        {
            Log.Information("Getting orders");
            var orders = await context.Orders
                .Where(o => !o.IsDeleted)
                .Include(o=>o.OrderItems)
                .ThenInclude(x => x.Product)
                .Where(o=> o.UserId == userId)
                .ToListAsync();
            if(orders.Count == 0) return new Responce<List<GetOrderDto>>(HttpStatusCode.NotFound,"Orders not found");
            var dtos = orders.Select(x => x.ToDto()).ToList();
            return new Responce<List<GetOrderDto>>(dtos);
        }
        catch (Exception e)
        {
            Log.Error(e, "Error in GetOrdersByUserId");
            return new Responce<List<GetOrderDto>>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    public async Task<Responce<GetOrderDto>> GetOrderById(int id)
    {
        try
        {
            Log.Information("Getting order {OrderId}", id);
            var  order = await context.Orders
                .Where(o => !o.IsDeleted)
                .Include(o => o.OrderItems)
                .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (order == null) return new Responce<GetOrderDto>(HttpStatusCode.NotFound, "Order not found");
            
            return new Responce<GetOrderDto>(order.ToDto());
        }
        catch (Exception e)
        {
            Log.Error(e, "Error in GetOrderById");
            return new Responce<GetOrderDto>(HttpStatusCode.InternalServerError,e.Message);
        }
    }
}