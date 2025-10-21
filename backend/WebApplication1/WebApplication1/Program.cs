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

// ====================================================================
// START: Cấu hình CORS
// ====================================================================
// 1. Định nghĩa Policy CORS
builder.Services.AddCors(options =>
{
    // Đặt tên Policy là "AllowFrontendOrigin"
    options.AddPolicy("AllowFrontendOrigin",
        builder =>
        {
            // Cho phép truy cập từ nguồn gốc (Origin) của Frontend: http://localhost:5000
            // Đây là bước quan trọng nhất để sửa lỗi CORS.
            builder.WithOrigins("https://localhost:5000")
                   // Cho phép tất cả các phương thức (GET, POST, PUT, DELETE, v.v.)
                   .AllowAnyMethod()
                   // Cho phép tất cả các tiêu đề (headers), bao gồm cả Authorization
                   .AllowAnyHeader();
            // Nếu bạn cần gửi cookie hoặc chứng chỉ, hãy thêm: .AllowCredentials();
        });
});
// ====================================================================
// END: Cấu hình CORS
// ====================================================================

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
var app = builder.Build();


// ====================================================================
// KHỞI TẠO DỮ LIỆU MẪU (Data Seeding) - Đặt tại đây
// ====================================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Gọi hàm Initialize tĩnh trong DbInitializer để chèn dữ liệu mẫu
        DbInitializer.Initialize(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}
// ====================================================================


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ====================================================================
// START: Kích hoạt CORS Policy
// ====================================================================
// 2. Kích hoạt Middleware CORS. 
// Lệnh này PHẢI đặt sau app.UseRouting() và trước app.UseAuthorization()
app.UseCors("AllowFrontendOrigin");
// ====================================================================
// END: Kích hoạt CORS Policy
// ====================================================================

app.UseAuthorization();

app.MapControllers();

app.Run();
