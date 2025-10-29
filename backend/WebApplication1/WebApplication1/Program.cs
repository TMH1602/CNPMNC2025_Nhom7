using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Cấu hình Entity Framework Core ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString)); // Sử dụng SQL Server provider


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendOrigin",
        builder =>
        {
            builder.WithOrigins("https://localhost:5000", "https://trinidad-avid-unappetisingly.ngrok-free.dev", "https://10.0.2.2:5000")
                   .AllowAnyMethod()
                   .AllowAnyHeader();
            // Nếu bạn cần gửi cookie hoặc chứng chỉ, hãy thêm: .AllowCredentials();
        });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<IVnPayService, VnPayService>();
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        DbInitializer.Initialize(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.UseCors("AllowFrontendOrigin");


app.UseAuthorization();

app.MapControllers();

app.Run();
