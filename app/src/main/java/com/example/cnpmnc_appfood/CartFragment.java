package com.example.cnpmnc_appfood;

import android.content.Context;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ListView;
import android.widget.TextView;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;
import java.util.List;
import java.util.ArrayList;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class CartFragment extends Fragment implements
        CartAdapter.CartItemChangeListener, DishRepository.OnDishDataChangeListener {

    private ListView lvCartItems;
    private TextView tvTotalCost;
    private CartAdapter cartAdapter;
    private List<CartItem> cartList;
    private DishRepository dishRepository;

    @Override
    public void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        dishRepository = DishRepository.getInstance();
    }

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container,
                             @Nullable Bundle savedInstanceState) {

        View view = inflater.inflate(R.layout.fragment_cart, container, false);

        lvCartItems = view.findViewById(R.id.listCart);
        tvTotalCost = view.findViewById(R.id.tvTotalPrice);

        cartList = CartManager.getInstance().getCartItems();

        cartAdapter = new CartAdapter(requireContext(), R.layout.item_cart, cartList, this);
        lvCartItems.setAdapter(cartAdapter);

        updateTotalCost();

        return view;
    }

    @Override
    public void onResume() {
        super.onResume();
        // 1. Đăng ký Listener Menu (để tải Menu trước)
        dishRepository.addListener(this);
        dishRepository.loadDishesFromServer();

        // 2. Gửi trạng thái giỏ hàng cục bộ lên server (Đồng bộ POST)
        sendCartToServer();
    }

    @Override
    public void onPause() {
        super.onPause();
        dishRepository.removeListener(this);
    }

    // 🎯 PHƯƠNG THỨC GỌI KHI MENU TẢI XONG 🎯
    @Override
    public void onDishDataChanged() {
        Log.d("CartFragment", "Dữ liệu Menu đã sẵn sàng. Bắt đầu tải Giỏ hàng.");
        fetchCartFromServer();
    }

    // --- LOGIC GỌI API CART GET (LẤY DỮ LIỆU) ---
    private void fetchCartFromServer() {
        SharedPreferences prefs = requireActivity().getSharedPreferences("UserPrefs", Context.MODE_PRIVATE);
        String username = prefs.getString("USERNAME", "guest");

        if (username.equals("guest")) {
            Log.w("CartFragment", "Chưa đăng nhập, không tải giỏ hàng từ server.");
            return;
        }

        ApiService apiService = RetrofitClient.getApiService();

        apiService.getCartDetails(username).enqueue(new Callback<CartApiResponse>() {
            @Override
            public void onResponse(@NonNull Call<CartApiResponse> call, @NonNull Response<CartApiResponse> response) {
                if (response.isSuccessful() && response.body() != null) {
                    // Lấy mảng items từ đối tượng cha (để khắc phục lỗi parsing)
                    List<CartApiItemDetail> serverCartItems = response.body().getItems();

                    // ĐỒNG BỘ VÀ CẬP NHẬT CARTMANAGER
                    CartManager.getInstance().syncCartFromServer(serverCartItems);

                    updateUIAfterSync();
                } else {
                    Log.e("CartFragment", "Lỗi server khi tải giỏ hàng: " + response.code() + ", Message: " + response.message());
                    updateUIAfterSync();
                }
            }

            @Override
            public void onFailure(@NonNull Call<CartApiResponse> call, @NonNull Throwable t) {
                Log.e("CartFragment", "Lỗi kết nối khi tải giỏ hàng: " + t.getMessage());
                updateUIAfterSync();
            }
        });
    }

    private void updateUIAfterSync() {
        // Cập nhật danh sách Adapter
        cartList.clear();
        cartList.addAll(CartManager.getInstance().getCartItems());

        cartAdapter.notifyDataSetChanged();
        updateTotalCost();
        Log.d("CartFragment", "UI đã cập nhật. Số món: " + cartList.size());
    }

    // --- CÁC PHẦN KHÁC ---

    private void sendCartToServer() {
        SharedPreferences prefs = requireActivity().getSharedPreferences("UserPrefs", Context.MODE_PRIVATE);
        String currentUsername = prefs.getString("USERNAME", "guest");

        if (currentUsername.equals("guest")) {
            return;
        }

        List<CartItem> localCartItems = CartManager.getInstance().getCartItems();

        if (localCartItems.isEmpty()) {
            Log.i("CartFragment", "Giỏ hàng cục bộ rỗng, không gửi API sync.");
            return;
        }

        List<CartItemRequest> itemRequests = new ArrayList<>();
        for (CartItem item : localCartItems) {
            int productId = item.getDish().getId();
            int quantity = item.getQuantity();
            itemRequests.add(new CartItemRequest(productId, quantity));
        }

        AddToCartRequest request = new AddToCartRequest(currentUsername, itemRequests);

        ApiService apiService = RetrofitClient.getApiService();

        apiService.addToCart(request).enqueue(new Callback<AddToCartResponse>() {
            @Override
            public void onResponse(@NonNull Call<AddToCartResponse> call, @NonNull Response<AddToCartResponse> response) {
                if (response.isSuccessful() && response.body() != null) {
                    Log.i("CartFragment", "Đồng bộ giỏ hàng lên server thành công.");
                } else {
                    Log.e("CartFragment", "Lỗi Server khi đồng bộ giỏ hàng: " + response.code());
                }
            }

            @Override
            public void onFailure(@NonNull Call<AddToCartResponse> call, @NonNull Throwable t) {
                Log.e("CartFragment", "Lỗi kết nối khi đồng bộ giỏ hàng: " + t.getMessage());
            }
        });
    }

    @Override
    public void onCartItemQuantityChanged() {
        Log.d("CartFragment", "Dữ liệu giỏ hàng đã thay đổi, cập nhật UI.");
        updateTotalCost();
    }

    private void updateTotalCost() {
        double total = 0;

        for (CartItem item : cartList) {
            total += item.getDish().getPrice() * item.getQuantity();
        }

        if (tvTotalCost != null) {
            tvTotalCost.setText(String.format("Tổng cộng: %,.0f VNĐ", total));
        }
    }
}