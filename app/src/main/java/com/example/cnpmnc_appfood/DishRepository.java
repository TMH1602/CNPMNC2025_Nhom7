package com.example.cnpmnc_appfood;

import android.util.Log;
import androidx.annotation.NonNull;
import java.util.ArrayList;
import java.util.List;

import okhttp3.OkHttpClient;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;
import retrofit2.Retrofit;
import retrofit2.converter.gson.GsonConverterFactory;

public class DishRepository {

    // SỬA 1: ĐIỀU CHỈNH ĐƯỜNG DẪN URL
    // *** CẢNH BÁO QUAN TRỌNG ***
    // Nếu bạn chạy API (backend) trên máy tính (localhost) và chạy ứng dụng
    // trên MÁY ẢO Android, bạn phải dùng IP "10.0.2.2" thay vì "localhost".
    // Nếu bạn dùng MÁY THẬT, hãy dùng IP của máy tính (ví dụ: 192.168.1.10)
    //
    // THAY THẾ IP NÀY CHO ĐÚNG:
    private static final String BASE_URL = "https://10.0.2.2:7132/"; // Dùng 10.0.2.2 cho máy ảo

    private static DishRepository instance;
    private final ApiService apiService;

    // Danh sách này sẽ là "nguồn sự thật" (source of truth)
    private final List<Dish> allDishes = new ArrayList<>();

    // Danh sách các listener (chính là HomeFragment)
    private final List<OnDishDataChangeListener> listeners = new ArrayList<>();

    // Interface để HomeFragment lắng nghe
    public interface OnDishDataChangeListener {
        void onDishDataChanged();
    }

    // Phương thức Private Constructor cho Singleton
    private DishRepository() {

        // SỬA 1: Lấy OkHttpClient không an toàn từ lớp bạn vừa tạo
        OkHttpClient okHttpClient = UnsafeOkHttpClient.getUnsafeOkHttpClient();

        // Khởi tạo Retrofit
        Retrofit retrofit = new Retrofit.Builder()
                .baseUrl(BASE_URL)
                .addConverterFactory(GsonConverterFactory.create())
                // SỬA 2: Thêm dòng .client() này vào
                .client(okHttpClient)
                .build();

        apiService = retrofit.create(ApiService.class);
    }

    // Singleton pattern (Giống code của bạn)
    public static synchronized DishRepository getInstance() {
        if (instance == null) {
            instance = new DishRepository();
        }
        return instance;
    }

    // Các phương thức quản lý listener (Giống code của bạn)
    public void addListener(OnDishDataChangeListener listener) {
        listeners.add(listener);
    }

    public void removeListener(OnDishDataChangeListener listener) {
        listeners.remove(listener);
    }

    // Thông báo cho tất cả listener (HomeFragment) rằng dữ liệu đã thay đổi
    private void notifyListeners() {
        for (OnDishDataChangeListener listener : listeners) {
            listener.onDishDataChanged();
        }
    }

    // Lấy danh sách món ăn hiện tại (Giống code của bạn)
    public List<Dish> getAllDishes() {
        return allDishes;
    }


    // ********************************************
    // *** THÊM PHƯƠNG THỨC NÀY VÀO ***
    // (FoodDetailsFragment cần phương thức này)
    // ********************************************
    /**
     * Tìm một món ăn trong danh sách đã tải về dựa theo ID.
     * @param foodId ID của món ăn cần tìm.
     * @return Đối tượng Dish nếu tìm thấy, ngược lại trả về null.
     */
    public Dish getDishById(int foodId) {
        // Duyệt qua danh sách allDishes (đã được tải về từ API)
        for (Dish dish : allDishes) {
            if (dish.getId() == foodId) {
                return dish; // Trả về món ăn nếu ID khớp
            }
        }
        return null; // Trả về null nếu không tìm thấy món ăn
    }
    public void addDish(String name, String description, double price, String imageUrl) {

        // 1. Tạo đối tượng Dish từ thông tin đầu vào
        Dish newDish = new Dish();
        newDish.setName(name);
        newDish.setDescription(description);
        newDish.setPrice(price);
        newDish.setImageUrl(imageUrl); // <-- Đọc cảnh báo bên dưới
        newDish.setCategory("Món mới"); // Tạm gán (vì form của bạn thiếu category)

        // 2. Gọi API để tạo món ăn
        Call<Dish> call = apiService.createDish(newDish);

        // 3. Thực thi bất đồng bộ
        call.enqueue(new Callback<Dish>() {
            @Override
            public void onResponse(@NonNull Call<Dish> call, @NonNull Response<Dish> response) {
                if (response.isSuccessful() && response.body() != null) {
                    // 4. Thêm món ăn mới (server trả về) vào danh sách
                    Dish createdDish = response.body();
                    allDishes.add(createdDish);

                    // 5. Thông báo cho HomeFragment cập nhật UI
                    notifyListeners();

                    Log.d("DishRepository", "Thêm món ăn thành công: " + createdDish.getName());
                } else {
                    Log.e("DishRepository", "Lỗi khi thêm món ăn: " + response.code());
                }
            }

            @Override
            public void onFailure(@NonNull Call<Dish> call, @NonNull Throwable t) {
                Log.e("DishRepository", "Lỗi API (addDish): " + t.getMessage(), t);
            }
        });
    }
    // ********************************************
    // *** THÊM PHƯƠNG THỨC NÀY VÀO ***
    // (Đây là phiên bản Java của code C#)
    // ********************************************
    /**
     * Đếm số lượng món ăn đang "Active" (IsActive = true)
     * mà repository đang giữ.
     * @return Số lượng món ăn đang hoạt động.
     */
    public int getActiveDishCount() {
        int count = 0;

        // Duyệt qua toàn bộ danh sách allDishes
        for (Dish dish : allDishes) {
            // Kiểm tra trường isActive (giống hệt C#)
            if (dish.isActive()) {
                count++;
            }
        }
        return count; // Trả về tổng số lượng
    }


    // SỬA 2: TRIỂN KHAI LOGIC GỌI API
    public void loadDishesFromServer() {
        // Gọi API
        Call<List<Dish>> call = apiService.getMenu();

        // Thực thi cuộc gọi BẤT ĐỒNG BỘ (trên một luồng khác)
        call.enqueue(new Callback<List<Dish>>() {
            @Override
            public void onResponse(@NonNull Call<List<Dish>> call, @NonNull Response<List<Dish>> response) {
                // 1. GỌI API THÀNH CÔNG
                if (response.isSuccessful() && response.body() != null) {
                    // Xóa dữ liệu cũ
                    allDishes.clear();
                    // Thêm dữ liệu mới từ API
                    allDishes.addAll(response.body());

                    // 2. THÔNG BÁO CHO HOMEFRAGMENT
                    // Báo cho HomeFragment (và các listener khác)
                    // "Này, dữ liệu mới về rồi, cập nhật UI đi!"
                    notifyListeners();

                    Log.d("DishRepository", "Tải dữ liệu thành công: " + allDishes.size() + " món.");
                } else {
                    Log.e("DishRepository", "Lỗi Response: " + response.code());
                }
            }

            @Override
            public void onFailure(@NonNull Call<List<Dish>> call, @NonNull Throwable t) {
                // 3. GỌI API THẤT BẠI
                // Thường là do mất mạng, sai URL, hoặc lỗi SSL
                Log.e("DishRepository", "Lỗi gọi API: " + t.getMessage(), t);
            }
        });
    }
}