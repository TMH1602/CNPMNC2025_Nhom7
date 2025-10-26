namespace FastFoodCompareAppEnhanced_v3_1.Models
{
    public class FooterViewModel
    {
        // Từ API Menu
        public int ActiveMenuItemCount { get; set; }

        // Từ API Cart
        public int ProcessedOrderCount { get; set; } // isProcessed = true
        public int PendingOrderCount { get; set; }   // isProcessed = false
    }
}