package com.example.cnpmnc_appfood;

import com.google.gson.annotations.SerializedName;

public class CartApiItemDetail {

    // Các trường cần thiết để đồng bộ hóa
    @SerializedName("productId")
    private int productId;

    @SerializedName("quantity")
    private int quantity;

    // Các trường chi tiết món ăn (để có thể hiển thị mà không cần getDishById)
    @SerializedName("productName")
    private String productName;

    @SerializedName("price")
    private double price; // Giá đơn vị

    @SerializedName("imageUrl")
    private String imageUrl;

    // Getters
    public int getProductId() { return productId; }
    public int getQuantity() { return quantity; }
    public String getProductName() { return productName; }
    public double getPrice() { return price; }
    public String getImageUrl() { return imageUrl; }
}