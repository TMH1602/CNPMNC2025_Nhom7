package com.example.cnpmnc_appfood;

import android.content.Context;
import android.content.Intent; // THÊM import Intent
import android.content.SharedPreferences;
import android.net.Uri; // THÊM import Uri
import android.os.Bundle;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button; // THÊM import Button
import android.widget.ListView;
import android.widget.TextView;
import android.widget.Toast; // THÊM import Toast

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;

import java.text.NumberFormat;
import java.util.ArrayList;
import java.util.List;
import java.util.Locale;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class CartFragment extends Fragment implements CartAdapter.CartItemChangeListener {

    private ListView lvCartItems;
    private TextView tvTotalCost;
    private Button btnCheckout; // THÊM Button Thanh Toán
    private CartAdapter cartAdapter;
    private List<CartApiItemDetail> cartList;
    private String currentUsername;
    private ApiService apiService; // THÊM ApiService

    @Override
    public void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        SharedPreferences prefs = requireActivity().getSharedPreferences("UserPrefs", Context.MODE_PRIVATE);
        currentUsername = prefs.getString("USERNAME", "string");

        cartList = new ArrayList<>();
        apiService = RetrofitClient.getApiService(); // Khởi tạo ApiService
    }

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container,
                             @Nullable Bundle savedInstanceState) {

        View view = inflater.inflate(R.layout.fragment_cart, container, false);

        lvCartItems = view.findViewById(R.id.listCart);
        tvTotalCost = view.findViewById(R.id.tvTotalPrice);
        btnCheckout = view.findViewById(R.id.btnCheckout); // Tìm nút Thanh Toán

        cartAdapter = new CartAdapter(requireContext(), R.layout.item_cart, cartList, this);
        lvCartItems.setAdapter(cartAdapter);

        // GÁN LISTENER CHO NÚT THANH TOÁN
        btnCheckout.setOnClickListener(v -> startPaymentProcess());

        updateTotalCost();

        return view;
    }

    @Override
    public void onResume() {
        super.onResume();
        fetchCartFromServer();
    }

    /**
     * Phương thức Getter để CartAdapter có thể lấy username hiện tại.
     */
    public String getCurrentUsername() {
        return currentUsername;
    }

    // --- LOGIC GỌI API CART GET (Tải lại toàn bộ giỏ hàng) ---
    private void fetchCartFromServer() {

        apiService.getCartDetails(currentUsername).enqueue(new Callback<CartApiResponse>() {
            @Override
            public void onResponse(@NonNull Call<CartApiResponse> call, @NonNull Response<CartApiResponse> response) {
                if (response.isSuccessful() && response.body() != null && response.body().getItems() != null) {

                    List<CartApiItemDetail> serverCartItems = response.body().getItems();

                    cartList.clear();
                    cartList.addAll(serverCartItems);

                    updateUIAfterSync();
                } else {
                    Log.e("CartFragment", "Lỗi server khi tải giỏ hàng: " + response.code() + ", Message: " + response.message());
                    cartList.clear();
                    updateUIAfterSync();
                }
            }

            @Override
            public void onFailure(@NonNull Call<CartApiResponse> call, @NonNull Throwable t) {
                Log.e("CartFragment", "Lỗi kết nối khi tải giỏ hàng: " + t.getMessage());
                cartList.clear();
                updateUIAfterSync();
            }
        });
    }

    private void updateUIAfterSync() {
        cartAdapter.notifyDataSetChanged();
        updateTotalCost();
        Log.d("CartFragment", "UI đã cập nhật. Số món: " + cartList.size());
    }

    @Override
    public void onCartItemQuantityChanged() {
        Log.d("CartFragment", "Yêu cầu cập nhật giỏ hàng. Tải lại dữ liệu.");
        // Giữ lại fetchCartFromServer để đồng bộ sau khi nút tăng/giảm (chỉ cập nhật cục bộ) được nhấn
        fetchCartFromServer();
    }

    private void updateTotalCost() {
        double total = 0;

        for (CartApiItemDetail item : cartList) {
            total += item.getPrice() * item.getQuantity();
        }

        if (tvTotalCost != null) {
            NumberFormat nf = NumberFormat.getInstance(new Locale("vi", "VN"));
            String formattedTotal = nf.format(total) + " VNĐ";
            tvTotalCost.setText(formattedTotal);
        }
    }

    // --- LOGIC XỬ LÝ THANH TOÁN (ĐÃ SỬA LẠI HOÀN TOÀN) ---

    /**
     * Bắt đầu quy trình: Bước 1 là gọi API /Cart/checkout CỦA BẠN.
     */
    private void startPaymentProcess() {

        Toast.makeText(requireContext(), "Đang xử lý checkout...", Toast.LENGTH_SHORT).show();

        // Gọi API checkout mới (API của bạn, không phải VnPay)
        apiService.checkoutCart(currentUsername).enqueue(new Callback<CheckoutApiResponse>() {
            @Override
            public void onResponse(@NonNull Call<CheckoutApiResponse> call, @NonNull Response<CheckoutApiResponse> response) {
                // 1. KIỂM TRA THÀNH CÔNG
                if (response.isSuccessful() && response.body() != null) {

                    CheckoutApiResponse checkoutData = response.body();

                    // 2. CHUYỂN SANG TRANG XÁC NHẬN (CheckoutActivity)
                    Intent intent = new Intent(requireContext(), CheckoutActivity.class);

                    // 3. GỬI DỮ LIỆU SANG
                    // (Chúng ta dùng Serializable để gửi cả object)
                    intent.putExtra("CHECKOUT_DATA", checkoutData);

                    startActivity(intent);

                    // 4. (Quan trọng) Tải lại giỏ hàng vì nó đã được checkout (giờ trống)
                    fetchCartFromServer();

                } else {
                    // 5. XỬ LÝ LỖI (Ví dụ: 400 Bad Request nếu giỏ hàng rỗng?)
                    Log.e("CartFragment", "Lỗi khi gọi /api/Cart/checkout: " + response.code() + " - " + response.message());
                    Toast.makeText(requireContext(), "Lỗi khi checkout. Code: " + response.code(), Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(@NonNull Call<CheckoutApiResponse> call, @NonNull Throwable t) {
                // 6. XỬ LÝ LỖI KẾT NỐI
                Log.e("CartFragment", "Lỗi kết nối khi gọi /api/Cart/checkout: " + t.getMessage());
                Toast.makeText(requireContext(), "Lỗi kết nối mạng.", Toast.LENGTH_SHORT).show();
            }
        });
    }


    /**
     * Hàm này KHÔNG CÒN DÙNG Ở ĐÂY NỮA.
     * Chúng ta đã chuyển nó sang CheckoutActivity.java
     */
    // private void createVnPayPaymentLink(int orderId) { ... }


    /**
     * Hàm này KHÔNG CÒN DÙNG Ở ĐÂY NỮA.
     * Chúng ta đã chuyển nó sang CheckoutActivity.java
     */
    // private void openPaymentUrlInBrowser(String url) { ... }
}

