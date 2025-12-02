package com.example.cnpmnc_appfood;

import java.util.List;

public class CartApiResponse {
    private int id;
    private String username;
    private List<CartApiItemDetail> items;
    public int getOrderId() { return id; }
    public List<CartApiItemDetail> getItems() { return items; }
}