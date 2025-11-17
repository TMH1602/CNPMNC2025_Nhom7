using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FastFoodCompareAppEnhanced_v3_1.TagHelpers
{
    // Tag Helper này sẽ "bắt" thẻ <tr> có thuộc tính "bg-color"
    [HtmlTargetElement("tr", Attributes = "bg-color")]
    public class TrTagHelper : TagHelper
    {
        // ASP.NET Core tự động map "bg-color" thành BgColor property
        public string BgColor { get; set; } = "dark"; 
        
        // ASP.NET Core tự động map "text-color" thành TextColor property
        public string TextColor { get; set; } = "white";

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            // Đảm bảo giá trị không null hoặc rỗng
            if (string.IsNullOrWhiteSpace(BgColor))
            {
                BgColor = "dark";
            }
            if (string.IsNullOrWhiteSpace(TextColor))
            {
                TextColor = "white";
            }
            
            // Xóa attribute bg-color và text-color khỏi output (chỉ dùng để trigger tag helper)
            output.Attributes.RemoveAll("bg-color");
            output.Attributes.RemoveAll("text-color");
            
            // Lấy class hiện tại (nếu có)
            string existingClass = "";
            if (output.Attributes.ContainsName("class"))
            {
                var classValue = output.Attributes["class"].Value;
                existingClass = classValue?.ToString()?.Trim() ?? "";
            }
            
            // Thêm class Bootstrap mới với các hiệu ứng nổi bật
            // Thêm border, shadow, và padding để làm nổi bật hơn
            var newClass = $"bg-{BgColor.Trim()} text-{TextColor.Trim()} border border-{BgColor.Trim()} border-3 shadow-sm fw-bold";
            var finalClass = string.IsNullOrWhiteSpace(existingClass) 
                ? newClass 
                : $"{existingClass.Trim()} {newClass}";
            
            // Set class attribute
            output.Attributes.SetAttribute("class", finalClass.Trim());
        }
    }
}