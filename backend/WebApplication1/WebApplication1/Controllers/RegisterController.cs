using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Controllers
{
    /// <summary>
    /// Controller dùng để xử lý các yêu cầu đăng ký người dùng mới.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")] // Route: /api/Register
    public class RegisterController : ControllerBase
    {
        #region Models

        /// <summary>
        /// Model dữ liệu được gửi lên trong yêu cầu đăng ký.
        /// </summary>
        public class RegisterRequest
        {
            /// <summary>
            /// Tên người dùng hoặc Email (phải là duy nhất).
            /// </summary>
            [Required(ErrorMessage = "Tên người dùng là bắt buộc.")]
            [EmailAddress(ErrorMessage = "Phải là định dạng Email hợp lệ.")]
            public string Email { get; set; } = string.Empty;

            /// <summary>
            /// Mật khẩu (yêu cầu độ dài tối thiểu).
            /// </summary>
            [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
            [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
            public string Password { get; set; } = string.Empty;

            /// <summary>
            /// Xác nhận lại mật khẩu.
            /// </summary>
            [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc.")]
            [Compare(nameof(Password), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        /// <summary>
        /// Model dữ liệu trả về sau khi xử lý đăng ký.
        /// </summary>
        public class RegisterResponse
        {
            /// <summary>
            /// Trạng thái thành công của yêu cầu.
            /// </summary>
            public bool IsSuccess { get; set; }

            /// <summary>
            /// Thông báo chi tiết về kết quả.
            /// </summary>
            public string Message { get; set; } = string.Empty;

            /// <summary>
            /// ID người dùng mới được tạo.
            /// </summary>
            public int UserId { get; set; }
        }

        #endregion


        /// <param name="request">Thông tin đăng ký (Email, Password, ConfirmPassword).</param>
        /// <returns>HTTP 201 Created nếu thành công, hoặc HTTP 400 Bad Request nếu thất bại.</returns>
        [HttpPost] // Route: /api/Register
        [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // HTTP 400 Bad Request
            }

            // --- Vùng xử lý logic đăng ký THỰC TẾ ---
            // *Trong thực tế: Kiểm tra Email đã tồn tại, Hash mật khẩu, Lưu vào Database*

            // Giả lập lưu thành công
            var newUserId = new Random().Next(100, 1000);

            var successResponse = new RegisterResponse
            {
                IsSuccess = true,
                Message = $"Đăng ký thành công cho Email: {request.Email}.",
                UserId = newUserId
            };

            // Trả về HTTP 201 Created cho biết một tài nguyên mới đã được tạo
            return StatusCode(StatusCodes.Status201Created, successResponse);
        }
    }
}