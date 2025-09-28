package com.example.cnpmnc_appfood;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.ImageView;
import android.widget.TextView;

import com.bumptech.glide.Glide;

import java.util.List;

public class CartAdapter extends ArrayAdapter<Dish> {
    private Context context;
    private List<Dish> cartItems;

    public CartAdapter(Context context, List<Dish> cartItems) {
        super(context, 0, cartItems);
        this.context = context;
        this.cartItems = cartItems;
    }

    @Override
    public View getView(int position, View convertView, ViewGroup parent) {
        Dish dish = cartItems.get(position);

        if (convertView == null) {
            convertView = LayoutInflater.from(context).inflate(R.layout.item_cart, parent, false);
        }

        ImageView ivCartImage = convertView.findViewById(R.id.ivCartImage);
        TextView tvCartName = convertView.findViewById(R.id.tvCartName);
        TextView tvCartPrice = convertView.findViewById(R.id.tvCartPrice);

        tvCartName.setText(dish.getName());
        tvCartPrice.setText(String.format("%,.0f VNĐ", dish.getPrice()));

        Glide.with(context)
                .load(dish.getImageUrl())
                .placeholder(R.drawable.ic_launcher_background)
                .into(ivCartImage);

        return convertView;
    }
}
