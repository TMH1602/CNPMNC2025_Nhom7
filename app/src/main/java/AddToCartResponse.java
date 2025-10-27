package com.example.cnpmnc_appfood;

import com.google.gson.annotations.SerializedName;
import java.util.List;

// Lưu ý: Chúng ta chỉ cần trường 'message' để hiển thị UI
public class AddToCartResponse {
    @SerializedName("username")
    private String username;

    @SerializedName("message")
    private String message; // Trường bạn muốn hiển thị

    // Bạn có thể bỏ qua trường itemsInCart nếu không dùng

    public String getMessage() {
        return message;
    }
}