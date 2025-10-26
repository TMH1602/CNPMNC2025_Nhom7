package com.example.cnpmnc_appfood;

import java.util.ArrayList;
import java.util.List;

public class DishRepository {
    private static List<Dish> dishes = new ArrayList<>();
    private static int nextId = 4;
    private static List<OnDishDataChangeListener> listeners = new ArrayList<>();

    static {
        // CẬP NHẬT: Dùng lại URL hoặc URI giả định (String) cho dữ liệu khởi tạo
        dishes.add(new Dish(1, "Burger Bò", "Bánh mì kẹp thịt bò ngon tuyệt", 55000, "https://imgur.com/gallery/burger-burger-burger-Jmb2lZw"));
        dishes.add(new Dish(2, "Gà Rán", "Gà chiên giòn hấp dẫn", 45000, "https://i.imgur.com/EfQbQmV.jpg"));
        dishes.add(new Dish(3, "Pizza Phô Mai", "Pizza nhiều phô mai kéo sợi", 120000, "https://i.imgur.com/z6S4m7k.jpg"));
    }

    public interface OnDishDataChangeListener {
        void onDishDataChanged();
    }

    public static void addListener(OnDishDataChangeListener listener) {
        listeners.add(listener);
    }

    public static void removeListener(OnDishDataChangeListener listener) {
        listeners.remove(listener);
    }

    private static void notifyDataChange() {
        for (OnDishDataChangeListener listener : listeners) {
            listener.onDishDataChanged();
        }
    }

    public static Dish getDishById(int id) {
        for (Dish d : dishes) {
            if (d.getId() == id) return d;
        }
        return null;
    }

    public static List<Dish> getAllDishes() {
        return dishes;
    }

    // SỬA: Hàm addDish nhận String imageUrl
    public static void addDish(String name, String description, double price, String imageUrl) {
        Dish newDish = new Dish(nextId++, name, description, price, imageUrl);
        dishes.add(newDish);
        notifyDataChange();
    }

    // SỬA: Hàm updateDish nhận String imageUrl
    public static void updateDish(int id, String name, String description, double price, String imageUrl) {
        for (int i = 0; i < dishes.size(); i++) {
            if (dishes.get(i).getId() == id) {
                dishes.set(i, new Dish(id, name, description, price, imageUrl));
                notifyDataChange();
                return;
            }
        }
    }

    public static void deleteDish(int id) {
        // Logic xóa (giữ nguyên)
    }
}