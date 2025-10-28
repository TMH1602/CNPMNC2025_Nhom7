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

import java.text.NumberFormat;
import java.util.ArrayList;
import java.util.List;
import java.util.Locale;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

// 💡 Loại bỏ việc implements DishRepository.OnDishDataChangeListener
public class CartFragment extends Fragment implements CartAdapter.CartItemChangeListener {

    private ListView lvCartItems;
    private TextView tvTotalCost;
    private CartAdapter cartAdapter;
    // 💡 cartList giờ lưu trữ CartApiItemDetail trực tiếp
    private List<CartApiItemDetail> cartList;
    private String currentUsername;

    @Override
    public void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        // Lấy username từ SharedPreferences
        SharedPreferences prefs = requireActivity().getSharedPreferences("UserPrefs", Context.MODE_PRIVATE);
        // "string" là giá trị mặc định nếu chưa đăng nhập
        currentUsername = prefs.getString("USERNAME", "string");

        // Khởi tạo list
        cartList = new ArrayList<>();
    }

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container,
                             @Nullable Bundle savedInstanceState) {

        View view = inflater.inflate(R.layout.fragment_cart, container, false);

        lvCartItems = view.findViewById(R.id.listCart);
        tvTotalCost = view.findViewById(R.id.tvTotalPrice);

        // 💡 Adapter giờ nhận List<CartApiItemDetail>
        cartAdapter = new CartAdapter(requireContext(), R.layout.item_cart, cartList, this);
        lvCartItems.setAdapter(cartAdapter);

        updateTotalCost();

        return view;
    }

    @Override
    public void onResume() {
        super.onResume();

        // Gọi API tải giỏ hàng ngay khi Fragment hiển thị
        fetchCartFromServer();
    }

    // --- LOGIC GỌI API CART GET ---
    private void fetchCartFromServer() {

        if (currentUsername.equals("string")) {
            Log.w("CartFragment", "Chưa đăng nhập, sử dụng username mặc định 'string' hoặc giỏ hàng rỗng.");
            // Giả định 'string' là username tạm thời, nếu là guest thực sự, bạn nên clear list
            // cartList.clear();
            // updateUIAfterSync();
            // return;
        }

        ApiService apiService = RetrofitClient.getApiService();

        // Gọi API: https://localhost:7132/api/Cart/{username}
        apiService.getCartDetails(currentUsername).enqueue(new Callback<CartApiResponse>() {
            @Override
            public void onResponse(@NonNull Call<CartApiResponse> call, @NonNull Response<CartApiResponse> response) {
                if (response.isSuccessful() && response.body() != null && response.body().getItems() != null) {

                    List<CartApiItemDetail> serverCartItems = response.body().getItems();

                    // Cập nhật list Adapter
                    cartList.clear();
                    cartList.addAll(serverCartItems);

                    updateUIAfterSync();
                } else {
                    Log.e("CartFragment", "Lỗi server khi tải giỏ hàng: " + response.code() + ", Message: " + response.message());
                    // Xóa list nếu lỗi hoặc dữ liệu rỗng
                    cartList.clear();
                    updateUIAfterSync();
                }
            }

            @Override
            public void onFailure(@NonNull Call<CartApiResponse> call, @NonNull Throwable t) {
                Log.e("CartFragment", "Lỗi kết nối khi tải giỏ hàng: " + t.getMessage());
                cartList.clear(); // Xóa list nếu lỗi kết nối
                updateUIAfterSync();
            }
        });
    }

    private void updateUIAfterSync() {
        cartAdapter.notifyDataSetChanged();
        updateTotalCost();
        Log.d("CartFragment", "UI đã cập nhật. Số món: " + cartList.size());
    }

    // --- CÁC PHẦN KHÁC ---

    @Override
    public void onCartItemQuantityChanged() {
        // Phương thức này được gọi khi người dùng thay đổi số lượng qua Adapter
        Log.d("CartFragment", "Dữ liệu giỏ hàng cục bộ đã thay đổi, cập nhật UI.");
        updateTotalCost();

        // 🎯 CẦN LÀM: Gọi API PUT/POST để lưu thay đổi lên server
        // Ví dụ: sendUpdateCartToServer();
        // Sau khi server thành công, bạn có thể gọi lại fetchCartFromServer() để đồng bộ hoàn toàn.
    }

    private void updateTotalCost() {
        double total = 0;

        // Tính tổng tiền từ List<CartApiItemDetail>
        for (CartApiItemDetail item : cartList) {
            total += item.getPrice() * item.getQuantity();
        }

        if (tvTotalCost != null) {
            NumberFormat nf = NumberFormat.getInstance(new Locale("vi", "VN"));
            String formattedTotal = nf.format(total) + " VNĐ";
            tvTotalCost.setText(formattedTotal);
        }
    }
}