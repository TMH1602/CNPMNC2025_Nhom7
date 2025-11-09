using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
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
builder.Services.AddAuthentication(options =>
{
    // Đặt scheme mặc định
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Cấu hình cách xác thực token
    options.TokenValidationParameters = new TokenValidationParameters
    {
        // Yêu cầu xác thực Issuer (người phát hành)
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"], // Đọc từ appsettings

        // Yêu cầu xác thực Audience (người dùng)
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"], // Đọc từ appsettings

        // Yêu cầu xác thực Lifetime (thời gian hết hạn)
        ValidateLifetime = true,

        // Yêu cầu xác thực và dùng Secret Key
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]) // Đọc key từ appsettings
        )
    };
});
builder.Services.AddAuthorization();
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
builder.Services.AddScoped<IVnPayService, VnPayService>();
builder.Services.AddScoped<IVnPayService2, VnPayService2>();
builder.Services.AddScoped<IEmailService, EmailService>();
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
