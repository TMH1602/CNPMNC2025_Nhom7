using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FastFoodCompareAppEnhanced_v3_1.TagHelpers
{
    // Tag Helper này sẽ "bắt" bất kỳ thẻ HTML nào có thuộc tính highlight
    [HtmlTargetElement("*", Attributes = "highlight")]
    public class HighlightTagHelper : TagHelper
    {
        // ASP.NET Core tự động map "highlight" attribute và convert "true"/"false" string thành boolean
        public bool Highlight { get; set; } = true;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            // Xóa attribute highlight khỏi output (chỉ dùng để trigger tag helper)
            output.Attributes.RemoveAll("highlight");
            
            // Chỉ xử lý nếu highlight = true
            if (!Highlight)
            {
                return;
            }
            
            // Chèn thẻ <b><i> vào TRƯỚC nội dung gốc
            output.PreContent.SetHtmlContent("<b><i>");
            
            // Chèn thẻ </i></b> vào SAU nội dung gốc
            output.PostContent.SetHtmlContent("</i></b>");
        }
    }
}