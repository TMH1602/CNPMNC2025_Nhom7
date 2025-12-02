using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace WebApplication1.Services
{
    public interface ICloudinaryService
    {
        // Định nghĩa phương thức tải lên
        Task<string?> UploadImageAsync(IFormFile file);
    }
}