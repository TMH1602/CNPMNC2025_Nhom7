package com.example.cnpmnc_appfood;

import java.util.ArrayList;
import java.util.List;

public class CartManager {
    private static List<Dish> cart = new ArrayList<>();

    // Thêm món vào giỏ
    public static void addToCart(Dish dish) {
        cart.add(dish);
    }

    // Lấy danh sách giỏ hàng
    public static List<Dish> getCart() {
        return cart;
    }

    // Xóa toàn bộ giỏ hàng
    public static void clearCart() {
        cart.clear();
    }

    // Xóa 1 món khỏi giỏ hàng
    public static void removeFromCart(Dish dish) {
        cart.remove(dish);
    }
}
