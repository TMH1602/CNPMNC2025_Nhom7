using Microsoft.EntityFrameworkCore;
using FastFoodCompareAppEnhanced_v3_1.Data;
using FastFoodCompareAppEnhanced_v3_1.Models;

var builder = WebApplication.CreateBuilder(args);

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// BƯỚC 1: Thêm dịch vụ CORS (Cho phép gọi API Backend)
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            // Giữ nguyên HTTPS
            policy.WithOrigins("https://localhost:5000") 
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});


// Add services
builder.Services.AddHttpClient();
builder.Services.AddControllersWithViews(); // Hỗ trợ Controller/View
builder.Services.AddRazorPages();          // 🔥 Hỗ trợ Razor Pages (Cho Admin Area)

// Cấu hình In-Memory DB (Chỉ dùng cho các trang User View)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("FastFoodDb"));

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Seed data 
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    // Giữ lại Seed Data cho Món ăn (Dishes)
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

    // 🔥🔥 ĐÃ BỎ Seed Data cho UserAccount vì bạn lấy từ SQL Server thật
    
    db.SaveChanges();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

// BƯỚC 2: Kích hoạt CORS middleware
app.UseCors(MyAllowSpecificOrigins);

app.UseSession();
app.UseAuthorization(); 

// =======================================================================
// ROUTING CUỐI CÙNG CHO RAZOR PAGES VÀ MVC
// =======================================================================
app.UseEndpoints(endpoints =>
{
    // 🔥 1. ĐỊNH TUYẾN RAZOR PAGES (Ưu tiên cao nhất)
    // Dòng này giúp hệ thống tìm thấy các trang trong thư mục Pages (bao gồm cả Areas/Admin/Pages)
    endpoints.MapRazorPages(); 

    // 2. ROUTING CHO MVC AREAS (Dành cho các Controller MVC khác có thể tồn tại trong Areas)
    endpoints.MapControllerRoute(
        name: "areas",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
    );

    // 3. ROUTING MẶC ĐỊNH CHO MVC (Dành cho Menu/Cart)
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Menu}/{action=Index}/{id?}");
});
app.Run();