using System.Text;
using Domain.Entities;
using Infrastructure.Data.Seeder;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Domain.DTOs.EmailDto;
using Infrastructure.Data;
using Infrastructure.FileStorage;
using Infrastructure.Interfaces;
using Infrastructure.Interfaces.IProducts___ICategories;
using Infrastructure.Interfaces.Reviews___Ratings;
using Infrastructure.Services;
using Infrastructure.Services.EmailServices;
using Infrastructure.Services.Products___Categories;
using Infrastructure.Services.Reviews___Ratings;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;

// ✅ Serilog Configuration
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Debug)
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day,
        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)
    .Enrich.FromLogContext()
    .MinimumLevel.Debug()
    .CreateLogger();

builder.Services.AddDbContext<DataContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<DataContext>();

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderItemService, OrderItemService>();
builder.Services.AddScoped<IReviewsRatings, ReviewsRatings>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFileStorage>(sp => new FileStorage(builder.Environment.ContentRootPath));
builder.Services.AddHttpContextAccessor();

builder.Services
    .AddIdentityCore<User>(opt =>
    {
        opt.Password.RequiredLength = 8;
        opt.Password.RequireNonAlphanumeric = false;
        opt.Password.RequireUppercase = false;
        opt.Password.RequireLowercase = false;
        opt.Password.RequireDigit = false;
        opt.User.RequireUniqueEmail = true;
    })
    .AddRoles<IdentityRole<int>>()
    .AddEntityFrameworkStores<DataContext>()
    .AddSignInManager();

builder.Services.AddScoped<IAccountService, AccountService>();

builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new() { Title = "Market API", Version = "v1" });
    var scheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter JWT Bearer token"
    };
    opt.AddSecurityDefinition("Bearer", scheme);
    opt.AddSecurityRequirement(new()
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new List<string>()
        }
    });
});

var jwt = builder.Configuration.GetSection("JWT");
var jwtKey = jwt["Key"];
var jwtIssuer = jwt["Issuer"];
var jwtAudience = jwt["Audience"];

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException("JWT:Key is not configured");
}

if (string.IsNullOrWhiteSpace(jwtIssuer))
{
    throw new InvalidOperationException("JWT:Issuer is not configured");
}

if (string.IsNullOrWhiteSpace(jwtAudience))
{
    throw new InvalidOperationException("JWT:Audience is not configured");
}

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.RequireHttpsMetadata = !env.IsDevelopment();
        opt.SaveToken = true;
        opt.TokenValidationParameters = new()
        {
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization(opt => opt.AddPolicy("AdminOnly", p => p.RequireRole("Admin")));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCors", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

        if (env.IsDevelopment() || allowedOrigins is null || allowedOrigins.Length == 0)
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
        else
        {
            policy
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Market API v1");
    c.RoutePrefix = "swagger";
});

app.UseCors("DefaultCors");
app.UseAuthentication();
app.UseAuthorization();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    try
    {
        var data = services.GetRequiredService<DataContext>();
        await data.Database.MigrateAsync();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
        await Seed.SeedRole(roleManager);

        var userManager = services.GetRequiredService<UserManager<User>>();
        await Seed.SeedAdmin(userManager, roleManager);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Database migration or seeding failed on startup");
    }
}

app.Run();
