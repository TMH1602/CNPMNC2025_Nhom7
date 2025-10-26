package com.example.cnpmnc_appfood;

import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.ListView;
import android.widget.TextView;
import android.widget.Toast;

import androidx.fragment.app.Fragment;

public class CartFragment extends Fragment implements CartAdapter.CartItemChangeListener {

    private ListView listCart;
    private TextView tvTotalPrice;
    private Button btnCheckout;
    private CartAdapter adapter;

    public CartFragment() {}

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        // Sử dụng fragment_cart.xml
        View view = inflater.inflate(R.layout.fragment_cart, container, false);

        listCart = view.findViewById(R.id.listCart);
        tvTotalPrice = view.findViewById(R.id.tvTotalPrice);
        btnCheckout = view.findViewById(R.id.btnCheckout);

        adapter = new CartAdapter(requireContext(), CartManager.getCartList(), this);
        listCart.setAdapter(adapter);

        updateCartUI();

        // Xử lý sự kiện Thanh Toán
        btnCheckout.setOnClickListener(v -> {
            if (CartManager.getTotalPrice() > 0) {
                Toast.makeText(getContext(), "Thanh toán thành công! Tổng tiền: " + String.format("%,.0f VNĐ", CartManager.getTotalPrice()), Toast.LENGTH_LONG).show();
                CartManager.clearCart(); // Xóa giỏ hàng sau khi thanh toán
                updateCartUI();
            } else {
                Toast.makeText(getContext(), "Giỏ hàng trống. Vui lòng thêm món.", Toast.LENGTH_SHORT).show();
            }
        });

        return view;
    }

    // Phương thức bắt sự kiện thay đổi từ Adapter
    @Override
    public void onQuantityChange() {
        updateCartUI();
    }

    // Hàm cập nhật giao diện (cập nhật danh sách và tổng tiền)
    private void updateCartUI() {
        adapter.clear();
        adapter.addAll(CartManager.getCartList());
        adapter.notifyDataSetChanged();

        double total = CartManager.getTotalPrice();
        tvTotalPrice.setText(String.format("Tổng: %,.0f VNĐ", total));
    }
}