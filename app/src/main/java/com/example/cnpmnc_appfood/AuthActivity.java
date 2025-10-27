package com.example.cnpmnc_appfood;

import android.os.Bundle;
import androidx.appcompat.app.AppCompatActivity;

public class AuthActivity extends AppCompatActivity {

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_auth);

        if (savedInstanceState == null) {
            // Tải LoginFragment mặc định khi Activity được tạo
            getSupportFragmentManager().beginTransaction()
                    .replace(R.id.auth_fragment_container, new LoginFragment())
                    .commit();
        }
    }

    // Phương thức công khai để các Fragment có thể chuyển đổi giữa nhau
    public void navigateToRegister() {
        getSupportFragmentManager().beginTransaction()
                .replace(R.id.auth_fragment_container, new RegisterFragment())
                .addToBackStack("login") // Cho phép quay lại màn hình đăng nhập
                .commit();
    }

    public void navigateToLogin() {
        getSupportFragmentManager().beginTransaction()
                .replace(R.id.auth_fragment_container, new LoginFragment())
                .commit();
    }
}