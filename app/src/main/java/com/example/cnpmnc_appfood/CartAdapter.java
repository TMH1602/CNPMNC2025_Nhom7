package com.example.cnpmnc_appfood;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.ImageView;
import android.widget.TextView;
// 🎯 BỎ CHÚ THÍCH DÒNG IMPORT GLIDE 🎯
import com.bumptech.glide.Glide;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import java.text.NumberFormat;
import java.util.List;
import java.util.Locale;

// 💡 Adapter nhận List<CartApiItemDetail>
public class CartAdapter extends ArrayAdapter<CartApiItemDetail> {

    private final Context context;
    private final int resource;
    private final CartItemChangeListener listener;
    private final NumberFormat nf = NumberFormat.getInstance(new Locale("vi", "VN"));

    public interface CartItemChangeListener {
        void onCartItemQuantityChanged();
    }

    // 💡 Constructor nhận List<CartApiItemDetail>
    public CartAdapter(@NonNull Context context, int resource, @NonNull List<CartApiItemDetail> objects, CartItemChangeListener listener) {
        super(context, resource, objects);
        this.context = context;
        this.resource = resource;
        this.listener = listener;
    }

    @NonNull
    @Override
    public View getView(int position, @Nullable View convertView, @NonNull ViewGroup parent) {
        if (convertView == null) {
            convertView = LayoutInflater.from(context).inflate(resource, parent, false);
        }

        // 💡 Lấy CartApiItemDetail
        final CartApiItemDetail cartItem = getItem(position);

        if (cartItem != null) {
            TextView tvDishName = convertView.findViewById(R.id.tvDishName);
            TextView tvPrice = convertView.findViewById(R.id.tvPrice);
            TextView tvQuantity = convertView.findViewById(R.id.tvQuantity);
            Button btnDecrease = convertView.findViewById(R.id.btnDecrease);
            Button btnIncrease = convertView.findViewById(R.id.btnIncrease);
            ImageView ivDishImage = convertView.findViewById(R.id.ivDishImage); // Lấy ImageView

            // Set Data từ CartApiItemDetail
            tvDishName.setText(cartItem.getProductName());

            double pricePerItem = cartItem.getPrice();
            String formattedPrice = nf.format(pricePerItem) + " VNĐ";
            tvPrice.setText(formattedPrice);

            tvQuantity.setText(String.valueOf(cartItem.getQuantity()));

            // 🎯 LOGIC LẤY ẢNH TỪ API URL VÀ GÁN VÀO ImageView 🎯
            String imageUrl = cartItem.getImageUrl();
            if (imageUrl != null && !imageUrl.isEmpty()) {
                Glide.with(context)
                        .load(imageUrl) // Sử dụng URL từ dữ liệu API
                        .placeholder(R.drawable.pizza_hai_san) // Sử dụng ảnh placeholder bạn đã có
                        .error(R.drawable.pizza_hai_san) // Giả định có ảnh error_image
                        .into(ivDishImage);
            } else {
                // Nếu URL rỗng/null, hiển thị ảnh mặc định
                ivDishImage.setImageResource(R.drawable.pizza_hai_san);
            }

            // ⚠️ LƯU Ý: Logic tăng giảm số lượng dưới đây chỉ cập nhật cục bộ
            // mà không gọi API. Sau khi cập nhật, bạn phải gọi API update cart.

            btnIncrease.setOnClickListener(v -> {
                // Tăng quantity trong đối tượng cục bộ (KHÔNG phải API)
                cartItem.setQuantity(cartItem.getQuantity() + 1);
                notifyDataSetChanged();
                listener.onCartItemQuantityChanged();
            });

            btnDecrease.setOnClickListener(v -> {
                int newQuantity = cartItem.getQuantity() - 1;
                if (newQuantity > 0) {
                    cartItem.setQuantity(newQuantity);
                } else {
                    // Logic xóa món ăn nếu số lượng = 0
                    remove(cartItem);
                }
                notifyDataSetChanged();
                listener.onCartItemQuantityChanged();
            });
        }

        return convertView;
    }
}