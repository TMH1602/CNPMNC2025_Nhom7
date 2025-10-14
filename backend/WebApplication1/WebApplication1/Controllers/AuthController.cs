using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using WebApplication1.Data; // Thêm DbContext
using WebApplication1.Models; // Thêm User Model
using Microsoft.EntityFrameworkCore; // Thêm cho các hàm Async

namespace MyWebApiWithSwagger.Controllers
{

    [ApiController]
    [Route("api/[controller]")] // Route: /api/Auth
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        // Dependency Injection cho DbContext
        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        // === Khu vực Models (Giữ nguyên) ===
        #region Models (Đăng nhập)
        public class LoginRequest
        {
            [Required(ErrorMessage = "Tên người dùng là bắt buộc.")]
            public string Username { get; set; } = string.Empty;

            [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
            public string Password { get; set; } = string.Empty;
        }

        public class LoginResponse
        {
            public bool IsSuccess { get; set; }
            public string Message { get; set; } = string.Empty;
            public string Token { get; set; } = string.Empty;
            public DateTime ExpiresIn { get; set; }
        }

        #endregion

        #region Models (Đổi Mật khẩu, Xóa Tài khoản, Xem Tài khoản)
        public class ChangePasswordRequest
        {
            [Required(ErrorMessage = "Email/Username là bắt buộc.")]
            public string Identifier { get; set; } = string.Empty;

            [Required(ErrorMessage = "Mật khẩu cũ là bắt buộc.")]
            public string OldPassword { get; set; } = string.Empty;

            [Required(ErrorMessage = "Mật khẩu mới là bắt buộc.")]
            [MinLength(6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự.")]
            public string NewPassword { get; set; } = string.Empty;
        }

        public class ChangePasswordResponse
        {
            public bool IsSuccess { get; set; }
            public string Message { get; set; } = string.Empty;
        }

        public class DeleteAccountRequest
        {
            [Required(ErrorMessage = "Email/Username là bắt buộc.")]
            public string Identifier { get; set; } = string.Empty;

            [Required(ErrorMessage = "Mật khẩu xác nhận là bắt buộc.")]
            public string CurrentPassword { get; set; } = string.Empty;
        }

        public class DeleteAccountResponse
        {
            public bool IsSuccess { get; set; }
            public string Message { get; set; } = string.Empty;
        }

        public class UserAccountResponse
        {
            public int UserId { get; set; }
            public string Email { get; set; } = string.Empty;
            public string DisplayName { get; set; } = string.Empty;
            public DateTime CreatedDate { get; set; }
        }
        #endregion
        // === Kết thúc khu vực Models ===


        // ------------------------------------------------------------------
        // ENDPOINT: ĐĂNG NHẬP (LOGIN)
        // ------------------------------------------------------------------
        [HttpPost("login")] // Route: /api/Auth/login
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 1. Tìm người dùng theo Username
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == request.Username.ToLower());

            if (user == null)
            {
                // Tránh tiết lộ liệu Username có tồn tại hay không
                return Unauthorized(new LoginResponse
                {
                    IsSuccess = false,
                    Message = "Tên người dùng hoặc mật khẩu không đúng. ❌"
                });
            }

            // 2. So sánh mật khẩu (Giả lập: so sánh chuỗi, thực tế phải so sánh Password Hash)
            if (user.PasswordHash == request.Password)
            {
                var expiryTime = DateTime.UtcNow.AddHours(1);
                var successResponse = new LoginResponse
                {
                    IsSuccess = true,
                    Message = "Đăng nhập thành công! ✅",
                    Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.XXX_GENERATED_TOKEN_XXX", // Token giả lập
                    ExpiresIn = expiryTime
                };
                return Ok(successResponse); // HTTP 200 OK
            }
            else
            {
                return Unauthorized(new LoginResponse
                {
                    IsSuccess = false,
                    Message = "Tên người dùng hoặc mật khẩu không đúng. ❌"
                }); // HTTP 401 Unauthorized
            }
        }

        // ------------------------------------------------------------------
        // ENDPOINT: ĐĂNG KÝ (REGISTER) - THÊM MỚI ĐỂ DỄ DÙNG
        // ------------------------------------------------------------------
        [HttpPost("register")] // Route: /api/Auth/register
        [ProducesResponseType(typeof(UserAccountResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 1. Kiểm tra username đã tồn tại chưa
            var existingUser = await _context.Users
                .AnyAsync(u => u.Username.ToLower() == request.Username.ToLower());

            if (existingUser)
            {
                return BadRequest(new LoginResponse
                {
                    IsSuccess = false,
                    Message = "Tên người dùng đã tồn tại. Vui lòng chọn tên khác."
                });
            }

            // 2. Tạo User mới
            var newUser = new User
            {
                Username = request.Username,
                Email = $"{request.Username}@example.com", // Giả lập Email
                PasswordHash = request.Password, // Giả lập lưu mật khẩu (thực tế phải hash)
                DisplayName = request.Username,
                CreatedDate = DateTime.UtcNow
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            var response = new UserAccountResponse
            {
                UserId = newUser.Id,
                Email = newUser.Email,
                DisplayName = newUser.DisplayName,
                CreatedDate = newUser.CreatedDate
            };

            return CreatedAtAction(nameof(GetUserAccount), new { email = newUser.Email }, response);
        }

        // ------------------------------------------------------------------
        // ENDPOINT: ĐỔI MẬT KHẨU
        // ------------------------------------------------------------------
        [HttpPost("change-password")] // Route: /api/Auth/change-password
        [ProducesResponseType(typeof(ChangePasswordResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 1. Tìm người dùng
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == request.Identifier.ToLower() ||
                                          u.Email.ToLower() == request.Identifier.ToLower());

            if (user == null)
            {
                return BadRequest(new ChangePasswordResponse
                {
                    IsSuccess = false,
                    Message = "Không tìm thấy tài khoản. ❌"
                });
            }

            // 2. Xác thực mật khẩu cũ
            if (user.PasswordHash != request.OldPassword)
            {
                return BadRequest(new ChangePasswordResponse
                {
                    IsSuccess = false,
                    Message = "Mật khẩu cũ không chính xác. ❌"
                });
            }

            // 3. Cập nhật mật khẩu mới (thực tế: hash trước khi lưu)
            user.PasswordHash = request.NewPassword;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new ChangePasswordResponse
            {
                IsSuccess = true,
                Message = $"Mật khẩu cho tài khoản {request.Identifier} đã được đổi thành công. ✅"
            });
        }

        // ------------------------------------------------------------------
        // ENDPOINT: XEM TÀI KHOẢN
        // ------------------------------------------------------------------
        [HttpGet("account/{identifier}")] // Route: /api/Auth/account/{identifier}
        [ProducesResponseType(typeof(UserAccountResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserAccount(string identifier)
        {
            // 1. Tìm kiếm tài khoản theo Username hoặc Email
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == identifier.ToLower() ||
                                          u.Email.ToLower() == identifier.ToLower());

            if (user == null)
            {
                return NotFound($"Không tìm thấy tài khoản với Email/Username: {identifier}. ❌");
            }

            // 2. Trả về thông tin hiển thị
            var accountInfo = new UserAccountResponse
            {
                UserId = user.Id,
                Email = user.Email,
                DisplayName = user.DisplayName,
                CreatedDate = user.CreatedDate
            };
            return Ok(accountInfo); // HTTP 200 OK
        }

        // ------------------------------------------------------------------
        // ENDPOINT: XÓA TÀI KHOẢN
        // ------------------------------------------------------------------
        [HttpDelete("account")] // Route: /api/Auth/account
        [ProducesResponseType(typeof(DeleteAccountResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(DeleteAccountResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 1. Tìm người dùng
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == request.Identifier.ToLower() ||
                                          u.Email.ToLower() == request.Identifier.ToLower());

            if (user == null)
            {
                return NotFound(new DeleteAccountResponse
                {
                    IsSuccess = false,
                    Message = $"Không tìm thấy tài khoản với Email/Username: {request.Identifier}. ❌"
                });
            }

            // 2. Xác minh mật khẩu
            if (user.PasswordHash != request.CurrentPassword)
            {
                return BadRequest(new DeleteAccountResponse
                {
                    IsSuccess = false,
                    Message = "Mật khẩu xác nhận không chính xác. Hành động xóa bị từ chối. 🔒"
                });
            }

            // 3. Thực hiện hành động xóa
            _context.Users.Remove(user);
            await _context.SaveChangesAsync(); // Lưu thay đổi vào Database

            return Ok(new DeleteAccountResponse
            {
                IsSuccess = true,
                Message = $"Tài khoản {request.Identifier} đã được xóa thành công khỏi hệ thống. 👋"
            });
        }
    }
}
