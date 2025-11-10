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
using Microsoft.Extensions.Caching.Memory;
namespace MyWebApiWithSwagger.Controllers
{

    [ApiController]
    [Route("api/[controller]")] // Route: /api/Auth
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _cache;
        public AuthController(ApplicationDbContext context,
                            IConfiguration config,
                            IMemoryCache cache) 
        {
            _context = context;
            _config = config;
            _cache = cache; 
        }


        #region Models (Đăng nhập)
        public class ChangeMyPasswordRequest
        {
            [Required(ErrorMessage = "Mật khẩu cũ là bắt buộc.")]
            public string OldPassword { get; set; } = string.Empty;

            [Required(ErrorMessage = "Mật khẩu mới là bắt buộc.")]
            [MinLength(6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự.")]
            public string NewPassword { get; set; } = string.Empty;
        }
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
        private class LoginAttempt
        {
            public int FailedCount { get; set; } = 0;
            public DateTime? LockoutExpiry { get; set; } = null;
        }
        #endregion
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var cacheKey = $"login_fail_{request.Username.ToLower()}";
            var attempt = await _cache.GetOrCreateAsync(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                return Task.FromResult(new LoginAttempt());
            });
            if (attempt.LockoutExpiry.HasValue && attempt.LockoutExpiry > DateTime.UtcNow)
            {
                var timeLeft = Math.Round((attempt.LockoutExpiry.Value - DateTime.UtcNow).TotalMinutes);
                return Unauthorized(new LoginResponse
                {
                    IsSuccess = false,
                    Message = $"Tài khoản đang bị khóa. Vui lòng thử lại sau {timeLeft} phút."
                });
            }
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == request.Username.ToLower());
            bool isPasswordValid = false;
            if (user != null)
            {
                isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            }
            if (user != null && isPasswordValid)
            {
                _cache.Remove(cacheKey);

                var expiryTime = DateTime.UtcNow.AddMinutes(30);
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
                attempt.FailedCount++;

                string message;

                if (attempt.FailedCount >= 3)
                {

                    attempt.LockoutExpiry = DateTime.UtcNow.AddMinutes(30);
                    attempt.FailedCount = 0; 
                    message = "Đăng nhập sai 3 lần. Tài khoản của bạn đã bị khóa trong 30 phút";
                }
                else
                {
                    message = $"Tên người dùng hoặc mật khẩu không đúng. (Lần {attempt.FailedCount}/3).";
                }
                _cache.Set(cacheKey, attempt, TimeSpan.FromMinutes(30));

                return Unauthorized(new LoginResponse { IsSuccess = false, Message = message });
            }
        }
        [HttpPost("register")]
        [ProducesResponseType(typeof(UserAccountResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRQ request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (await _context.Users.AnyAsync(u => u.Username.ToLower() == request.Username.ToLower()))
            {
                return BadRequest(new { IsSuccess = false, Message = "Tên người dùng đã tồn tại." });
            }
            if (await _context.Users.AnyAsync(u => u.Email.ToLower() == request.Email.ToLower()))
            {
                return BadRequest(new { IsSuccess = false, Message = "Email đã tồn tại." });
            }

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
            return StatusCode(201, response);
        }

        [HttpPost("registerAdmin")]
        [ProducesResponseType(typeof(UserAccountResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Registerad([FromBody] RegisterRQ request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (await _context.Users.AnyAsync(u => u.Username.ToLower() == request.Username.ToLower()))
            {
                return BadRequest(new { IsSuccess = false, Message = "Tên người dùng đã tồn tại." });
            }
            if (await _context.Users.AnyAsync(u => u.Email.ToLower() == request.Email.ToLower()))
            {
                return BadRequest(new { IsSuccess = false, Message = "Email đã tồn tại." });
            }

            var newUser = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password), // Băm mật khẩu
                Address = request.Address,
                DisplayName = "Admin",
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
        [HttpPost("registerRes")]
        [ProducesResponseType(typeof(UserAccountResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterRes([FromBody] RegisterRQ request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (await _context.Users.AnyAsync(u => u.Username.ToLower() == request.Username.ToLower()))
            {
                return BadRequest(new { IsSuccess = false, Message = "Tên người dùng đã tồn tại." });
            }
            if (await _context.Users.AnyAsync(u => u.Email.ToLower() == request.Email.ToLower()))
            {
                return BadRequest(new { IsSuccess = false, Message = "Email đã tồn tại." });
            }
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

        [HttpPost("change-password")]
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

            bool isOldPasswordValid = BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash);
            if (!isOldPasswordValid)
            {
                return BadRequest(new ChangePasswordResponse { IsSuccess = false, Message = "Mật khẩu cũ không chính xác. ❌" });
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new ChangePasswordResponse { IsSuccess = true, Message = "Mật khẩu đã được đổi thành công. ✅" });
        }

        [HttpGet("account/{identifier}")] 
        [Authorize]
        [ProducesResponseType(typeof(UserAccountResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserAccount(string identifier)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == identifier.ToLower() ||
                                          u.Email.ToLower() == identifier.ToLower());

            if (user == null)
            {
                return NotFound($"Không tìm thấy tài khoản với Email/Username: {identifier}. ❌");
            }

            var accountInfo = new UserAccountResponse
            {
                UserId = user.Id,
                Email = user.Email,
                DisplayName = user.DisplayName,
                CreatedDate = user.CreatedDate
            };
            return Ok(accountInfo); 
        }

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

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash);
            if (!isPasswordValid)
            {
                return BadRequest(new DeleteAccountResponse { IsSuccess = false, Message = "Mật khẩu xác nhận không chính xác. 🔒" });
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new DeleteAccountResponse { IsSuccess = true, Message = "Tài khoản của bạn đã được xóa thành công. 👋" });
        }

        private string GenerateJwtToken(User user, DateTime expiryTime)
        {
            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]) 
            );
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("userId", user.Id.ToString()), 
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
