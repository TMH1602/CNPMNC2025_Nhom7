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
                        new Product {
                            Name = "Gà Rán Giòn Tan (1M)",
                            Price = 59000m,
                            Description = "Gà rán giòn tan, chuẩn vị. (1 Miếng)",
                            Category = "Gà",
                            ImageUrl = "https://res.cloudinary.com/du42rq1ki/image/upload/v1760536853/friedchicken_kv1wd3.jpg" ,// <== THÊM IMAGE URL
                            IsActive = true
                        },
                        new Product {
                            Name = "Combo Gà Vui Vẻ",
                            Price = 99000m,
                            Description = "2 Gà rán + 1 Khoai tây chiên cỡ vừa + 1 Nước ngọt.",
                            Category = "Combo",
                            ImageUrl = "https://res.cloudinary.com/du42rq1ki/image/upload/v1760536853/friedchicken_kv1wd3.jpg",
                            IsActive = true

                        },
                        new Product {
                            Name = "Burger Bò Phô Mai",
                            Price = 75000m,
                            Description = "Thịt bò Úc, phô mai tan chảy, sốt đặc biệt.",
                            Category = "Burger",
                            ImageUrl = "https://res.cloudinary.com/du42rq1ki/image/upload/v1760536853/burger_uwn0wn.jpg",
                            IsActive = true
                        },
                        new Product {
                            Name = "Khoai Tây Chiên Lắc",
                            Price = 35000m,
                            Description = "Khoai tây chiên, lắc phô mai hoặc rong biển.",
                            Category = "Món phụ",
                            ImageUrl = "https://res.cloudinary.com/du42rq1ki/image/upload/v1760536853/fries_ohsugr.jpg",
                            IsActive = true
                        },
                        new Product {
                            Name = "Pepsi/Coca-Cola (L)",
                            Price = 25000m,
                            Description = "Nước ngọt cỡ lớn.",
                            Category = "Đồ uống",
                            ImageUrl = "https://res.cloudinary.com/du42rq1ki/image/upload/v1760623746/Coca-vs-Pepsi_tzxu5o.jpg",
                            IsActive = true
                        },
                        new Product {
                            Name = "Kem Vani",
                            Price = 20000m,
                            Description = "Kem mát lạnh vị Vani.",
                            Category = "Tráng miệng",
                            ImageUrl = "https://res.cloudinary.com/du42rq1ki/image/upload/v1760623746/KemVani_mntywx.jpg",
                            IsActive = true
                        },
                        new Product {
                            Name = "Salad Gà Nướng",
                            Price = 85000m,
                            Description = "Ức gà nướng, rau xanh tươi, sốt Caesar.",
                            Category = "Salad",
                            ImageUrl = "https://res.cloudinary.com/du42rq1ki/image/upload/v1760536853/salad_yj8omu.jpg",
                            IsActive = true
                        },
                        new Product {
                            Name = "Mì Ý Sốt Bò Bằm",
                            Price = 110000m,
                            Description = "Mì Ý chuẩn vị, sốt bò bằm đậm đà.",
                            Category = "Mì Ý",
                            ImageUrl = "https://res.cloudinary.com/du42rq1ki/image/upload/v1760623746/MyYSotBoBam_jqgprg.jpg",
                            IsActive = true
                        },
                        new Product {
                            Name = "Cơm Gà Xối Mỡ",
                            Price = 79000m,
                            Description = "Cơm dẻo, gà xối mỡ giòn rụm.",
                            Category = "Cơm",
                            ImageUrl = "https://res.cloudinary.com/du42rq1ki/image/upload/v1760623749/Com-Ga-Xoi-Mo-77-1_unglox.jpg",
                            IsActive = true
                        },
                        new Product {
                            Name = "Combo Gia Đình",
                            Price = 250000m,
                            Description = "4 Gà + 2 Burger + Khoai lớn + 4 Nước.",
                            Category = "Combo",
                            ImageUrl = "https://res.cloudinary.com/du42rq1ki/image/upload/v1760623746/combo-gia-dinh-2_kbhmzp.jpg",
                            IsActive = true
                        },
                        new Product {
                            Name = "Burger Cá Hồi",
                            Price = 120000m,
                            Description = "Thịt cá hồi áp chảo, rau xà lách tươi.",
                            Category = "Burger",
                            ImageUrl = "https://res.cloudinary.com/du42rq1ki/image/upload/v1760623745/BurgerCaHoi_bxzj1l.jpg",
                            IsActive = true
                        },
                        new Product {
                            Name = "Bánh Mì Kẹp Thịt",
                            Price = 45000m,
                            Description = "Bánh mì nướng giòn, nhân thịt heo/gà tùy chọn.",
                            Category = "Bánh Mì",
                            ImageUrl = "https://res.cloudinary.com/du42rq1ki/image/upload/v1760623748/Banhmykepthit_lgizgs.jpg",
                            IsActive = true
                        },
                        new Product {
                            Name = "Nước Cam Ép",
                            Price = 30000m,
                            Description = "Cam tươi 100%, không đường.",
                            Category = "Đồ uống",
                            ImageUrl = "https://res.cloudinary.com/du42rq1ki/image/upload/v1760623747/Cach-lam-nuoc-cam-ep-ngon-va-thom-ket-hop-voi-le-va-gung-5_dqs5re.jpg",
                            IsActive = true
                        },
                        new Product {
                            Name = "Gà Popcorn",
                            Price = 45000m,
                            Description = "Thịt gà viên chiên giòn, sốt chấm.",
                            Category = "Gà",
                            ImageUrl = "https://res.cloudinary.com/du42rq1ki/image/upload/v1760623745/GaPopcorn_eaflx2.jpg",
                            IsActive = true
                        },
                        new Product {
                            Name = "Khoai Tây Nghiền",
                            Price = 28000m,
                            Description = "Khoai tây nghiền mịn, sốt gravy.",
                            Category = "Món phụ",
                            ImageUrl = "https://res.cloudinary.com/du42rq1ki/image/upload/v1760623744/dd2b2329-khoai-tay-nghien-4_k2j69b.jpg",
                            IsActive = true
                        },
                        new Product {
                            Name = "Súp Nấm",
                            Price = 40000m,
                            Description = "Súp kem nấm hương thơm ngon.",
                            Category = "Súp",
                            ImageUrl = "https://res.cloudinary.com/du42rq1ki/image/upload/v1760623746/thai-soup-with-chicken-mushrooms_2829-10045_r1baxm.avif" // <== THÊM IMAGE URL
                        },
                        new Product {
                            Name = "Pizza Hải Sản",
                            Price = 180000m,
                            Description = "Pizza cỡ vừa, tôm, mực, phô mai Mozzarella.",
                            Category = "Pizza",
                            ImageUrl = "https://res.cloudinary.com/du42rq1ki/image/upload/v1760623744/cach-lam-pizza-hai-san-nong-gion-hap-dan-ngay-tai-nha-201911300931185790_l7m8iw.jpg" // <== THÊM IMAGE URL
                        },
                        new Product {
                            Name = "Trà Sữa Matcha",
                            Price = 50000m,
                            Description = "Trà sữa vị Matcha truyền thống.",
                            Category = "Đồ uống",
                            ImageUrl = "https://res.cloudinary.com/du42rq1ki/image/upload/v1760623744/tra-sua-matcha-tran-chau-den_pr2pgh.jpg" // <== THÊM IMAGE URL
                        },
                        new Product {
                            Name = "Burger Phô Mai 2 Tầng",
                            Price = 115000m,
                            Description = "2 miếng thịt bò, 2 lớp phô mai dày.",
                            Category = "Burger",
                            ImageUrl = "https://res.cloudinary.com/du42rq1ki/image/upload/v1760623744/BurgerPhoMai2Tang_g0gmgf.jpg" // <== THÊM IMAGE URL
                        },
                        new Product {
                            Name = "Phở Bò Tái",
                            Price = 65000m,
                            Description = "Phở truyền thống, thịt bò tái.",
                            Category = "Khác",
                            ImageUrl = "https://res.cloudinary.com/du42rq1ki/image/upload/v1760623744/PhoBoTai_guaaep.jpg" // <== THÊM IMAGE URL
                        }
                    };
                    context.Products.AddRange(products);
                }

                // 2. SEED DỮ LIỆU TÀI KHOẢN (USERS)
                // (Giữ nguyên phần này)
                if (!context.Users.Any())
                {
                    var users = new User[]
                    {
                        new User
                        {
                            Username = "admin@example.com",
                            Email = "admin@example.com",
                            // LƯU Ý: Trong ứng dụng thực tế, hãy HASH mật khẩu trước khi lưu.
                            PasswordHash = "LamDinh1702",
                            Address = "828 Sư Vạn Hạnh Quận 10 thành phố Hồ Chí Minh",
                            DisplayName = "Quản Trị Viên",
                            CreatedDate = DateTime.UtcNow
                        },
                        new User
                        {
                            Username = "user@example.com",
                            Email = "user@example.com",
                            PasswordHash = "LamDinh1702",
                            Address = "828 Sư Vạn Hạnh Quận 10 thành phố Hồ Chí Minh",
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