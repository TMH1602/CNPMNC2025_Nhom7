package com.example.cnpmnc_appfood;

import com.google.gson.annotations.SerializedName;
import java.util.List;

public class Dish {

    // --- Trường dữ liệu ---
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

    @SerializedName("isActive")
    private boolean isActive;

    // --- Trường phức tạp (Dùng List<Object> để parsing không lỗi) ---
    @SerializedName("cartItems")
    private List<Object> cartItems;

    @SerializedName("orderDetails")
    private List<Object> orderDetails;

    // --- Constructor BẮT BUỘC cho Gson ---
    public Dish() {
    }

    // --- Getters and Setters (đã kiểm tra và đúng) ---

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

    public List<Object> getCartItems() { return cartItems; }
    public void setCartItems(List<Object> cartItems) { this.cartItems = cartItems; }

    public List<Object> getOrderDetails() { return orderDetails; }
    public void setOrderDetails(List<Object> orderDetails) { this.orderDetails = orderDetails; }
}