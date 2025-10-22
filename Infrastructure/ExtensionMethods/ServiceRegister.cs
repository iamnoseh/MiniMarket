using Infrastructure.Data;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.IProducts___ICategories;
using Infrastructure.Interfaces.Reviews___Ratings;
using Infrastructure.Services;
using Infrastructure.Services.EmailServices;
using Infrastructure.Services.Products___Categories;
using Infrastructure.Services.Reviews___Ratings;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.ExtensionMethods;

public static class ServiceRegister
{
    public static void RegisterService(this IServiceCollection services)
    {
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IOrderItemService, OrderItemService>();
        services.AddScoped<IReviewsService, ReviewsService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<DataContext>();
    }
}