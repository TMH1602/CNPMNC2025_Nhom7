package com.example.cnpmnc_appfood;

import com.google.gson.annotations.SerializedName;
import java.util.List;

public class AddToCartRequest {
    @SerializedName("username")
    private String username;

    @SerializedName("items")
    private List<CartItemRequest> items;

    public AddToCartRequest(String username, List<CartItemRequest> items) {
        this.username = username;
        this.items = items;
    }
}