package com.example.cnpmnc_appfood;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.ImageButton;
import android.widget.ImageView;
import android.widget.TextView;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
// Loại bỏ import Glide
import com.bumptech.glide.Glide;

import java.util.List;
import java.util.Map;

public class CartAdapter extends ArrayAdapter<Map.Entry<Dish, Integer>> {
    private Context context;
    private List<Map.Entry<Dish, Integer>> cartItems;
    private CartItemChangeListener listener;

    // Interface để thông báo cho Fragment khi có thay đổi (số lượng/xóa)
    public interface CartItemChangeListener {
        void onQuantityChange();
    }

    public CartAdapter(@NonNull Context context, List<Map.Entry<Dish, Integer>> cartItems, CartItemChangeListener listener) {
        super(context, 0, cartItems);
        this.context = context;
        this.cartItems = cartItems;
        this.listener = listener;
    }

    @NonNull
    @Override
    public View getView(int position, @Nullable View convertView, @NonNull ViewGroup parent) {
        Map.Entry<Dish, Integer> entry = cartItems.get(position);
        Dish dish = entry.getKey();
        int quantity = entry.getValue();

        if (convertView == null) {
            // Sử dụng item_cart.xml
            convertView = LayoutInflater.from(context).inflate(R.layout.item_cart, parent, false);
        }

        // Ánh xạ Views
        ImageView ivCartImage = convertView.findViewById(R.id.ivCartImage);
        TextView tvCartName = convertView.findViewById(R.id.tvCartName);
        TextView tvCartPrice = convertView.findViewById(R.id.tvCartPrice);
        TextView tvQuantity = convertView.findViewById(R.id.tvQuantity);
        Button btnIncrease = convertView.findViewById(R.id.btnIncrease);
        Button btnDecrease = convertView.findViewById(R.id.btnDecrease);
        ImageButton btnRemove = convertView.findViewById(R.id.btnRemoveItem);

        // Đổ dữ liệu
        tvCartName.setText(dish.getName());
        tvCartPrice.setText(String.format("%,.0f VNĐ", dish.getPrice()));
        tvQuantity.setText(String.valueOf(quantity));

        // THAY THẾ GLIDE: Dùng Resource ID
        ivCartImage = convertView.findViewById(R.id.ivCartImage);
        Glide.with(context)
                .load(dish.getImageUrl())
                .placeholder(R.drawable.ic_launcher_background)
                .into(ivCartImage);

        // Xử lý sự kiện Tăng/Giảm/Xóa
        btnIncrease.setOnClickListener(v -> {
            CartManager.updateQuantity(dish, quantity + 1);
            listener.onQuantityChange();
        });

        btnDecrease.setOnClickListener(v -> {
            CartManager.updateQuantity(dish, quantity - 1);
            listener.onQuantityChange();
        });

        btnRemove.setOnClickListener(v -> {
            CartManager.removeFromCart(dish);
            listener.onQuantityChange();
        });

        return convertView;
    }
}