package com.example.cnpmnc_appfood;

import android.content.Context;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.Menu;
import android.view.MenuInflater;
import android.view.MenuItem;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ListView;
import android.widget.Toast;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;

import java.util.ArrayList;
import java.util.List;

import com.example.cnpmnc_appfood.FoodDetailsFragment;

// Thêm các import cần thiết cho API
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class HomeFragment extends Fragment implements
        DishAdapter.OnDishClickListener,
        DishRepository.OnDishDataChangeListener,
        DishAdapter.OnCartClickListener {

    private ListView lvDishListHome;
    private DishAdapter adapter;
    private DishRepository dishRepository;
    private ApiService apiService; // THÊM ApiService

    @Override
    public void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setHasOptionsMenu(true);

        // Khởi tạo ApiService
        apiService = RetrofitClient.getApiService();
    }

    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container,
                             @Nullable Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_home, container, false);

        lvDishListHome = view.findViewById(R.id.lvDishListHome);

        dishRepository = DishRepository.getInstance();

        List<Dish> activeDishes = dishRepository.getActiveDishes();

        adapter = new DishAdapter(
                requireContext(),
                R.layout.item_dish,
                activeDishes,
                this,
                this
        );

        lvDishListHome.setAdapter(adapter);
        dishRepository.addListener(this);
        dishRepository.loadDishesFromServer();

        return view;
    }

    // --- LOGIC TOOLBAR/MENU GIỎ HÀNG ---
    // (Giữ nguyên các phương thức onCreateOptionsMenu, onOptionsItemSelected, navigateToCartFragment)

    @Override
    public void onCreateOptionsMenu(@NonNull Menu menu, @NonNull MenuInflater inflater) {
        inflater.inflate(R.menu.menu_cart, menu);
        super.onCreateOptionsMenu(menu, inflater);
    }

    @Override
    public boolean onOptionsItemSelected(@NonNull MenuItem item) {
        if (item.getItemId() == R.id.action_cart) {
            navigateToCartFragment();
            return true;
        }
        return super.onOptionsItemSelected(item);
    }

    private void navigateToCartFragment() {
        if (getActivity() != null) {
            getActivity().getSupportFragmentManager().beginTransaction()
                    .replace(R.id.fragment_container, new CartFragment())
                    .addToBackStack("menu_to_cart")
                    .commit();
        }
    }


    // --- CÁC PHƯƠNG THỨC KHÁC ---

    @Override
    public void onDestroyView() {
        super.onDestroyView();
        if (dishRepository != null) {
            dishRepository.removeListener(this);
        }
    }

    @Override
    public void onDishDataChanged() {
        if (dishRepository != null && adapter != null) {
            List<Dish> newActiveDishes = dishRepository.getActiveDishes();
            ((DishAdapter)adapter).setDishList(newActiveDishes);
            Log.d("HomeFragment", "Tổng số món đang bán: " + newActiveDishes.size());
        }
    }

    @Override
    public void onDishClick(Dish dish) {
        FoodDetailsFragment detailsFragment = FoodDetailsFragment.newInstance(dish.getId());
        if (getActivity() != null) {
            getActivity().getSupportFragmentManager().beginTransaction()
                    .replace(R.id.fragment_container, detailsFragment)
                    .addToBackStack(null)
                    .commit();
        }
    }

    /**
     * CẬP NHẬT: Phương thức này giờ sẽ gọi API POST /api/Cart/add
     */
    @Override
    public void onAddToCartClick(Dish dish) {
        // 1. Lấy username từ SharedPreferences
        SharedPreferences prefs = requireActivity().getSharedPreferences("UserPrefs", Context.MODE_PRIVATE);
        String username = prefs.getString("USERNAME", "guest");

        com.example.cnpmnc_appfood.CartItemRequest itemRequest = new com.example.cnpmnc_appfood.CartItemRequest(dish.getId(), 1);

        List<com.example.cnpmnc_appfood.CartItemRequest> itemsList = new ArrayList<>();
        itemsList.add(itemRequest);

        AddToCartRequest request = new AddToCartRequest(username, itemsList);

        // Hiển thị Toast đang xử lý
        Toast.makeText(getContext(), "Đang thêm " + dish.getName() + "...", Toast.LENGTH_SHORT).show();

        // 4. Gọi API
        apiService.addToCart(request).enqueue(new Callback<AddToCartResponse>() {
            @Override
            public void onResponse(@NonNull Call<com.example.cnpmnc_appfood.AddToCartResponse> call, @NonNull Response<AddToCartResponse> response) {
                if (response.isSuccessful()) {
                    Toast.makeText(getContext(), "Đã thêm " + dish.getName() + " vào giỏ hàng", Toast.LENGTH_SHORT).show();

                    // (Tùy chọn) Cập nhật CartManager cục bộ nếu vẫn muốn giữ đồng bộ
                    // CartManager.getInstance().addItemToCart(dish);
                } else {
                    Log.e("HomeFragment_API", "Lỗi API khi thêm: " + response.code());
                    Toast.makeText(getContext(), "Thêm vào giỏ hàng thất bại", Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(@NonNull Call<AddToCartResponse> call, @NonNull Throwable t) {
                Log.e("HomeFragment_API", "Lỗi kết nối: " + t.getMessage());
                Toast.makeText(getContext(), "Lỗi kết nối. Vui lòng thử lại.", Toast.LENGTH_SHORT).show();
            }
        });
    }
}
