using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using WebApplication1.ViewModels;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public ChatController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        // POST: api/Chat/Send
        [HttpPost("Send")]
        // Chúng ta sẽ nhận một DTO thay vì string đơn giản để linh hoạt hơn
        public async Task<ActionResult<string>> SendContext([FromBody] UserInputDto input)
        {
            // 1. Lấy cấu hình
            var apiUrl = _configuration["ChatbotSettings:ApiUrl"];
            var apiKey = _configuration["ChatbotSettings:ApiKey"];

            if (string.IsNullOrEmpty(apiUrl) || string.IsNullOrEmpty(apiKey))
            {
                return StatusCode(500, new { Message = "Lỗi cấu hình OpenAI API." });
            }

            // 2. Chuẩn bị Request Body (sử dụng DTOs mới)
            var requestDto = new OpenAIChatRequest
            {
                Model = "gpt-5-nano", // Bạn có thể dùng "gpt-4" nếu có quyền truy cập
                Messages = new List<OpenAIMessage>
            {
                // Thêm một "system prompt" để định hướng cho Bot
                new OpenAIMessage { Role = "system", Content = "Bạn là trợ lý ảo của một chuỗi đồ ăn nhanh." },
                // Thêm nội dung của người dùng
                new OpenAIMessage { Role = "user", Content = input.Context }
            }
            };

            // 3. Serialize và tạo HttpClient
            // Sử dụng JsonSerializerOptions để tương thích với JsonPropertyName
            var jsonPayload = JsonSerializer.Serialize(requestDto);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var httpClient = _httpClientFactory.CreateClient();

            // 4. Thêm Header xác thực (RẤT QUAN TRỌNG)
            // OpenAI sử dụng "Bearer Token"
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            HttpResponseMessage response;
            try
            {
                response = await httpClient.PostAsync(apiUrl, content);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(503, $"Lỗi kết nối tới OpenAI: {ex.Message}");
            }

            // 5. Xử lý Response
            var jsonResponse = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                // Trả về lỗi từ OpenAI
                return StatusCode((int)response.StatusCode, jsonResponse);
            }

            try
            {
                // 6. Deserialize phản hồi của OpenAI
                var openAIResponse = JsonSerializer.Deserialize<OpenAIChatResponse>(jsonResponse);

                // 7. Lấy nội dung phản hồi
                // Kiểm tra xem có phản hồi và lựa chọn đầu tiên có tồn tại không
                if (openAIResponse?.Choices != null && openAIResponse.Choices.Count > 0)
                {
                    string botResponse = openAIResponse.Choices[0].Message.Content;
                    return Ok(new { ResponseContext = botResponse });
                }
                else
                {
                    return StatusCode(500, "Không nhận được nội dung phản hồi hợp lệ từ OpenAI.");
                }
            }
            catch (JsonException ex)
            {
                return StatusCode(500, $"Lỗi khi đọc phản hồi từ OpenAI: {ex.Message}");
            }
        }
    }
}
