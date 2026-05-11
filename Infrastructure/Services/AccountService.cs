using System.Net;
using Domain.DTOs.Account;
using Domain.DTOs.EmailDto;
using Domain.Entities;
using Domain.Responses;
using Infrastructure.FileStorage;
using Infrastructure.Helpers;
using Infrastructure.Interfaces;
using Infrastructure.Services.EmailServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Infrastructure.Services
{
    public class AccountService(
        UserManager<User> userManager,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration,
        IEmailService emailService,
        IImageStorageService imageStorage) : IAccountService
    {
        public async Task<Responce<string>> Register(Register register)
        {
            try
            {
                Log.Information("Registering new account");
                var existingUser = await userManager.FindByNameAsync(register.UserName);
                if (existingUser != null)
                    return new Responce<string>(HttpStatusCode.BadRequest, "User already exists");
                var existingEmail = await userManager.FindByEmailAsync(register.Email);
                if (existingEmail != null)
                    return new Responce<string>(HttpStatusCode.BadRequest, "Email already exists");
                var user = new User
                {
                    FullName = register.FullName,
                    UserName = register.UserName,
                    Email = register.Email,
                    Address = register.Address,
                    PhoneNumber = register.PhoneNumber,
                    Age = register.Age,
                };
                if (register.ProfileImage != null)
                {
                    user.AvatarUrl = await imageStorage.SaveAsync(register.ProfileImage, "UserAvatar");
                }
                var result = await userManager.CreateAsync(user, register.Password);
                await userManager.AddToRoleAsync(user, "Customer");
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return new Responce<string>(HttpStatusCode.BadRequest, errors);
                }
                
                return new Responce<string>("Customer created successfully");

            }
            catch (ArgumentException e)
            {
                Log.Warning(e, "Invalid profile image upload");
                return new Responce<string>(HttpStatusCode.BadRequest, e.Message);
            }
            catch (Exception e)
            {
                Log.Error(e, "Error in Register");
                return new Responce<string>(HttpStatusCode.InternalServerError, "Something went wrong");
            }
        }

        public async Task<Responce<string>> Login(LoginDto login)
        {
            try
            {
                Log.Information("Logining account");
                var user = await userManager.FindByNameAsync(login.UserName);
                if (user == null)
                    return new Responce<string>(HttpStatusCode.NotFound, "Номи корбар ё рамз нодуруст аст");
                var isPasswordValid = await userManager.CheckPasswordAsync(user, login.Password);
                if (!isPasswordValid)
                    return new Responce<string>(HttpStatusCode.Unauthorized, "Номи корбар ё рамз нодуруст аст");
                var token = await GenerateJwtTokenHelper.GenerateJwtToken(user, userManager, configuration);
                return new Responce<string>(token) { Message = "Воридшавӣ бо муваффақият анҷом ёфт" };
            }
            catch (Exception e)
            {
                Log.Error(e, "Error in Login");
                return new Responce<string>(HttpStatusCode.InternalServerError, "Something went wrong");
            }
        }

        public async Task<Responce<string>> ChangePassword(ChangePassword changePassword)
        {
            try
            {
                Log.Information("Changing password");
                var userClaims = httpContextAccessor.HttpContext?.User.FindFirst("UserId")?.Value
                                 ?? httpContextAccessor.HttpContext?.User.FindFirst("NameId")?.Value;
                var userId = int.TryParse(userClaims, out var id);

                var user = userManager.Users.FirstOrDefault(x => x.Id == id);
                if (user == null)
                    return new Responce<string>(HttpStatusCode.BadRequest, "Something went wrong");
                var res = await userManager.ChangePasswordAsync(user, changePassword.OldPassword,
                    changePassword.Password);
                if (!res.Succeeded) return new Responce<string>(HttpStatusCode.OK, "Your password not changed");
                return new Responce<string>(HttpStatusCode.OK, "Your password has been changed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in ChangePassword");
                return new Responce<string>(HttpStatusCode.InternalServerError,
                    "Хатогӣ ҳангоми ивазкунии рамз");
            }
        }
    }
}
