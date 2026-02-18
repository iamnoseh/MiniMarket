using System.Net;
using Domain.DTOs.UserDto;
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

public class UserService(DataContext context,
    IFileStorage file) : IUserService
{
    public async Task<Responce<string>> UpdateUser(UpdateUserDto user)
    {
        try
        {
            Log.Information("Updating user");
            var update = await context.Users.FirstOrDefaultAsync(x => x.Id == user.Id);
            if(update == null) return new Responce<string>(HttpStatusCode.NotFound, "User not found");
            if (user.AvatarUrl != null)
            {
                if (!string.IsNullOrEmpty(update.AvatarUrl))
                {
                    await file.DeleteFile(update.AvatarUrl);
                }
                await file.SaveFile(user.AvatarUrl, "UserAvatar");
            }
            update.FullName = user.FullName;
            update.Email = user.Email;
            update.Age = user.Age;
            update.Address = user.Address;
            update.PhoneNumber = user.PhoneNumber;
            update.UpdatedAt = DateTime.UtcNow;
            var res = await context.SaveChangesAsync();
            if (res > 0) Log.Information("User {UserId} updated", user.Id);
            else Log.Warning("User {UserId} update failed", user.Id);

            return res > 0
                ? new Responce<string>(HttpStatusCode.OK, "User successfully updated")
                : new Responce<string>(HttpStatusCode.NotFound, "User not update");
        }
        catch (Exception e)
        {
            Log.Error(e, "Error in UpdateUser");
            return new Responce<string>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    public async Task<Responce<string>> DeleteUser(int id)
    {
        try
        {
            Log.Information("Deleting user {UserId}", id);
            var user = await context.Users.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (user == null) return new Responce<string>(HttpStatusCode.NotFound, "User not found");
            
            user.IsDeleted = true;
            user.UpdatedAt = DateTime.UtcNow;
            
            var res = await context.SaveChangesAsync();
            if (res > 0)
            {
                Log.Information("User {UserId} soft-deleted", id);
            }
            else
            {
                Log.Error("User {UserId} soft-delete failed", id);
            }
            return res > 0
                ? new Responce<string>(HttpStatusCode.OK, "User deleted")
                : new Responce<string>(HttpStatusCode.BadRequest, "User could not be deleted");
        }
        catch (Exception e)
        {
            Log.Error(e, "Error in DeleteUser");
            return new Responce<string>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    public async Task<Responce<GetUserDto>> GetUser(int id)
    {
        try
        {
            Log.Information("Getting user {UserId}", id);
            var user = await context.Users.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (user == null) return new Responce<GetUserDto>(HttpStatusCode.NotFound, "User not found");
            
            return new Responce<GetUserDto>(user.ToDto());
        }
        catch (Exception e)
        {
            Log.Error(e, "Error in GetUser");
            return new Responce<GetUserDto>(HttpStatusCode.InternalServerError, e.Message);
        }
    }

    public async Task<PaginationResponce<List<GetUserDto>>> GetUsers(UserFilter filter)
    {
        try
        {
            Log.Information("Getting users");
            var query = context.Users.Where(u => !u.IsDeleted).AsQueryable();
            if (!string.IsNullOrEmpty(filter.FullName))
            {
                query = query.Where(x => x.FullName.Contains(filter.FullName));
            }

            if (!string.IsNullOrEmpty(filter.Email))
            {
                query = query.Where(x => x.Email.Contains(filter.Email));
            }

            if (!string.IsNullOrEmpty(filter.PhoneNumber))
            {
                query = query.Where(x => x.PhoneNumber.Contains(filter.PhoneNumber));
            }

            if (!string.IsNullOrEmpty(filter.Address))
            {
                query = query.Where(x => x.Address.Contains(filter.Address));
            }

            if (filter.Age.HasValue)
            {
                query = query.Where(x => x.Age == filter.Age);
            }

            query = query.Where(x => x.IsDeleted == false);
            var (users, total) = await query.ToPagedListAsync(filter.PageNumber, filter.PageSize);
            
            if(users.Count == 0) return new PaginationResponce<List<GetUserDto>>(HttpStatusCode.OK, "Users not found");
            
            var dtos = users.Select(x => x.ToDto()).ToList();
            return new PaginationResponce<List<GetUserDto>>(dtos, total, filter.PageNumber, filter.PageSize);
        }
        catch (Exception e)
        {
            Log.Error(e, "Error in GetUsers");
            return new PaginationResponce<List<GetUserDto>>(HttpStatusCode.InternalServerError, e.Message);
        }
    }
}