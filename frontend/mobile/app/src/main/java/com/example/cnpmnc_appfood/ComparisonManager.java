package com.example.cnpmnc_appfood;

import java.util.ArrayList;
import java.util.List;

public class ComparisonManager {
    // Giới hạn so sánh 2 món ăn
    private static final int MAX_COMPARE_ITEMS = 2;
    private static List<Dish> comparisonList = new ArrayList<>();

    // Thêm món ăn vào danh sách so sánh
    // Trả về true nếu thêm thành công, false nếu danh sách đã đầy
    public static boolean addToCompare(Dish dish) {
        // Kiểm tra xem món ăn đã có trong danh sách chưa
        for (Dish d : comparisonList) {
            if (d.getId() == dish.getId()) {
                return true; // Đã có sẵn, không cần thêm lại
            }
        }

        if (comparisonList.size() < MAX_COMPARE_ITEMS) {
            comparisonList.add(dish);
            return true;
        }
        return false; // Danh sách đã đầy
    }

    // Lấy danh sách món ăn đang so sánh
    public static List<Dish> getComparisonList() {
        return comparisonList;
    }

    // Xóa toàn bộ danh sách so sánh
    public static void clearComparison() {
        comparisonList.clear();
    }

    // Lấy số lượng món trong danh sách
    public static int getComparisonListSize() {
        return comparisonList.size();
    }
}