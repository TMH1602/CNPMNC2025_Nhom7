using Microsoft.EntityFrameworkCore;
using WebApplication1.Models; // Tham chiếu đến các Models
using System.Linq;

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
        public DbSet<VnPayCardToken> VnPayCardTokens { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        // Bổ sung cho Lịch sử Đơn hàng (Order History)
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        // Cần chỉnh sửa tên DbSet (theo quy ước)
        public DbSet<ProductDeletedByAdmin> ProductDeletedByAdmins { get; set; }
        // LƯU Ý: Đổi tên DbSet thành số nhiều (ProductDeletedByAdmins) để khớp với quy ước
        // và tránh lỗi 'Invalid object name' nếu tên bảng trong DB là số nhiều.

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =================================================================
            // Cấu hình Global
            // =================================================================
            // Đảm bảo tên bảng cho ProductDeletedByAdmin là số ít nếu bạn muốn
            // modelBuilder.Entity<ProductDeletedByAdmin>().ToTable("ProductDeletedByAdmin"); 


            // =================================================================
            // 1. CẤU HÌNH USER VÀ CART
            // =================================================================

            // Cấu hình Khóa Thay Thế (Alternate Key) cho User.Username
            modelBuilder.Entity<User>()
                .HasAlternateKey(u => u.Username)
                .HasName("AK_User_Username"); // Đảm bảo Username là duy nhất

            // Cấu hình mối quan hệ 1-N giữa User và Cart (sử dụng Username làm Khóa Ngoại)
            modelBuilder.Entity<Cart>()
                .HasOne(c => c.User)              // Cart thuộc về 1 User
                                                  // Giả định bạn đã thêm 'public ICollection<Cart> Carts { get; set; }' vào User.cs
                .WithMany(u => u.Carts)           // User có nhiều Carts
                .HasForeignKey(c => c.Username)   // Khóa ngoại trong Cart là Username
                .HasPrincipalKey(u => u.Username) // Khóa tham chiếu trong User là Username
                .OnDelete(DeleteBehavior.Cascade);

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
                // Giả định bạn đã thêm 'public ICollection<Order> Orders { get; set; }' vào User.cs
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.Username)
                .HasPrincipalKey(u => u.Username)
                .OnDelete(DeleteBehavior.Restrict);

            // Cấu hình Khóa Chính Kép cho OrderDetail
            modelBuilder.Entity<OrderDetail>()
                .HasKey(od => new { od.OrderId, od.ProductId });

            // Cấu hình mối quan hệ OrderDetail -> Order
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình mối quan hệ OrderDetail -> Product
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Product)
                .WithMany(p => p.OrderDetails)
                .HasForeignKey(od => od.ProductId)
                .OnDelete(DeleteBehavior.Restrict); // Giữ Product khi OrderDetail bị xóa

            // =================================================================
            // 3. CẤU HÌNH VNPAYCARDTOKEN (TOKEN THẺ)
            // =================================================================

            // Cấu hình mối quan hệ 1-N giữa User và VnPayCardToken (sử dụng Username làm Khóa Ngoại)
            modelBuilder.Entity<VnPayCardToken>()
                .HasOne(t => t.User)
                // Giả định bạn đã thêm 'public ICollection<VnPayCardToken> Tokens { get; set; }' vào User.cs
                .WithMany(u => u.Tokens) // Đổi tên thuộc tính điều hướng trong User.cs thành 'Tokens'
                .HasForeignKey(t => t.Username)
                .HasPrincipalKey(u => u.Username)
                .OnDelete(DeleteBehavior.Cascade); // Xóa User sẽ xóa Token
        }
    }
}