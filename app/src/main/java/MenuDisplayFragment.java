package com.example.cnpmnc_appfood;

import android.content.Context; // Import mới
import android.content.SharedPreferences; // Import mới
import android.os.Bundle;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ListView;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class MenuDisplayFragment extends Fragment implements
        DishAdapter.OnDishClickListener, DishAdapter.OnCartClickListener {

    private ListView lvDishList;
    private DishAdapter dishAdapter;
    private final List<Dish> dishList = new ArrayList<>();

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container,
                             @Nullable Bundle savedInstanceState) {
        // Sử dụng layout của HomeFragment
        View view = inflater.inflate(R.layout.fragment_home, container, false);

        lvDishList = view.findViewById(R.id.lvDishListHome);

        // Khởi tạo Adapter với 5 tham số
        dishAdapter = new DishAdapter(requireContext(), R.layout.item_dish, dishList, this, this);
        lvDishList.setAdapter(dishAdapter);

        return view;
    }

    @Override
    public void onViewCreated(@NonNull View view, @Nullable Bundle savedInstanceState) {
        super.onViewCreated(view, savedInstanceState);
        fetchDishList();
    }

    // --- LOGIC TẢI MENU (GET) ---
    private void fetchDishList() {
        ApiService apiService = RetrofitClient.getApiService();

        apiService.getMenu().enqueue(new Callback<List<Dish>>() {
            @Override
            public void onResponse(@NonNull Call<List<Dish>> call, @NonNull Response<List<Dish>> response) {
                if (response.isSuccessful() && response.body() != null) {
                    List<Dish> fetchedList = response.body();

                    if (fetchedList.size() > 0) {
                        dishAdapter.setDishList(fetchedList);
                        Log.i("API_SUCCESS", "Menu tải thành công: " + fetchedList.size() + " món.");
                    }
                } else {
                    Toast.makeText(getContext(), "Lỗi tải dữ liệu menu.", Toast.LENGTH_LONG).show();
                }
            }

            @Override
            public void onFailure(@NonNull Call<List<Dish>> call, @NonNull Throwable t) {
                Log.e("API_FAILURE", "Không thể kết nối API Menu.", t);
                Toast.makeText(getContext(), "Không thể kết nối API.", Toast.LENGTH_LONG).show();
            }
        });
    }

    // --- XỬ LÝ CLICK THÊM VÀO GIỎ (POST) ---
    @Override
    public void onAddToCartClick(Dish dish) {

        // 🎯 BƯỚC 1: ĐỌC TÊN NGƯỜI DÙNG TỪ SHAREDPREFERENCES 🎯
        SharedPreferences prefs = requireActivity().getSharedPreferences("UserPrefs", Context.MODE_PRIVATE);

        // Lấy tên người dùng đã lưu. Nếu chưa đăng nhập, mặc định là "guest".
        String currentUsername = prefs.getString("USERNAME", "guest");

        int productId = dish.getId();
        int quantity = 1;

        // 2. Chuẩn bị Request Body
        CartItemRequest itemRequest = new CartItemRequest(productId, quantity);
        List<CartItemRequest> items = Collections.singletonList(itemRequest);

        // 3. TẠO REQUEST VỚI USERNAME ĐÃ LƯU
        AddToCartRequest request = new AddToCartRequest(currentUsername, items);

        // 4. Gọi API
        callAddToCartApi(request, dish.getName());
    }

    /**
     * Thực hiện cuộc gọi API POST đến /api/Cart/add.
     */
    private void callAddToCartApi(AddToCartRequest request, String dishName) {
        ApiService apiService = RetrofitClient.getApiService();

        apiService.addToCart(request).enqueue(new Callback<AddToCartResponse>() {
            @Override
            public void onResponse(@NonNull Call<AddToCartResponse> call, @NonNull Response<AddToCartResponse> response) {
                if (response.isSuccessful() && response.body() != null) {
                    // LẤY VÀ HIỂN THỊ MESSAGE TỪ SERVER
                    String message = response.body().getMessage();
                    Toast.makeText(getContext(), message, Toast.LENGTH_SHORT).show();

                } else {
                    Log.e("CART_API", "Lỗi Server: " + response.code());
                    Toast.makeText(getContext(), "Lỗi Server khi thêm " + dishName + ": Code " + response.code(), Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(@NonNull Call<AddToCartResponse> call, @NonNull Throwable t) {
                Log.e("CART_API", "Thêm giỏ hàng thất bại: " + t.getMessage(), t);
                Toast.makeText(getContext(), "Lỗi kết nối khi thêm món.", Toast.LENGTH_SHORT).show();
            }
        });
    }

    // --- XỬ LÝ CLICK ITEM ---
    @Override
    public void onDishClick(Dish dish) {
        Toast.makeText(getContext(), "Xem chi tiết: " + dish.getName(), Toast.LENGTH_SHORT).show();
        // Logic chuyển màn hình chi tiết món ăn
    }
}