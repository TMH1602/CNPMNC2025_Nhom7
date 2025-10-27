package com.example.cnpmnc_appfood;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.ImageView;
import android.widget.TextView;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import com.bumptech.glide.Glide; // Cần dùng lại Glide
import java.util.List;

public class DishAdapter extends ArrayAdapter<Dish> {
    private Context context;
    private List<Dish> dishList;
    private OnDishClickListener dishClickListener;

    public interface OnDishClickListener {
        void onDishClick(Dish dish);
    }

    public DishAdapter(@NonNull Context context, List<Dish> dishList, OnDishClickListener listener) {
        super(context, 0, dishList);
        this.context = context;
        this.dishList = dishList;
        this.dishClickListener = listener;
    }

    @NonNull
    @Override
    public View getView(int position, @Nullable View convertView, @NonNull ViewGroup parent) {
        Dish dish = dishList.get(position);

        if (convertView == null) {
            convertView = LayoutInflater.from(context).inflate(R.layout.item_dish, parent, false);
        }

        ImageView ivDishImage = convertView.findViewById(R.id.ivDishImage);
        TextView tvDishName = convertView.findViewById(R.id.tvDishName);
        TextView tvDishDescription = convertView.findViewById(R.id.tvDishDescription);
        TextView tvDishPrice = convertView.findViewById(R.id.tvDishPrice);

        tvDishName.setText(dish.getName());
        tvDishDescription.setText(dish.getDescription());
        tvDishPrice.setText(String.format("%,.0f VNĐ", dish.getPrice()));

        // SỬA: Dùng Glide tải ảnh từ String URL/URI
        Glide.with(context)
                .load(dish.getImageUrl())
                .placeholder(R.drawable.ic_launcher_background)
                .into(ivDishImage);

        convertView.setOnClickListener(v -> {
            if (dishClickListener != null) {
                dishClickListener.onDishClick(dish);
            }
        });

        return convertView;
    }
}