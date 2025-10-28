package com.example.cnpmnc_appfood;

import java.util.List;

public class AddToCartRequest {
    private String username;
    private List<CartItemRequest> items;

    public AddToCartRequest(String username, List<CartItemRequest> items) {
        this.username = username;
        this.items = items;
    }
}