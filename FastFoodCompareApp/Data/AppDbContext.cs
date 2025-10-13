using Microsoft.EntityFrameworkCore;
using FastFoodCompareApp.Models;

namespace FastFoodCompareApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Dish> Dishes { get; set; } = null!;
    }
}
