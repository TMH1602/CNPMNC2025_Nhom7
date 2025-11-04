using FastFoodChain.API.Models;
using Microsoft.EntityFrameworkCore;

namespace FastFoodChain.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Thiết lập Unique Index cho Email để không có 2 tài khoản trùng email
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Seed Data (Dữ liệu mẫu) - Tùy chọn
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Email = "admin@fastfood.com",
                    Name = "Admin Default",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!@#"), // Đặt mật khẩu phức tạp và hash nó
                    RoleToken = 1 // Admin
                }
            );
        }
    }
}