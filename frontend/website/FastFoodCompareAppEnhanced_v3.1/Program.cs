using Microsoft.EntityFrameworkCore;
using FastFoodCompareAppEnhanced_v3_1.Data;
using FastFoodCompareAppEnhanced_v3_1.Models;

// 💡 1. THÊM CÁC THƯ VIỆN SAU (Quan trọng)
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// Thêm dịch vụ CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("https://localhost:5000") 
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});


// Add services
builder.Services.AddHttpClient();
builder.Services.AddControllersWithViews(); 
builder.Services.AddRazorPages();       

// Cấu hình In-Memory DB
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("FastFoodDb"));

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 💡 2. LẤY CẤU HÌNH JWT TỪ APPSETTINGS.JSON
// (Đảm bảo bạn đã copy "Jwt" section từ appsettings.json của Backend sang đây)
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrEmpty(jwtKey) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
{
    throw new InvalidOperationException("Cấu hình JWT (Key, Issuer, Audience) bị thiếu trong appsettings.json của Frontend.");
}

// 💡 3. DẠY FRONTEND CÁCH ĐỌC TOKEN (GIỐNG HỆT BACKEND)
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Cấu hình xác thực
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateLifetime = true,
        RoleClaimType = ClaimTypes.Role // Chỉ định claim chứa vai trò
    };

    // *** SỬA LỖI 401: DẠY MIDDLEWARE ĐỌC COOKIE ***
    // (Xóa OnForbidden và OnChallenge, chỉ giữ OnMessageReceived)
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Thử đọc token từ cookie có tên 'jwtToken'
            context.Token = context.Request.Cookies["jwtToken"];
            return Task.CompletedTask;
        }
    };
});

// 💡 4. DẠY FRONTEND CÁC POLICY (GIỐNG HỆT BACKEND)
// (Sử dụng tên vai trò chính xác từ database của bạn: "Admin", "Khách Hàng", "Nhà hàng")
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => 
        policy.RequireRole("Admin")); 

    options.AddPolicy("RestaurantOnly", policy => 
        policy.RequireRole("Nhà hàng"));
        
    options.AddPolicy("CustomerOnly", policy => 
        policy.RequireRole("Khách Hàng"));

    options.AddPolicy("AdminOrRestaurant", policy =>
        policy.RequireRole("Admin", "Nhà hàng"));
});


var app = builder.Build();

// Seed data 
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    if (!db.Dishes.Any())
    {
        db.Dishes.AddRange(new List<Dish>
        {
            new Dish { Id = 1, Name = "Burger Bò", Price = 5.99m, Calories = 750, Rating = 4.5m, Category = "Burger", ImageUrl = "/images/burger.jpg" },
            new Dish { Id = 2, Name = "Gà Rán (3 miếng)", Price = 6.49m, Calories = 950, Rating = 4.2m, Category = "Chicken", ImageUrl = "/images/friedchicken.jpg" },
            new Dish { Id = 3, Name = "Khoai Tây Chiên", Price = 2.49m, Calories = 300, Rating = 4.0m, Category = "Sides", ImageUrl = "/images/fries.jpg" },
            new Dish { Id = 4, Name = "Salad Tươi", Price = 3.99m, Calories = 180, Rating = 3.8m, Category = "Salad", ImageUrl = "/images/salad.jpg" },
            new Dish { Id = 5, Name = "Pizza Mini", Price = 7.99m, Calories = 850, Rating = 4.6m, Category = "Pizza", ImageUrl = "/images/pizza.jpg" },
        });
    }
    
    db.SaveChanges();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

// 💡 BƯỚC 5: THÊM MIDDLEWARE XỬ LÝ LỖI (Cách mới)
// Middleware này phải nằm sau UseRouting và trước UseEndpoints
app.UseStatusCodePagesWithReExecute("/Account/HandleError", "?code={0}");

app.UseCors(MyAllowSpecificOrigins);
app.UseSession();

// 💡 BƯỚC 6: KÍCH HOẠT MIDDLEWARE (ĐÚNG THỨ TỰ)
app.UseAuthentication(); // 1. Xác thực (Đọc token)
app.UseAuthorization();  // 2. Phân quyền (Kiểm tra vai trò)

// =======================================================================
// ROUTING CUỐI CÙNG CHO RAZOR PAGES VÀ MVC
// =======================================================================
app.UseEndpoints(endpoints =>
{
    endpoints.MapRazorPages(); 

    endpoints.MapControllerRoute(
        name: "areas",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
    );

    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Menu}/{action=Index}/{id?}");
});


app.Run();