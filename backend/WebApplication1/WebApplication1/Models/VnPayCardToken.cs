using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    /// <summary>
    /// Model lưu trữ Token thẻ đã được VNPay cấp phép (Tokenization).
    /// </summary>
    public class VnPayCardToken
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int UserId { get; set; } 
        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;
        [Required]
        [MaxLength(100)]
        public string Token { get; set; } = string.Empty;
        [MaxLength(20)]
        public string CardNumber { get; set; } = string.Empty;
        [MaxLength(10)]
        public string BankCode { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        [ForeignKey("Username")]
        public virtual User? User { get; set; }
    }
}