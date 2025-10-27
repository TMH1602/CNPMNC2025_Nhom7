using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq; // Để sử dụng Linq cho Count() và Where()
using FastFoodCompareAppEnhanced_v3_1.Models; // Sử dụng Model MenuItem vừa tạo

// Bạn cần đảm bảo HttpClient được đăng ký trong Program.cs (ví dụ: builder.Services.AddHttpClient();)
public class FastFoodFooterViewComponent : ViewComponent
{
    private readonly IHttpClientFactory _httpClientFactory;

    // Dependency Injection: Nhận IHttpClientFactory
    public FastFoodFooterViewComponent(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        int activeItemCount = 0;
        
        // 1. Tạo HttpClient
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("Accept", "text/plain");

        // 2. Gọi API
        var apiUrl = "https://localhost:7132/api/Menu";
        
        try
        {
            var response = await client.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            
            // 3. Deserialize JSON
            var menuItems = JsonSerializer.Deserialize<List<MenuItem>>(jsonString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // Bỏ qua sự khác biệt về chữ hoa/chữ thường
            });

            // 4. Xử lý và Đếm dữ liệu
            if (menuItems != null)
            {
                activeItemCount = menuItems.Count(item => item.IsActive);
            }
        }
        catch (HttpRequestException ex)
        {
            // Xử lý lỗi kết nối/API
            Console.WriteLine($"Lỗi khi gọi API Menu: {ex.Message}");
            // Có thể giữ activeItemCount = 0 hoặc gán một giá trị lỗi
        }
        
        // 5. Truyền số lượng đếm được sang View
        return View(activeItemCount);
    }
}