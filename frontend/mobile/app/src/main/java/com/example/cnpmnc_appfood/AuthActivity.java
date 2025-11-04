package com.example.cnpmnc_appfood;

import android.content.Intent; // 🎯 CẦN THÊM IMPORT NÀY 🎯
import android.os.Bundle;
import androidx.appcompat.app.AppCompatActivity;
import androidx.fragment.app.FragmentManager;
import androidx.fragment.app.FragmentTransaction;
import android.util.Log;

public class AuthActivity extends AppCompatActivity {

    // Khai báo ID của Fragment Container (giữ nguyên)
    private static final int FRAGMENT_CONTAINER_ID = R.id.auth_fragment_container;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_auth);

        if (savedInstanceState == null) {
            // Tải LoginFragment mặc định khi Activity được tạo
            getSupportFragmentManager().beginTransaction()
                    .replace(FRAGMENT_CONTAINER_ID, new LoginFragment())
                    .commit();
        }
    }

    // Phương thức công khai để chuyển sang Register Fragment (giữ nguyên)
    public void navigateToRegister() {
        getSupportFragmentManager().beginTransaction()
                .replace(FRAGMENT_CONTAINER_ID, new RegisterFragment())
                .addToBackStack("login")
                .commit();
    }

    // Phương thức công khai để chuyển sang Login Fragment (giữ nguyên)
    public void navigateToLogin() {
        getSupportFragmentManager().beginTransaction()
                .replace(FRAGMENT_CONTAINER_ID, new LoginFragment())
                .commit();
    }

    // 🎯 PHƯƠNG THỨC ĐÃ SỬA: CHUYỂN HOÀN TOÀN SANG MAINACTIVITY 🎯
    /**
     * Phương thức này được gọi từ LoginFragment khi API Login thành công.
     * Nó khởi động MainActivity và đóng AuthActivity.
     */
    public void onLoginSuccess() {
        Log.d("AuthActivity", "Đăng nhập thành công, chuyển sang MainActivity (HomeFragment).");

        // 1. Tạo Intent để khởi động MainActivity
        Intent intent = new Intent(this, MainActivity.class);

        // 2. Thiết lập cờ để xóa Activity hiện tại khỏi stack
        // Điều này ngăn người dùng nhấn Back để quay lại màn hình đăng nhập
        intent.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TASK);

        // 3. Khởi chạy Activity
        startActivity(intent);

        // 4. Đóng AuthActivity
        finish();
    }
}