package com.example.cnpmnc_appfood;

import com.google.gson.annotations.SerializedName;
import java.util.List; // <-- 1. THÊM IMPORT NÀY

public class Dish {

    @SerializedName("id")
    private int id;

    @SerializedName("name")
    private String name;

    @SerializedName("price")
    private double price;

    @SerializedName("description")
    private String description;

    @SerializedName("category")
    private String category;

    @SerializedName("imageUrl")
    private String imageUrl;

    // Log của bạn cho thấy 'isActive' đã được trả về, rất tốt!
    @SerializedName("isActive")
    private boolean isActive;

    // ********************************************
    // *** 2. THÊM 2 TRƯỜNG BỊ THIẾU VÀO ĐÂY ***
    // (Gson cần chúng để không bị lỗi)
    // ********************************************
    @SerializedName("cartItems")
    private List<Object> cartItems; // Dùng "Object" để nó khớp với mảng rỗng []

    @SerializedName("orderDetails")
    private List<Object> orderDetails; // Dùng "Object" để nó khớp với mảng rỗng []

    // --- Constructor và Getter/Setter (Giữ nguyên) ---

    public Dish() {
    }

    // (Bạn có thể giữ các constructor và getter/setter khác của mình)

    public int getId() { return id; }
    public void setId(int id) { this.id = id; }
    public String getName() { return name; }
    public void setName(String name) { this.name = name; }
    public double getPrice() { return price; }
    public void setPrice(double price) { this.price = price; }
    public String getDescription() { return description; }
    public void setDescription(String description) { this.description = description; }
    public String getCategory() { return category; }
    public void setCategory(String category) { this.category = category; }
    public String getImageUrl() { return imageUrl; }
    public void setImageUrl(String imageUrl) { this.imageUrl = imageUrl; }
    public boolean isActive() { return isActive; }
    public void setActive(boolean active) { isActive = active; }

    // Getter/Setter cho 2 trường mới (Không bắt buộc, nhưng nên có)
    public List<Object> getCartItems() { return cartItems; }
    public void setCartItems(List<Object> cartItems) { this.cartItems = cartItems; }
    public List<Object> getOrderDetails() { return orderDetails; }
    public void setOrderDetails(List<Object> orderDetails) { this.orderDetails = orderDetails; }
}