package com.example.cnpmnc_appfood;

import android.os.Bundle;

import androidx.fragment.app.Fragment;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.ImageView;
import android.widget.TextView;
import android.widget.Toast;

import androidx.annotation.NonNull; // Thêm
import androidx.annotation.Nullable; // Thêm

import com.bumptech.glide.Glide;

public class FoodDetailsFragment extends Fragment {

    private TextView tvFoodName, tvFoodPrice, tvFoodDescription;
    private ImageView ivFoodImage;
    private Button btnAddToCart, btnViewCart, btnAddToCompare, btnViewComparison;

    // SỬA 1: Thêm một biến để giữ Repository
    private DishRepository dishRepository;

    public FoodDetailsFragment() {
        // Required empty public constructor
    }

    public static FoodDetailsFragment newInstance(int foodId) {
        FoodDetailsFragment fragment = new FoodDetailsFragment();
        Bundle args = new Bundle();
        args.putInt("foodId", foodId);
        fragment.setArguments(args);
        return fragment;
    }

    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container,
                             @Nullable Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_food_details, container, false);

        // 1. Ánh xạ Views (Giữ nguyên)
        tvFoodName = view.findViewById(R.id.tvFoodName);
        // ... (các ánh xạ khác giữ nguyên) ...
        btnViewComparison = view.findViewById(R.id.btnViewComparison);

        // SỬA 2: Lấy instance của Repository (giống như HomeFragment)
        dishRepository = DishRepository.getInstance();

        // 2. Lấy dữ liệu món ăn
        int foodId = getArguments() != null ? getArguments().getInt("foodId") : -1;

        // SỬA 3: Gọi hàm 'getDishById' từ instance, không còn 'static'
        Dish dish = dishRepository.getDishById(foodId);

        if (dish != null) {
            // 3. Đổ dữ liệu (Giữ nguyên)
            tvFoodName.setText(dish.getName());
            tvFoodPrice.setText(String.format("%,.0f VNĐ", dish.getPrice()));
            tvFoodDescription.setText(dish.getDescription());

            // 4. Tải ảnh bằng Glide (Giữ nguyên)
            Glide.with(this)
                    .load(dish.getImageUrl())
                    .placeholder(R.drawable.ic_launcher_background)
                    .into(ivFoodImage);

            // 5. Xử lý sự kiện nút (Toàn bộ phần này giữ nguyên)

            // Nút Thêm vào Giỏ hàng
            btnAddToCart.setOnClickListener(v -> {
                // (Giả sử bạn có CartManager)
                // CartManager.addToCart(dish);
                Toast.makeText(getContext(), "Đã thêm vào giỏ!", Toast.LENGTH_SHORT).show();
            });

            // Nút Xem Giỏ hàng
            btnViewCart.setOnClickListener(v -> {
                // (Giả sử bạn có CartFragment)
                // CartFragment cartFragment = new CartFragment();
                // ...
            });

            // Nút Thêm vào So sánh
            btnAddToCompare.setOnClickListener(v -> {
                // (Giả sử bạn có ComparisonManager)
                // boolean success = ComparisonManager.addToCompare(dish);
                // ...
                updateCompareButtonVisibility();
            });

            // Nút Xem So sánh
            btnViewComparison.setOnClickListener(v -> {
                // (Giả sử bạn có ComparisonFragment)
                // ComparisonFragment comparisonFragment = new ComparisonFragment();
                // ...
            });
        }

        updateCompareButtonVisibility();

        return view;
    }

    // Hàm kiểm tra (Giữ nguyên)
    private void updateCompareButtonVisibility() {
        // (Giả sử bạn có ComparisonManager)
        // if (ComparisonManager.getComparisonListSize() > 0) {
        //     btnViewComparison.setVisibility(View.VISIBLE);
        // } else {
        //     btnViewComparison.setVisibility(View.GONE);
        // }
    }
}