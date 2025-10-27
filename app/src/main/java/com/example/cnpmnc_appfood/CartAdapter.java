package com.example.cnpmnc_appfood;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.ImageButton;
import android.widget.ImageView;
import android.widget.TextView;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import com.bumptech.glide.Glide;
import java.util.List;

// 🎯 KHẮC PHỤC LỖI: Thêm import CartManager 🎯
import com.example.cnpmnc_appfood.CartManager;


public class CartAdapter extends ArrayAdapter<CartItem> {

    private final Context context;
    private final int resource;
    private final List<CartItem> cartItems;
    private final CartItemChangeListener listener; // Biến để lưu Listener

    // 🎯 INTERFACE BỊ THIẾU 🎯
    public interface CartItemChangeListener {
        void onCartItemQuantityChanged();
        // void onCartItemRemoved();
    }

    /**
     * Constructor đã sửa: Thêm tham số CartItemChangeListener.
     */
    public CartAdapter(@NonNull Context context, int resource, List<CartItem> cartItems, CartItemChangeListener listener) {
        super(context, resource, cartItems);
        this.context = context;
        this.resource = resource;
        this.cartItems = cartItems;
        this.listener = listener; // Gán Listener
    }

    @NonNull
    @Override
    public View getView(int position, @Nullable View convertView, @NonNull ViewGroup parent) {

        if (convertView == null) {
            convertView = LayoutInflater.from(context).inflate(this.resource, parent, false);
        }

        CartItem item = getItem(position);
        if (item == null) {
            return convertView;
        }

        Dish dish = item.getDish();

        // 1. Ánh xạ các View
        ImageView ivDishImage = convertView.findViewById(R.id.ivCartDishImage);
        TextView tvDishName = convertView.findViewById(R.id.tvCartDishName);
        TextView tvDishPrice = convertView.findViewById(R.id.tvCartDishPrice);
        TextView tvQuantity = convertView.findViewById(R.id.tvCartQuantity);
        ImageButton btnIncrease = convertView.findViewById(R.id.btnCartIncrease);
        ImageButton btnDecrease = convertView.findViewById(R.id.btnCartDecrease);

        // 2. Gán dữ liệu
        tvDishName.setText(dish.getName());
        double totalPrice = dish.getPrice() * item.getQuantity();
        tvDishPrice.setText(String.format("%,.0f VNĐ", totalPrice));
        tvQuantity.setText(String.valueOf(item.getQuantity()));

        Glide.with(context)
                .load(dish.getImageUrl())
                .placeholder(R.drawable.ic_launcher_background)
                .into(ivDishImage);

        // 3. Xử lý sự kiện TĂNG số lượng
        btnIncrease.setOnClickListener(v -> {
            int newQuantity = item.getQuantity() + 1;

            // FIX LỖI: Gọi qua CartManager.getInstance()
            CartManager.getInstance().updateQuantity(dish, newQuantity);
            notifyDataSetChanged();

            // GỌI LISTENER: Thông báo cho CartFragment cập nhật tổng tiền
            listener.onCartItemQuantityChanged();
        });

        // 4. Xử lý sự kiện GIẢM số lượng
        btnDecrease.setOnClickListener(v -> {
            int newQuantity = item.getQuantity() - 1;

            // FIX LỖI: Gọi qua CartManager.getInstance()
            CartManager.getInstance().updateQuantity(dish, newQuantity);
            notifyDataSetChanged();

            // GỌI LISTENER: Thông báo cho CartFragment cập nhật tổng tiền
            listener.onCartItemQuantityChanged();
        });

        return convertView;
    }

    @Nullable
    @Override
    public CartItem getItem(int position) {
        // Trả về item từ danh sách nội bộ
        return cartItems.get(position);
    }
}