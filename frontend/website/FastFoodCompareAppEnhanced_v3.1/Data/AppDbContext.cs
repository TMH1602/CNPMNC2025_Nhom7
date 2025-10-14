using Microsoft.EntityFrameworkCore;
using FastFoodCompareAppEnhanced_v3_1.Models;

namespace FastFoodCompareAppEnhanced_v3_1.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Dish> Dishes { get; set; } = null!;
        public DbSet<UserAccount> Users { get; set; } = null!;
    }
}
