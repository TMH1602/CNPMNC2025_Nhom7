package com.example.cnpmnc_appfood;

import com.google.gson.annotations.SerializedName;
import java.util.List;

public class CartApiResponse {
    @SerializedName("id")
    private int id;

    @SerializedName("username")
    private String username;

    @SerializedName("items") // PHẢI KHỚP TÊN NÀY
    private List<CartApiItemDetail> items; // Mảng các mục chi tiết

    public List<CartApiItemDetail> getItems() {
        return items;
    }
}