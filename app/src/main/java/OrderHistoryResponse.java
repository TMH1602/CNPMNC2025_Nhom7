package com.example.cnpmnc_appfood;

import java.util.List;
// Đảm bảo OrderItemDetail cũng đã được định nghĩa đúng

public class OrderHistoryResponse {
    private int orderId;
    private String orderDate;
    private double totalAmount;
    private String status;
    private List<OrderItemDetail> items;

    public int getOrderId() { return orderId; }
    public String getOrderDate() { return orderDate; }
    public double getTotalAmount() { return totalAmount; }
    public String getStatus() { return status; }
    public List<OrderItemDetail> getItems() { return items; }
}
