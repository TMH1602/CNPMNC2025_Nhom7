package com.example.cnpmnc_appfood;

// 💡 Cần import annotations của Gson
// import com.google.gson.annotations.SerializedName;
public class CartApiItemDetail {
    private int productId;
    private String productName;
    private double price;
    private int quantity;
    private String imageUrl;
    private double totalItemPrice;

    // Getters
    public int getProductId() { return productId; }
    public String getProductName() { return productName; }
    public double getPrice() { return price; }
    public int getQuantity() { return quantity; }
    public String getImageUrl() { return imageUrl; }
    public double getTotalItemPrice() { return totalItemPrice; }

    // Setters (Cần thiết cho Adapter cập nhật cục bộ)
    public void setQuantity(int quantity) { this.quantity = quantity; }
}