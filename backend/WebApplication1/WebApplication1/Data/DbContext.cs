using Microsoft.EntityFrameworkCore;
using WebApplication1.Models; // Tham chiếu đến Model Product

namespace WebApplication1.Data
{
    /// <summary>
    /// Context cho Entity Framework Core, đại diện cho session kết nối với Database.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet này ánh xạ lớp Product thành bảng 'Products' trong Database
        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
