using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using WholesaleRetailStore.Data;
using Microsoft.AspNetCore.Cors;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        }));

// Register business services
builder.Services.AddScoped<WholesaleRetailStore.Services.PricingService>();
builder.Services.AddScoped<WholesaleRetailStore.Services.IntelligentSearchService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS services
builder.Services.AddCors();

// Add SPA services
builder.Services.AddSpaStaticFiles(configuration =>
{
    configuration.RootPath = "frontend/build";
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Serve React app with no-cache headers for development
app.UseSpaStaticFiles();

// Add no-cache headers for HTML files to prevent caching issues
app.Use(async (context, next) =>
{
    if (context.Request.Path.Value.EndsWith(".html") || context.Request.Path.Value == "/")
    {
        context.Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
        context.Response.Headers.Add("Pragma", "no-cache");
        context.Response.Headers.Add("Expires", "0");
    }
    await next();
});

app.UseRouting();
app.UseAuthorization();

// Configure CORS
app.UseCors(options => 
{
    options.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader();
});

// Map API routes only
app.MapControllers();

// Serve React app for all non-API routes
app.MapFallbackToFile("index.html");

app.Run();
