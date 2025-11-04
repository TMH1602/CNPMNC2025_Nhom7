package com.example.cnpmnc_appfood;

import android.os.Bundle;
import androidx.fragment.app.Fragment;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;

public class FoodDetailsFragment extends Fragment {

    // Khai báo hằng số cho tham số (giả sử bạn truyền ID món ăn)
    private static final String ARG_FOOD_ID = "food_id";
    private int foodId;

    public FoodDetailsFragment() {
        // Constructor rỗng bắt buộc
    }

    /**
     * Phương thức khởi tạo tĩnh để truyền ID món ăn.
     */
    public static FoodDetailsFragment newInstance(int foodId) {
        FoodDetailsFragment fragment = new FoodDetailsFragment();
        Bundle args = new Bundle();
        args.putInt(ARG_FOOD_ID, foodId);
        fragment.setArguments(args);
        return fragment;
    }

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        if (getArguments() != null) {
            foodId = getArguments().getInt(ARG_FOOD_ID);
        }
    }

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        // Thay thế bằng layout chi tiết món ăn của bạn
        return inflater.inflate(R.layout.fragment_food_details, container, false);
    }
}