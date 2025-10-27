package com.example.cnpmnc_appfood;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.ImageButton; // Import mới
import android.widget.ImageView;
import android.widget.TextView;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import com.bumptech.glide.Glide;
import java.util.List;

public class DishAdapter extends ArrayAdapter<Dish> {

    private final Context context;
    private final List<Dish> dishList;
    private final OnDishClickListener dishClickListener;
    private final OnCartClickListener cartClickListener; // Biến cho Listener mới
    private final int resource;

    public interface OnDishClickListener {
        void onDishClick(Dish dish);
    }

    // 🎯 INTERFACE MỚI CHO GIỎ HÀNG 🎯
    public interface OnCartClickListener {
        void onAddToCartClick(Dish dish);
    }

    /**
     * Constructor đã sửa: Chấp nhận 5 tham số.
     */
    public DishAdapter(@NonNull Context context, int resource, List<Dish> dishList,
                       OnDishClickListener dishClickListener, OnCartClickListener cartClickListener) {
        super(context, resource, dishList);

        this.context = context;
        this.resource = resource;
        this.dishList = dishList;
        this.dishClickListener = dishClickListener;
        this.cartClickListener = cartClickListener; // Gán Listener mới
    }

    @NonNull
    @Override
    public View getView(int position, @Nullable View convertView, @NonNull ViewGroup parent) {

        if (convertView == null) {
            convertView = LayoutInflater.from(context).inflate(this.resource, parent, false);
        }

        Dish dish = dishList.get(position);

        // Ánh xạ View
        ImageView ivDishImage = convertView.findViewById(R.id.ivDishImage);
        TextView tvDishName = convertView.findViewById(R.id.tvDishName);
        TextView tvDishDescription = convertView.findViewById(R.id.tvDishDescription);
        TextView tvDishPrice = convertView.findViewById(R.id.tvDishPrice);
        ImageButton btnAddToCart = convertView.findViewById(R.id.btnAddToCart); // Ánh xạ nút mới

        // Gán dữ liệu
        tvDishName.setText(dish.getName());
        tvDishDescription.setText(dish.getDescription());
        tvDishPrice.setText(String.format("%,.0f VNĐ", dish.getPrice()));

        // Tải ảnh bằng Glide
        Glide.with(context)
                .load(dish.getImageUrl())
                .placeholder(R.drawable.ic_launcher_background)
                .error(R.drawable.ic_launcher_background)
                .into(ivDishImage);

        // 🎯 XỬ LÝ CLICK NÚT THÊM VÀO GIỎ 🎯
        btnAddToCart.setOnClickListener(v -> {
            if (cartClickListener != null) {
                cartClickListener.onAddToCartClick(dish); // Gọi Listener mới
            }
        });

        // Xử lý click item (nếu click vào toàn bộ item)
        convertView.setOnClickListener(v -> {
            if (dishClickListener != null) {
                dishClickListener.onDishClick(dish);
            }
        });

        return convertView;
    }

    public void setDishList(List<Dish> newDishList) {
        this.dishList.clear();
        this.dishList.addAll(newDishList);
        notifyDataSetChanged();
    }
}