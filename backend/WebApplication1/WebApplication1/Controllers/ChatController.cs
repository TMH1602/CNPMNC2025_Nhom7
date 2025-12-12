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

        [HttpPost("Send")]
        public async Task<ActionResult<string>> SendContext([FromBody] UserInputDto input)
        {
            var apiUrl = _configuration["ChatbotSettings:ApiUrl"];
            var apiKey = _configuration["ChatbotSettings:ApiKey"];

            if (string.IsNullOrEmpty(apiUrl) || string.IsNullOrEmpty(apiKey))
            {
                return StatusCode(500, new { Message = "Lỗi cấu hình OpenAI API." });
            }

            var requestDto = new OpenAIChatRequest
            {
                Model = "gpt-5-nano", 
                Messages = new List<OpenAIMessage>
            {
                new OpenAIMessage { Role = "system", Content = "Bạn là trợ lý ảo của một chuỗi đồ ăn nhanh." },
                new OpenAIMessage { Role = "user", Content = input.Context }
            }
            };
            var jsonPayload = JsonSerializer.Serialize(requestDto);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var httpClient = _httpClientFactory.CreateClient();

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

            var jsonResponse = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, jsonResponse);
            }

            try
            {
                var openAIResponse = JsonSerializer.Deserialize<OpenAIChatResponse>(jsonResponse);
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
