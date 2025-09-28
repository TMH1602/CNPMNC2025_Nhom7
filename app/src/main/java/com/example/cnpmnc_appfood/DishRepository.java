package com.example.cnpmnc_appfood;

import java.util.ArrayList;
import java.util.List;

public class DishRepository {
    private static List<Dish> dishes = new ArrayList<>();

    static {
        dishes.add(new Dish(1, "Burger Bò", "Bánh mì kẹp thịt bò ngon tuyệt", 55000, "https://i.imgur.com/7p7zF0J.jpg"));
        dishes.add(new Dish(2, "Gà Rán", "Gà chiên giòn hấp dẫn", 45000, "https://i.imgur.com/EfQbQmV.jpg"));
        dishes.add(new Dish(3, "Pizza Phô Mai", "Pizza nhiều phô mai kéo sợi", 120000, "https://i.imgur.com/z6S4m7k.jpg"));
    }

    public static Dish getDishById(int id) {
        for (Dish d : dishes) {
            if (d.getId() == id) return d;
        }
        return null;
    }
}
