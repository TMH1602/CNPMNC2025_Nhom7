package com.example.cnpmnc_appfood;

import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ListView;
import android.widget.TextView;

import androidx.fragment.app.Fragment;

import java.util.List;

public class CartFragment extends Fragment {

    private ListView listCart;
    private TextView tvTotalPrice;

    public CartFragment() {}

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_cart, container, false);

        listCart = view.findViewById(R.id.listCart);
        tvTotalPrice = view.findViewById(R.id.tvTotalPrice);

        List<Dish> cartItems = CartManager.getCart();
        CartAdapter adapter = new CartAdapter(requireContext(), cartItems);
        listCart.setAdapter(adapter);

        // Tính tổng tiền
        double total = 0;
        for (Dish d : cartItems) {
            total += d.getPrice();
        }
        tvTotalPrice.setText(String.format("Tổng: %,.0f VNĐ", total));

        return view;
    }
}
