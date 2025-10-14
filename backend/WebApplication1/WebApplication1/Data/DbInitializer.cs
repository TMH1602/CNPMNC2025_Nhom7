using WebApplication1.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims; // Chỉ dùng cho ví dụ, không bắt buộc

namespace WebApplication1.Data
{
    /// <summary>
    /// Chứa logic để khởi tạo dữ liệu mẫu (seed data) cho Database.
    /// </summary>
    public static class DbInitializer
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            // Lấy DbContext instance từ Service Provider
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                // 1. SEED DỮ LIỆU SẢN PHẨM (PRODUCTS)
                if (!context.Products.Any())
                {
                    var products = new Product[]
                    {
                        new Product { Name = "Gà Rán Giòn Tan (1M)", Price = 59000m, Description = "Gà rán giòn tan, chuẩn vị. (1 Miếng)", Category = "Gà" },
                        new Product { Name = "Combo Gà Vui Vẻ", Price = 99000m, Description = "2 Gà rán + 1 Khoai tây chiên cỡ vừa + 1 Nước ngọt.", Category = "Combo" },
                        new Product { Name = "Burger Bò Phô Mai", Price = 75000m, Description = "Thịt bò Úc, phô mai tan chảy, sốt đặc biệt.", Category = "Burger" },
                        new Product { Name = "Khoai Tây Chiên Lắc", Price = 35000m, Description = "Khoai tây chiên, lắc phô mai hoặc rong biển.", Category = "Món phụ" },
                        new Product { Name = "Pepsi/Coca-Cola (L)", Price = 25000m, Description = "Nước ngọt cỡ lớn.", Category = "Đồ uống" },
                        new Product { Name = "Kem Vani", Price = 20000m, Description = "Kem mát lạnh vị Vani.", Category = "Tráng miệng" },
                        new Product { Name = "Salad Gà Nướng", Price = 85000m, Description = "Ức gà nướng, rau xanh tươi, sốt Caesar.", Category = "Salad" },
                        new Product { Name = "Mì Ý Sốt Bò Bằm", Price = 110000m, Description = "Mì Ý chuẩn vị, sốt bò bằm đậm đà.", Category = "Mì Ý" },
                        new Product { Name = "Cơm Gà Xối Mỡ", Price = 79000m, Description = "Cơm dẻo, gà xối mỡ giòn rụm.", Category = "Cơm" },
                        new Product { Name = "Combo Gia Đình", Price = 250000m, Description = "4 Gà + 2 Burger + Khoai lớn + 4 Nước.", Category = "Combo" },
                        new Product { Name = "Burger Cá Hồi", Price = 120000m, Description = "Thịt cá hồi áp chảo, rau xà lách tươi.", Category = "Burger" },
                        new Product { Name = "Bánh Mì Kẹp Thịt", Price = 45000m, Description = "Bánh mì nướng giòn, nhân thịt heo/gà tùy chọn.", Category = "Bánh Mì" },
                        new Product { Name = "Nước Cam Ép", Price = 30000m, Description = "Cam tươi 100%, không đường.", Category = "Đồ uống" },
                        new Product { Name = "Gà Popcorn", Price = 45000m, Description = "Thịt gà viên chiên giòn, sốt chấm.", Category = "Gà" },
                        new Product { Name = "Khoai Tây Nghiền", Price = 28000m, Description = "Khoai tây nghiền mịn, sốt gravy.", Category = "Món phụ" },
                        new Product { Name = "Súp Nấm", Price = 40000m, Description = "Súp kem nấm hương thơm ngon.", Category = "Súp" },
                        new Product { Name = "Pizza Hải Sản", Price = 180000m, Description = "Pizza cỡ vừa, tôm, mực, phô mai Mozzarella.", Category = "Pizza" },
                        new Product { Name = "Trà Sữa Matcha", Price = 50000m, Description = "Trà sữa vị Matcha truyền thống.", Category = "Đồ uống" },
                        new Product { Name = "Burger Phô Mai 2 Tầng", Price = 115000m, Description = "2 miếng thịt bò, 2 lớp phô mai dày.", Category = "Burger" },
                        new Product { Name = "Phở Bò Tái", Price = 65000m, Description = "Phở truyền thống, thịt bò tái.", Category = "Khác" }
                    };
                    context.Products.AddRange(products);
                }

                // 2. SEED DỮ LIỆU TÀI KHOẢN (USERS)
                if (!context.Users.Any())
                {
                    var users = new User[]
                    {
                        new User
                        {
                            Username = "admin@example.com",
                            Email = "admin@example.com",
                            PasswordHash = "LamDinh1702",
                            DisplayName = "Quản Trị Viên",
                            CreatedDate = DateTime.UtcNow
                        },
                        new User
                        {
                            Username = "user@example.com",
                            Email = "user@example.com",
                            PasswordHash = "LamDinh1702",
                            DisplayName = "Khách Hàng Thử Nghiệm",
                            CreatedDate = DateTime.UtcNow
                        }
                    };
                    context.Users.AddRange(users);
                }

                // Lưu tất cả thay đổi vào Database
                context.SaveChanges();
            }
        }
    }
}
