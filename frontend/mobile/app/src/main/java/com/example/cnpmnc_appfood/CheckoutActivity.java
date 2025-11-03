package com.example.cnpmnc_appfood;

import android.content.Intent;
import android.net.Uri;
import android.os.Bundle;
import android.util.Log;
import android.view.View; // THÊM import này
import android.widget.Button;
import android.widget.TextView;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.appcompat.app.AppCompatActivity;

import java.text.NumberFormat;
import java.util.Locale;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class CheckoutActivity extends AppCompatActivity {

    private TextView tvOrderId, tvOrderDate, tvTotalAmount, tvMessage;
    private Button btnPayVnPay, btnContinueShopping;

    private ApiService apiService;
    private int currentOrderId = -1; // Lưu OrderId để dùng cho thanh toán

    @Override
    protected void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_checkout);

        // Khởi tạo ApiService
        apiService = RetrofitClient.getApiService();

        // Ánh xạ View
        tvOrderId = findViewById(R.id.tvOrderId);
        tvOrderDate = findViewById(R.id.tvOrderDate);
        tvTotalAmount = findViewById(R.id.tvTotalAmount);
        tvMessage = findViewById(R.id.tvMessage);
        btnPayVnPay = findViewById(R.id.btnPayVnPay);
        btnContinueShopping = findViewById(R.id.btnContinueShopping);

        // Lấy dữ liệu từ Intent (được gửi từ CartFragment)
        Intent intent = getIntent();
        if (intent != null && intent.hasExtra("CHECKOUT_DATA")) {
            CheckoutApiResponse checkoutData = (CheckoutApiResponse) intent.getSerializableExtra("CHECKOUT_DATA");

            if (checkoutData != null) {
                // Lưu OrderId lại
                currentOrderId = checkoutData.getOrderId();

                // Hiển thị dữ liệu
                tvOrderId.setText(String.valueOf(checkoutData.getOrderId()));
                tvOrderDate.setText(checkoutData.getOrderDate()); // Cần format ngày nếu muốn
                tvMessage.setText(checkoutData.getMessage());

                // Format tiền tệ
                NumberFormat nf = NumberFormat.getInstance(new Locale("vi", "VN"));
                String formattedTotal = nf.format(checkoutData.getTotalAmount()) + " VNĐ";
                tvTotalAmount.setText(formattedTotal);
            }
        }

        // --- Gán sự kiện Click ---

        // 1. Nút "Tiếp tục mua sắm" -> Chỉ cần đóng Activity này lại
        btnContinueShopping.setOnClickListener(v -> {
            finish(); // Đóng trang xác nhận và quay lại giỏ hàng
        });

        // 2. Nút "Thanh toán VNPay"
        btnPayVnPay.setOnClickListener(v -> {
            if (currentOrderId > 0) {
                // Bây giờ mới gọi API tạo link VnPay
                createVnPayPaymentLink(currentOrderId);
            } else {
                Toast.makeText(this, "Lỗi: Không tìm thấy Mã đơn hàng.", Toast.LENGTH_SHORT).show();
            }
        });

        // --- (MỚI) XỬ LÝ DEEP LINK KHI APP MỚI MỞ ---
        // (Kiểm tra xem app có được mở từ link cnpmapp:// không)
        handleDeepLinkIntent(intent);
    }

    /**
     * (MỚI) Xử lý khi app được mở lại từ Deep Link (khi đang chạy)
     * Vì chúng ta dùng launchMode="singleTop"
     */
    @Override
    protected void onNewIntent(Intent intent) {
        super.onNewIntent(intent);
        setIntent(intent); // Cập nhật intent mới
        handleDeepLinkIntent(intent);
    }

    /**
     * (MỚI) Hàm trung tâm xử lý logic Deep Link
     */
    private void handleDeepLinkIntent(Intent intent) {
        if (intent == null || intent.getAction() == null || !intent.getAction().equals(Intent.ACTION_VIEW)) {
            // Đây không phải là deep link, bỏ qua
            return;
        }

        Uri data = intent.getData();
        // Kiểm tra đúng scheme và host đã đăng ký trong Manifest
        if (data != null && "cnpmapp".equals(data.getScheme()) && "checkout".equals(data.getHost())) {

            // URL trả về từ C# là: cnpmapp://checkout/success?orderId=15
            // hoặc:   cnpmapp://checkout/fail?orderId=15

            String statusPath = data.getPath(); // Sẽ là "/success" hoặc "/fail"
            String orderId = data.getQueryParameter("orderId");

            if (statusPath != null && statusPath.contains("success")) {
                // THANH TOÁN THÀNH CÔNG
                Toast.makeText(this, "Thanh toán thành công cho đơn hàng: " + orderId, Toast.LENGTH_LONG).show();

                // Cập nhật UI
                btnPayVnPay.setVisibility(View.GONE); // Ẩn nút thanh toán
                btnContinueShopping.setText("Hoàn tất"); // Đổi text nút
                tvMessage.setText("ĐÃ THANH TOÁN THÀNH CÔNG!");
                tvMessage.setTextColor(getResources().getColor(android.R.color.holo_green_dark));

            } else if (statusPath != null && statusPath.contains("fail")) {
                // THANH TOÁN THẤT BẠI
                Toast.makeText(this, "Thanh toán thất bại cho đơn hàng: " + orderId, Toast.LENGTH_LONG).show();
                tvMessage.setText("THANH TOÁN THẤT BẠI!");
                tvMessage.setTextColor(getResources().getColor(android.R.color.holo_red_dark));
                // (Nút thanh toán vẫn hiển thị để người dùng thử lại)
            }
        }
    }


    /**
     * Gọi API để tạo link thanh toán VnPay.
     * (ĐÃ SỬA: Thêm "mobile" làm source)
     */
    private void createVnPayPaymentLink(int orderId) {
        Toast.makeText(this, "Đang tạo liên kết thanh toán...", Toast.LENGTH_SHORT).show();

        // ---- THAY ĐỔI QUAN TRỌNG: Thêm "mobile" ----
        apiService.createVnPayPayment(orderId, "mobile").enqueue(new Callback<VnPayCreatePaymentResponse>() {
            @Override
            public void onResponse(@NonNull Call<VnPayCreatePaymentResponse> call, @NonNull Response<VnPayCreatePaymentResponse> response) {
                if (response.isSuccessful() && response.body() != null && response.body().getPaymentUrl() != null) {
                    String paymentUrl = response.body().getPaymentUrl();
                    openPaymentUrlInBrowser(paymentUrl);
                } else {
                    Log.e("CheckoutActivity", "Lỗi tạo link VnPay: " + response.code() + " - " + response.message());
                    Toast.makeText(CheckoutActivity.this, "Lỗi tạo liên kết thanh toán.", Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(@NonNull Call<VnPayCreatePaymentResponse> call, @NonNull Throwable t) {
                Log.e("CheckoutActivity", "Lỗi kết nối khi tạo link VnPay: " + t.getMessage());
                Toast.makeText(CheckoutActivity.this, "Lỗi kết nối mạng.", Toast.LENGTH_SHORT).show();
            }
        });
    }

    /**
     * Mở URL trong trình duyệt mặc định của thiết bị.
     */
    private void openPaymentUrlInBrowser(String url) {
        Intent browserIntent = new Intent(Intent.ACTION_VIEW, Uri.parse(url));
        try {
            startActivity(browserIntent);
        } catch (Exception e) {
            Log.e("CheckoutActivity", "Không thể mở trình duyệt: " + e.getMessage());
            Toast.makeText(this, "Không thể mở trình duyệt để thanh toán.", Toast.LENGTH_SHORT).show();
        }
    }
}

