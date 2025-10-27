package com.example.cnpmnc_appfood;

import android.util.Log;
import java.util.ArrayList;
import java.util.List;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class DishRepository {

    private static final String TAG = "DishRepository";
    private static DishRepository instance;
    private final List<Dish> allDishes = new ArrayList<>();
    private final List<OnDishDataChangeListener> listeners = new ArrayList<>();

    private final ApiService apiService;

    public interface OnDishDataChangeListener {
        void onDishDataChanged();
    }

    public static DishRepository getInstance() {
        if (instance == null) {
            instance = new DishRepository();
        }
        return instance;
    }

    private DishRepository() {
        apiService = RetrofitClient.getApiService();
    }

    // --- QUẢN LÝ LISTENER (DATA CHANGE NOTIFICATION) ---

    public void addListener(OnDishDataChangeListener listener) {
        listeners.add(listener);
    }

    public void removeListener(OnDishDataChangeListener listener) {
        listeners.remove(listener);
    }

    private void notifyListeners() {
        for (OnDishDataChangeListener listener : listeners) {
            listener.onDishDataChanged(); // 👈 Kích hoạt cập nhật UI trong HomeFragment
        }
    }

    // --- LOGIC API GET VÀ LƯU TRỮ ---

    /**
     * Kích hoạt cuộc gọi API để tải danh sách món ăn.
     */
    public void loadDishesFromServer() {
        apiService.getMenu().enqueue(new Callback<List<Dish>>() {
            @Override
            public void onResponse(Call<List<Dish>> call, Response<List<Dish>> response) {
                if (response.isSuccessful() && response.body() != null) {
                    List<Dish> fetchedList = response.body();

                    // 1. Cập nhật danh sách nội bộ và lưu
                    setDishes(fetchedList);

                    // 2. Thông báo cho các Fragment đang lắng nghe
                    notifyListeners();
                    Log.d(TAG, "Tải API thành công. Thông báo cập nhật.");
                } else {
                    Log.e(TAG, "Lỗi Server khi tải menu: " + response.code());
                }
            }

            @Override
            public void onFailure(Call<List<Dish>> call, Throwable t) {
                Log.e(TAG, "Lỗi kết nối API Menu: " + t.getMessage());
                // Vẫn gọi notifyListeners() để Fragment biết đã có lỗi và có thể hiển thị thông báo lỗi
                notifyListeners();
            }
        });
    }

    // Lưu trữ dữ liệu mới nhận được
    public void setDishes(List<Dish> fetchedList) {
        allDishes.clear();
        allDishes.addAll(fetchedList);
    }

    // ------------------------------------------
    // --- LOGIC TRUY XUẤT DỮ LIỆU CỤC BỘ ---
    // ------------------------------------------

    /**
     * Trả về danh sách các món ăn có IsActive = true.
     */
    public List<Dish> getActiveDishes() {
        List<Dish> activeList = new ArrayList<>();
        if (allDishes != null) {
            for (Dish dish : allDishes) {
                if (dish.isActive()) {
                    activeList.add(dish);
                }
            }
        }
        return activeList;
    }

    /**
     * Tìm và trả về một món ăn cụ thể dựa trên ID.
     */
    public Dish getDishById(int id) {
        for (Dish dish : allDishes) {
            if (dish.getId() == id) {
                return dish;
            }
        }
        return null;
    }

    public List<Dish> getAllDishes() {
        return new ArrayList<>(allDishes);
    }

    public int getActiveDishCount() {
        return getActiveDishes().size();
    }

    // --- LOGIC API POST KHÁC ---
    public void addDish(String name, String desc, double price, String imageUrl) {

        Dish newDish = new Dish();
        newDish.setName(name);
        newDish.setDescription(desc);
        newDish.setPrice(price);
        newDish.setImageUrl(imageUrl);
        newDish.setActive(true);

        apiService.createDish(newDish).enqueue(new retrofit2.Callback<Dish>() {
            @Override
            public void onResponse(Call<Dish> call, Response<Dish> response) {
                if (response.isSuccessful() && response.body() != null) {
                    allDishes.add(response.body());
                    notifyListeners();
                } else {
                    Log.e(TAG, "Lỗi Server khi thêm món: " + response.code());
                }
            }

            @Override
            public void onFailure(Call<Dish> call, Throwable t) {
                Log.e(TAG, "Lỗi kết nối khi thêm món: " + t.getMessage());
            }
        });
    }
}