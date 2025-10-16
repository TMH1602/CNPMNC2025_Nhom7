using Microsoft.EntityFrameworkCore;
using WebApplication1.Models; // Tham chiếu đến các Models

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

        // DbSet cho các Models đã được định nghĩa
        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; } // Bảng trung gian Giỏ hàng

        // Bổ sung cho Lịch sử Đơn hàng (Order History)
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; } // Bảng trung gian Chi tiết Đơn hàng

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =================================================================
            // 1. CẤU HÌNH USER VÀ CART
            // =================================================================

            // Cấu hình Khóa Thay Thế (Alternate Key) cho User.Username
            modelBuilder.Entity<User>()
                .HasAlternateKey(u => u.Username)
                .HasName("AK_User_Username"); // Đảm bảo Username là duy nhất

            // Cấu hình mối quan hệ 1-N giữa User và Cart (sử dụng Username làm Khóa Ngoại)
            modelBuilder.Entity<Cart>()
                .HasOne(c => c.User)              // Cart thuộc về 1 User
                .WithMany(u => u.Carts)           // User có nhiều Carts
                .HasForeignKey(c => c.Username)   // Khóa ngoại trong Cart là Username
                .HasPrincipalKey(u => u.Username) // Khóa tham chiếu trong User là Username
                .OnDelete(DeleteBehavior.Cascade); // Xóa User sẽ xóa Cart liên quan

            // Cấu hình Khóa Chính Kép cho CartItem (Bảng trung gian N-N giữa Cart và Product)
            modelBuilder.Entity<CartItem>()
                .HasKey(ci => new { ci.CartId, ci.ProductId });

            // Cấu hình mối quan hệ CartItem -> Cart
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình mối quan hệ CartItem -> Product
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany(p => p.CartItems)
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Restrict); // Giữ Product khi CartItem bị xóa

            // =================================================================
            // 2. CẤU HÌNH ORDER VÀ ORDERDETAIL (LỊCH SỬ)
            // =================================================================

            // Cấu hình mối quan hệ 1-N giữa User và Order (sử dụng Username làm Khóa Ngoại)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.Username)
                .HasPrincipalKey(u => u.Username)
                .OnDelete(DeleteBehavior.Restrict); // Giữ User khi Order bị xóa

            // Cấu hình Khóa Chính Kép cho OrderDetail (Bảng trung gian N-N giữa Order và Product)
            modelBuilder.Entity<OrderDetail>()
                .HasKey(od => new { od.OrderId, od.ProductId });

            // Cấu hình mối quan hệ OrderDetail -> Order
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa Order sẽ xóa OrderDetail

            // Cấu hình mối quan hệ OrderDetail -> Product
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Product)
                .WithMany(p => p.OrderDetails)
                .HasForeignKey(od => od.ProductId)
                .OnDelete(DeleteBehavior.Restrict); // Giữ Product khi OrderDetail bị xóa
        }
    }
}