using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ProductBrief.Configurations;
using ProductBrief.Data;
using ProductBrief.Data.Repositories;
using ProductBrief.Facade;
using ProductBrief.Models;
using ProductBrief.Models.Validators;
using ProductBrief.Services;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();

builder.Services.AddDbContext<PurchaseTransactionDbContext>(options =>
    options.UseSqlite("Data Source=wex_product_brief.db"));

builder.Services.Configure<TreasuryApiSettings>(builder.Configuration.GetSection("TreasuryApi"));

builder.Services.AddSingleton(new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true
});

// Register repositories for loose coupling from DbContext
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IIdempotencyKeyRepository, IdempotencyKeyRepository>();

// Register services
builder.Services.AddScoped<IPurchaseTransactionService, PurchaseTransactionService>();
builder.Services.AddScoped<IIdempotencyService, IdempotencyService>();
builder.Services.AddScoped<IPurchaseTransactionFacade, PurchaseTransactionFacade>();

builder.Services.AddScoped<IValidator<CreatePurchaseTransactionRequest>, CreatePurchaseTransactionValidator>();
builder.Services.AddScoped<ITreasuryExchangeRateService, TreasuryExchangeRateService>();

builder.Services.AddHttpClient("TreasuryApi")
    .ConfigureHttpClient(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PurchaseTransactionDbContext>();
    dbContext.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ProductBrief API V1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
