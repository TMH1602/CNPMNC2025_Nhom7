package com.example.cnpmnc_appfood;

import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.TextView;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;

// Loại bỏ import Glide

import com.bumptech.glide.Glide;

import java.util.List;

public class ComparisonFragment extends Fragment {

    // Views cho món 1
    private ImageView ivDish1Image;
    private TextView tvDish1Name, tvDish1Price, tvDish1Description;

    // Views cho món 2
    private ImageView ivDish2Image;
    private TextView tvDish2Name, tvDish2Price, tvDish2Description;

    private TextView tvNotEnoughItems;
    private LinearLayout comparisonContent;
    private Button btnClearComparison;


    public ComparisonFragment() {
        // Required empty public constructor
    }

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_comparison, container, false);

        // Ánh xạ các views chung
        tvNotEnoughItems = view.findViewById(R.id.tvNotEnoughItems);
        comparisonContent = view.findViewById(R.id.comparisonContent);
        btnClearComparison = view.findViewById(R.id.btnClearComparison);

        // Ánh xạ views món 1
        ivDish1Image = view.findViewById(R.id.ivDish1Image);
        tvDish1Name = view.findViewById(R.id.tvDish1Name);
        tvDish1Price = view.findViewById(R.id.tvDish1Price);
        tvDish1Description = view.findViewById(R.id.tvDish1Description);

        // Ánh xạ views món 2
        ivDish2Image = view.findViewById(R.id.ivDish2Image);
        tvDish2Name = view.findViewById(R.id.tvDish2Name);
        tvDish2Price = view.findViewById(R.id.tvDish2Price);
        tvDish2Description = view.findViewById(R.id.tvDish2Description);

        btnClearComparison.setOnClickListener(v -> {
            ComparisonManager.clearComparison();
            Toast.makeText(getContext(), "Đã xóa danh sách so sánh.", Toast.LENGTH_SHORT).show();
            // Quay về màn hình trước đó
            requireActivity().getSupportFragmentManager().popBackStack();
        });

        return view;
    }

    @Override
    public void onViewCreated(@NonNull View view, @Nullable Bundle savedInstanceState) {
        super.onViewCreated(view, savedInstanceState);
        loadComparisonData(); // Gọi hàm xử lý logic ở đây
    }


    private void loadComparisonData() {
        List<Dish> comparisonList = ComparisonManager.getComparisonList();
        View fragmentView = getView();
        if (fragmentView == null) return;

        if (comparisonList.size() < 2) {
            tvNotEnoughItems.setVisibility(View.VISIBLE);
            comparisonContent.setVisibility(View.GONE);
            if (comparisonList.size() == 1) {
                tvNotEnoughItems.setText("Vui lòng chọn thêm 1 món nữa để so sánh.");
                comparisonContent.setVisibility(View.VISIBLE);
                fragmentView.findViewById(R.id.dish2Layout).setVisibility(View.INVISIBLE);
                populateDishData(comparisonList.get(0), ivDish1Image, tvDish1Name, tvDish1Price, tvDish1Description);
            }
        } else {
            tvNotEnoughItems.setVisibility(View.GONE);
            comparisonContent.setVisibility(View.VISIBLE);
            fragmentView.findViewById(R.id.dish2Layout).setVisibility(View.VISIBLE);

            Dish dish1 = comparisonList.get(0);
            Dish dish2 = comparisonList.get(1);

            populateDishData(dish1, ivDish1Image, tvDish1Name, tvDish1Price, tvDish1Description);
            populateDishData(dish2, ivDish2Image, tvDish2Name, tvDish2Price, tvDish2Description);

            // Xử lý tô màu giá thấp hơn
            if (dish1.getPrice() < dish2.getPrice()) {
                tvDish1Price.setBackgroundColor(getResources().getColor(android.R.color.holo_green_light));
                tvDish2Price.setBackgroundColor(getResources().getColor(android.R.color.transparent));
            } else if (dish2.getPrice() < dish1.getPrice()) {
                tvDish2Price.setBackgroundColor(getResources().getColor(android.R.color.holo_green_light));
                tvDish1Price.setBackgroundColor(getResources().getColor(android.R.color.transparent));
            } else {
                tvDish1Price.setBackgroundColor(getResources().getColor(android.R.color.transparent));
                tvDish2Price.setBackgroundColor(getResources().getColor(android.R.color.transparent));
            }
        }
    }

    private void populateDishData(Dish dish, ImageView image, TextView name, TextView price, TextView description) {
        name.setText(dish.getName());
        price.setText(String.format("%,.0f VNĐ", dish.getPrice()));
        description.setText(dish.getDescription());

        // THAY THẾ GLIDE: Dùng Resource ID
        Glide.with(this)
                .load(dish.getImageUrl()) // ĐÃ SỬA CHƯA?
                .placeholder(R.drawable.ic_launcher_background)
                .into(image);
        price.setBackgroundColor(getResources().getColor(android.R.color.transparent)); // Reset màu nền
    }
}