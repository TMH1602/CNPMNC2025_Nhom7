package com.example.cnpmnc_appfood;

import com.google.gson.annotations.SerializedName;
import java.io.Serializable;

// Dùng Serializable để có thể gửi cả đối tượng này qua Intent
public class CheckoutApiResponse implements Serializable {

    @SerializedName("orderId")
    private int orderId;

    @SerializedName("totalAmount")
    private double totalAmount;

    @SerializedName("orderDate")
    private String orderDate;

    @SerializedName("message")
    private String message;

    // --- Getters ---

    public int getOrderId() {
        return orderId;
    }

    public double getTotalAmount() {
        return totalAmount;
    }

    public String getOrderDate() {
        return orderDate;
    }

    public String getMessage() {
        return message;
    }
}
