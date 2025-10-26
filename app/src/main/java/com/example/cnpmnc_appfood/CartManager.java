package com.example.cnpmnc_appfood;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

public class CartManager {
    // Sử dụng Map để lưu trữ Dish và số lượng (Integer)
    private static Map<Dish, Integer> cart = new HashMap<>();

    // Thêm món vào giỏ (Tự động tăng số lượng nếu món đã có)
    public static void addToCart(Dish dish) {
        int currentQuantity = cart.getOrDefault(dish, 0);
        cart.put(dish, currentQuantity + 1);
    }

    // Cập nhật số lượng
    public static void updateQuantity(Dish dish, int quantity) {
        if (quantity <= 0) {
            cart.remove(dish); // Xóa khỏi giỏ nếu số lượng <= 0
        } else {
            cart.put(dish, quantity);
        }
    }

    // Xóa 1 món khỏi giỏ hàng (bất kể số lượng)
    public static void removeFromCart(Dish dish) {
        cart.remove(dish);
    }

    // Lấy danh sách Map Entry để Adapter có thể hiển thị (gồm Dish và Quantity)
    public static List<Map.Entry<Dish, Integer>> getCartList() {
        return new ArrayList<>(cart.entrySet());
    }

    // Tính tổng tiền
    public static double getTotalPrice() {
        double total = 0;
        for (Map.Entry<Dish, Integer> entry : cart.entrySet()) {
            total += entry.getKey().getPrice() * entry.getValue();
        }
        return total;
    }

    // Xóa toàn bộ giỏ hàng
    public static void clearCart() {
        cart.clear();
    }
}