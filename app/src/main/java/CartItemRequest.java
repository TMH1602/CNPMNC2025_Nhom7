package com.example.cnpmnc_appfood;

import com.google.gson.annotations.SerializedName;

public class CartItemRequest {
    @SerializedName("productId")
    private int productId;

    @SerializedName("quantity")
    private int quantity;

    public CartItemRequest(int productId, int quantity) {
        this.productId = productId;
        this.quantity = quantity;
    }
    // Cần Getters/Setters nếu Retrofit không gọi constructor đầy đủ
}