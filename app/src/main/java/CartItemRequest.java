package com.example.cnpmnc_appfood;

public class CartItemRequest {
    private int productId;
    private int quantity;

    public CartItemRequest(int productId, int quantity) {
        this.productId = productId;
        this.quantity = quantity;
    }

    public int getProductId() { return productId; }
    public int getQuantity() { return quantity; }
}