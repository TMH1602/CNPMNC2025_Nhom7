using System;
using System.Collections.Generic;

namespace WebApplication1.ViewModels
{
    public class OrderHistoryDto
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;

        // Danh sách các mục trong đơn hàng (ViewModel)
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
    }
}