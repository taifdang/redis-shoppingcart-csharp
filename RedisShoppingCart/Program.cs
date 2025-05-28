using Microsoft.EntityFrameworkCore;
using Server.API.Data;
using Server.API.Middleware;
using Server.API.Models;
using Server.API.Repositories;
using Server.API.Sevices;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//config
builder.Services.AddDbContext<DatabaseContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Database"));
});
//
builder.Services.AddSingleton<IConnectionMultiplexer>(options =>
  ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")));

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});
//
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<GetUserContext>();
//
builder.Services.AddSession();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
using var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
context.Database.Migrate();
if (!context.products.Any())
{
    context.products.AddRange(
        new Products { name = "Coca-cola", price = 10, stock = 10 },
        new Products { name = "Pepsi", price = 5, stock = 10 },
        new Products { name = "Fanta", price = 4, stock = 10 }
    );
    context.SaveChanges();
}

app.UseHttpsRedirection();

app.UseAuthorization();
//use session
app.UseSession();
//
app.UseCheckSession();

app.MapControllers();

app.Run();
