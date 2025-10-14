using Microsoft.EntityFrameworkCore;
using FastFoodCompareAppEnhanced_v3_1.Data;
using FastFoodCompareAppEnhanced_v3_1.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();
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

    if (!db.Users.Any())
    {
        db.Users.AddRange(new List<UserAccount>
        {
            new UserAccount { Id = 1, Username = "admin", Password = "123", FullName = "Quản trị viên" },
            new UserAccount { Id = 2, Username = "user", Password = "123", FullName = "Người dùng" },
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
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Menu}/{action=Index}/{id?}");

app.Run();
