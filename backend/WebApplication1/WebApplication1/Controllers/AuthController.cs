using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using WebApplication1.Data; 
using WebApplication1.Models; 
using Microsoft.EntityFrameworkCore; 
using System.Text;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using BCrypt.Net; 
using Microsoft.AspNetCore.Authorization; 
using Microsoft.Extensions.Configuration;
namespace MyWebApiWithSwagger.Controllers
{

    [ApiController]
    [Route("api/[controller]")] // Route: /api/Auth
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        // Dependency Injection cho DbContext
        public AuthController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }


        // === Khu vực Models (Giữ nguyên) ===
        #region Models (Đăng nhập)
        public class ChangeMyPasswordRequest
        {
            [Required(ErrorMessage = "Mật khẩu cũ là bắt buộc.")]
            public string OldPassword { get; set; } = string.Empty;

            [Required(ErrorMessage = "Mật khẩu mới là bắt buộc.")]
            [MinLength(6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự.")]
            public string NewPassword { get; set; } = string.Empty;
        }

        // Model bảo mật (dùng cho hàm [Authorize])
        public class DeleteMyAccountRequest
        {
            [Required(ErrorMessage = "Mật khẩu xác nhận là bắt buộc.")]
            public string CurrentPassword { get; set; } = string.Empty;
        }

        public class LoginRequest
        {
            [Required(ErrorMessage = "Tên người dùng là bắt buộc.")]
            public string Username { get; set; } = string.Empty;

            [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
            public string Password { get; set; } = string.Empty;
        }

        public class RegisterRQ
        {
            [Required(ErrorMessage = "Tên người dùng là bắt buộc.")]
            public string Username { get; set; } = string.Empty;

            [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
            public string Password { get; set; } = string.Empty;
            [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
            public string Address { get; set; } = string.Empty;

            [Required(ErrorMessage = "Email là bắt buộc.")]
            [EmailAddress(ErrorMessage = "Định dạng Email không hợp lệ (cần có ký tự @).")]
            public string Email { get; set; } = string.Empty;

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
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == request.Username.ToLower());

            if (user == null)
            {
                return Unauthorized(new LoginResponse { IsSuccess = false, Message = "Tên người dùng hoặc mật khẩu không đúng. ❌" });
            }
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

            if (isPasswordValid)
            {
                var expiryTime = DateTime.UtcNow.AddHours(2);
                var tokenString = GenerateJwtToken(user, expiryTime);

                var successResponse = new LoginResponse
                {
                    IsSuccess = true,
                    Message = "Đăng nhập thành công! ✅",
                    Token = tokenString,
                    ExpiresIn = expiryTime
                };
                return Ok(successResponse);
            }
            else
            {
                return Unauthorized(new LoginResponse { IsSuccess = false, Message = "Tên người dùng hoặc mật khẩu không đúng. ❌" });
            }
        }


        // ------------------------------------------------------------------
        // ENDPOINT: ĐĂNG KÝ (REGISTER) - THÊM MỚI ĐỂ DỄ DÙNG
        // ------------------------------------------------------------------
        [HttpPost("register")]
        [ProducesResponseType(typeof(UserAccountResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRQ request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // 1. Kiểm tra tồn tại
            if (await _context.Users.AnyAsync(u => u.Username.ToLower() == request.Username.ToLower()))
            {
                return BadRequest(new { IsSuccess = false, Message = "Tên người dùng đã tồn tại." });
            }
            if (await _context.Users.AnyAsync(u => u.Email.ToLower() == request.Email.ToLower()))
            {
                return BadRequest(new { IsSuccess = false, Message = "Email đã tồn tại." });
            }

            // 2. SỬA LỖI BẢO MẬT: Dùng BCrypt.HashPassword
            var newUser = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password), // Băm mật khẩu
                Address = request.Address,
                DisplayName = "Khách Hàng",
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

            // Trả về 201 Created với thông tin tài khoản mới (không trả về hàm GetMyAccount vì nó cần token)
            return StatusCode(201, response);
        }

        // ------------------------------------------------------------------
        // ENDPOINT: ĐĂNG KÝ NHÀ HÀNG (REGISTERRES)
        // ------------------------------------------------------------------
        [HttpPost("registerRes")]
        [ProducesResponseType(typeof(UserAccountResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterRes([FromBody] RegisterRQ request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // 1. Kiểm tra tồn tại
            if (await _context.Users.AnyAsync(u => u.Username.ToLower() == request.Username.ToLower()))
            {
                return BadRequest(new { IsSuccess = false, Message = "Tên người dùng đã tồn tại." });
            }
            if (await _context.Users.AnyAsync(u => u.Email.ToLower() == request.Email.ToLower()))
            {
                return BadRequest(new { IsSuccess = false, Message = "Email đã tồn tại." });
            }

            // 2. SỬA LỖI BẢO MẬT: Dùng BCrypt.HashPassword
            var newUser = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password), // Băm mật khẩu
                Address = request.Address,
                DisplayName = "Nhà hàng",
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

            return StatusCode(201, response);
        }


        // ------------------------------------------------------------------
        // ENDPOINT: ĐỔI MẬT KHẨU
        // ------------------------------------------------------------------
        [HttpPost("change-password")] // Route: /api/Auth/change-password
        [Authorize]
        [ProducesResponseType(typeof(ChangePasswordResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangeMyPassword([FromBody] ChangeMyPasswordRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userIdString = User.FindFirstValue("userId");
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

            var user = await _context.Users.FindAsync(int.Parse(userIdString));
            if (user == null) return NotFound(new ChangePasswordResponse { IsSuccess = false, Message = "Không tìm thấy tài khoản. ❌" });

            // Xác thực mật khẩu cũ
            bool isOldPasswordValid = BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash);
            if (!isOldPasswordValid)
            {
                return BadRequest(new ChangePasswordResponse { IsSuccess = false, Message = "Mật khẩu cũ không chính xác. ❌" });
            }

            // Băm và cập nhật mật khẩu mới
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new ChangePasswordResponse { IsSuccess = true, Message = "Mật khẩu đã được đổi thành công. ✅" });
        }


        // ------------------------------------------------------------------
        // ENDPOINT: XEM TÀI KHOẢN
        // ------------------------------------------------------------------
        [HttpGet("account/{identifier}")] // Route: /api/Auth/account/{identifier}
        [Authorize]
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
        [HttpDelete("delete-my-account")]
        [Authorize]
        [ProducesResponseType(typeof(DeleteAccountResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteMyAccount([FromBody] DeleteMyAccountRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userIdString = User.FindFirstValue("userId");
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

            var user = await _context.Users.FindAsync(int.Parse(userIdString));
            if (user == null) return NotFound(new DeleteAccountResponse { IsSuccess = false, Message = "Không tìm thấy tài khoản. ❌" });

            // Xác minh mật khẩu
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash);
            if (!isPasswordValid)
            {
                return BadRequest(new DeleteAccountResponse { IsSuccess = false, Message = "Mật khẩu xác nhận không chính xác. 🔒" });
            }

            // Xóa tài khoản
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new DeleteAccountResponse { IsSuccess = true, Message = "Tài khoản của bạn đã được xóa thành công. 👋" });
        }

        private string GenerateJwtToken(User user, DateTime expiryTime)
        {
            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]) // Đọc từ config
            );
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("userId", user.Id.ToString()), // Claim tùy chỉnh quan trọng
                new Claim("displayName", user.DisplayName),
                new Claim(ClaimTypes.Role, user.DisplayName)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: expiryTime,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
