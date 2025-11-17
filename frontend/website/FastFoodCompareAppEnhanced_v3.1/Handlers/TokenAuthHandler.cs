using System.Net.Http.Headers;

namespace FastFoodCompareAppEnhanced_v3_1.Handlers
{
    // Lớp này sẽ tự động đính kèm token vào HttpClient requests
    public class TokenAuthHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TokenAuthHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Lấy HttpContext hiện tại
            var context = _httpContextAccessor.HttpContext;

            if (context != null)
            {
                // Đọc token từ cookie (tên cookie phải khớp với Program.cs)
                var token = context.Request.Cookies["jwtToken"];

                if (!string.IsNullOrEmpty(token))
                {
                    // Gắn token vào header "Authorization"
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }

            // Tiếp tục gửi request đi
            return await base.SendAsync(request, cancellationToken);
        }
    }
}