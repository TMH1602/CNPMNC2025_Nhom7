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

import com.bumptech.glide.Glide; // Import thư viện Glide

public class FoodDetailsFragment extends Fragment {

    private TextView tvFoodName, tvFoodPrice, tvFoodDescription;
    private ImageView ivFoodImage;
    // Thêm các nút chức năng
    private Button btnAddToCart, btnViewCart, btnAddToCompare, btnViewComparison;

    public FoodDetailsFragment() {
        // Required empty public constructor
    }

    /**
     * Phương thức tạo Fragment với ID của món ăn.
     */
    public static FoodDetailsFragment newInstance(int foodId) {
        FoodDetailsFragment fragment = new FoodDetailsFragment();
        Bundle args = new Bundle();
        args.putInt("foodId", foodId);
        fragment.setArguments(args);
        return fragment;
    }

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_food_details, container, false);

        // 1. Ánh xạ Views
        tvFoodName = view.findViewById(R.id.tvFoodName);
        tvFoodPrice = view.findViewById(R.id.tvFoodPrice);
        tvFoodDescription = view.findViewById(R.id.tvFoodDescription);
        ivFoodImage = view.findViewById(R.id.ivFoodImage);

        btnAddToCart = view.findViewById(R.id.btnAddToCart);
        btnViewCart = view.findViewById(R.id.btnViewCart);
        btnAddToCompare = view.findViewById(R.id.btnAddToCompare);
        btnViewComparison = view.findViewById(R.id.btnViewComparison);

        // 2. Lấy dữ liệu món ăn
        int foodId = getArguments() != null ? getArguments().getInt("foodId") : -1;
        Dish dish = DishRepository.getDishById(foodId);

        if (dish != null) {
            // 3. Đổ dữ liệu
            tvFoodName.setText(dish.getName());
            tvFoodPrice.setText(String.format("%,.0f VNĐ", dish.getPrice()));
            tvFoodDescription.setText(dish.getDescription());

            // 4. Tải ảnh bằng Glide (Sử dụng getImageUrl() trả về String)
            Glide.with(this)
                    .load(dish.getImageUrl())
                    .placeholder(R.drawable.ic_launcher_background) // Ảnh placeholder khi đang tải
                    .into(ivFoodImage);

            // 5. Xử lý sự kiện nút

            // Nút Thêm vào Giỏ hàng
            btnAddToCart.setOnClickListener(v -> {
                CartManager.addToCart(dish);
                Toast.makeText(getContext(), "Đã thêm vào giỏ!", Toast.LENGTH_SHORT).show();
            });

            // Nút Xem Giỏ hàng (chuyển sang CartFragment)
            btnViewCart.setOnClickListener(v -> {
                CartFragment cartFragment = new CartFragment();
                requireActivity().getSupportFragmentManager().beginTransaction()
                        .replace(R.id.fragment_container, cartFragment)
                        .addToBackStack(null)
                        .commit();
            });

            // Nút Thêm vào So sánh
            btnAddToCompare.setOnClickListener(v -> {
                boolean success = ComparisonManager.addToCompare(dish);
                if (success) {
                    Toast.makeText(getContext(), "Đã thêm '" + dish.getName() + "' vào danh sách so sánh!", Toast.LENGTH_SHORT).show();
                } else {
                    Toast.makeText(getContext(), "Danh sách so sánh đã đầy (Tối đa 2 món).", Toast.LENGTH_LONG).show();
                }
                updateCompareButtonVisibility();
            });

            // Nút Xem So sánh (chuyển sang ComparisonFragment)
            btnViewComparison.setOnClickListener(v -> {
                ComparisonFragment comparisonFragment = new ComparisonFragment();
                requireActivity().getSupportFragmentManager().beginTransaction()
                        .replace(R.id.fragment_container, comparisonFragment)
                        .addToBackStack(null)
                        .commit();
            });
        }

        // Cập nhật trạng thái nút Xem So sánh khi khởi tạo
        updateCompareButtonVisibility();

        return view;
    }

    // Hàm kiểm tra và cập nhật trạng thái hiển thị của nút "Xem so sánh"
    private void updateCompareButtonVisibility() {
        if (ComparisonManager.getComparisonListSize() > 0) {
            btnViewComparison.setVisibility(View.VISIBLE);
        } else {
            btnViewComparison.setVisibility(View.GONE);
        }
    }
}