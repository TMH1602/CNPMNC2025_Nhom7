package com.example.cnpmnc_appfood;

import java.util.ArrayList;
import java.util.List;
import android.util.Log;

public class CartManager {

    private final List<CartItem> cartItems = new ArrayList<>();
    private static CartManager instance;

    public static CartManager getInstance() {
        if (instance == null) {
            instance = new CartManager();
        }
        return instance;
    }

    private CartManager() {
        // Private constructor
    }

    // --- LOGIC ĐỒNG BỘ HÓA TỪ SERVER ---

    /**
     * Đồng bộ hóa giỏ hàng cục bộ bằng dữ liệu từ API.
     * Phương thức này cần DishRepository để tìm chi tiết Dish (khắc phục lỗi hiển thị).
     */
    public void syncCartFromServer(List<CartApiItemDetail> serverItemDetails) {
        // Giả định DishRepository tồn tại
        DishRepository dishRepository = DishRepository.getInstance();
        cartItems.clear();

        for (CartApiItemDetail apiItem : serverItemDetails) {
            Dish dish = dishRepository.getDishById(apiItem.getProductId());

            if (dish == null) {
                // 🎯 KHẮC PHỤC LỖI: TẠO DISH TẠM THỜI 🎯
                dish = new Dish();
                dish.setId(apiItem.getProductId());
                dish.setName(apiItem.getProductName());
                dish.setPrice(apiItem.getPrice());
                dish.setImageUrl(apiItem.getImageUrl());
                dish.setActive(true);

                Log.w("CartManager", "Dish ID " + apiItem.getProductId() + " được tạo tạm thời.");
            }

            // Nếu dish đã được tìm thấy (hoặc vừa được tạo tạm thời)
            CartItem localItem = new CartItem(dish, apiItem.getQuantity());
            cartItems.add(localItem);
        }
    }

    // --- LOGIC GIỎ HÀNG CƠ BẢN (Cho nút Tăng/Giảm Cục bộ) ---

    public void addItemToCart(Dish dish) {
        for (CartItem item : cartItems) {
            if (item.getDish().getId() == dish.getId()) {
                item.setQuantity(item.getQuantity() + 1);
                return;
            }
        }
        cartItems.add(new CartItem(dish, 1));
    }

    public void updateQuantity(Dish dish, int newQuantity) {
        for (CartItem item : cartItems) {
            if (item.getDish().getId() == dish.getId()) {
                if (newQuantity > 0) {
                    item.setQuantity(newQuantity);
                } else {
                    cartItems.remove(item);
                }
                return;
            }
        }
    }

    public List<CartItem> getCartItems() {
        return cartItems;
    }

    public void clearCart() {
        cartItems.clear();
    }
}