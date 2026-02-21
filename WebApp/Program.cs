using Domain.Entities;
using Infrastructure.Data.Seeder;
using Domain.DTOs.EmailDto;
using Infrastructure.Data;
using Infrastructure.ExtensionMethods;
using Infrastructure.FileStorage;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Debug)
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day,
        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)
    .Enrich.FromLogContext()
    .MinimumLevel.Debug()
    .CreateLogger();

builder.Services.RegisterDataContext(builder.Configuration);

builder.Services.RegisterService();


builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IFileStorage>(sp => new FileStorage(builder.Environment.ContentRootPath));

builder.Services.AddHttpContextAccessor();

builder.Services.RegisterIdentity();

builder.Services.RegisterSwagger();

builder.Services.RegisterJwt(builder.Configuration);

builder.Services.AddAuthorization(opt => opt.AddPolicy("AdminOnly", p => p.RequireRole("Admin")));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["*"];
        if (allowedOrigins.Contains("*")) policy.AllowAnyOrigin();
        else policy.WithOrigins(allowedOrigins);
        policy.AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseMiddleware<WebApp.Middlewares.ExceptionMiddleware>();

app.UseStaticFiles();


app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Market API v1");
    c.RoutePrefix = "swagger"; 
});

app.UseCors("AllowSpecificOrigins");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
using (var scope = app.Services.CreateScope())
{
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
