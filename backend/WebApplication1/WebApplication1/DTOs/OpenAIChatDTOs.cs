using System.Text.Json.Serialization;

namespace WebApplication1.ViewModels
{
    public class OpenAIChatRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = "gpt-3.5-turbo";

        [JsonPropertyName("messages")]
        public List<OpenAIMessage> Messages { get; set; } = new List<OpenAIMessage>();
    }

    public class OpenAIMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = "user";

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }
    public class OpenAIChatResponse
    {
        [JsonPropertyName("choices")]
        public List<Choice> Choices { get; set; } = new List<Choice>();
    }

    public class Choice
    {
        [JsonPropertyName("message")]
        public OpenAIMessage Message { get; set; } = new OpenAIMessage();

        [JsonPropertyName("finish_reason")]
        public string FinishReason { get; set; } = string.Empty;
    }
}
