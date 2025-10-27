package com.example.cnpmnc_appfood;

import android.content.Context; // 🎯 IMPORT MỚI 🎯
import android.content.Intent;
import android.content.SharedPreferences; // 🎯 IMPORT MỚI 🎯
import android.os.Bundle;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.EditText;
import android.widget.TextView;
import android.widget.Toast;

import androidx.fragment.app.Fragment;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class LoginFragment extends Fragment {

    private EditText etUsername, etPassword;
    private Button btnLogin;
    private TextView tvToRegister;
    private TextView tvSkipLogin;

    private ApiService apiService;

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_login, container, false);

        // Ánh xạ View
        etUsername = view.findViewById(R.id.etLoginUsername);
        etPassword = view.findViewById(R.id.etLoginPassword);
        btnLogin = view.findViewById(R.id.btnLogin);
        tvToRegister = view.findViewById(R.id.tvToRegister);
        tvSkipLogin = view.findViewById(R.id.tvSkipLogin);

        apiService = RetrofitClient.getApiService();

        // Xử lý sự kiện Đăng nhập
        btnLogin.setOnClickListener(v -> {
            String username = etUsername.getText().toString().trim();
            String password = etPassword.getText().toString();

            if (username.isEmpty() || password.isEmpty()) {
                Toast.makeText(getContext(), "Vui lòng nhập đầy đủ thông tin.", Toast.LENGTH_SHORT).show();
                return;
            }

            btnLogin.setEnabled(false);
            loginUser(username, password);
        });

        // Xử lý sự kiện chuyển sang màn hình Đăng ký
        tvToRegister.setOnClickListener(v -> {
            if (getActivity() instanceof AuthActivity) {
                ((AuthActivity) getActivity()).navigateToRegister();
            }
        });

        // Xử lý sự kiện BỎ QUA ĐĂNG NHẬP
        tvSkipLogin.setOnClickListener(v -> {
            if (getActivity() instanceof AuthActivity) {
                // Tùy chọn: Lưu tên người dùng giả/Khách (nếu cần)
                // saveUserData("GuestUser", null);
                ((AuthActivity) getActivity()).onLoginSuccess();
            } else {
                Intent intent = new Intent(getActivity(), MainActivity.class);
                startActivity(intent);
                if (getActivity() != null) {
                    getActivity().finish();
                }
            }
        });

        return view;
    }

    private void loginUser(String username, String password) {

        LoginRequest loginRequest = new LoginRequest(username, password);
        Call<String> call = apiService.login(loginRequest);

        call.enqueue(new Callback<String>() {
            @Override
            public void onResponse(Call<String> call, Response<String> response) {
                btnLogin.setEnabled(true);

                if (response.isSuccessful()) {
                    String authToken = response.body(); // Giả sử body là chuỗi token

                    // 🎯 LƯU USERNAME VÀ TOKEN VÀO SHAREDPREFERENCES 🎯
                    if (getActivity() != null) {
                        saveUserData(username, authToken);
                    }

                    Toast.makeText(getContext(), "Đăng nhập thành công!", Toast.LENGTH_SHORT).show();

                    // Chuyển đổi Fragment
                    if (getActivity() instanceof AuthActivity) {
                        ((AuthActivity) getActivity()).onLoginSuccess();
                    } else {
                        Intent intent = new Intent(getActivity(), MainActivity.class);
                        startActivity(intent);
                        if (getActivity() != null) {
                            getActivity().finish();
                        }
                    }

                } else {
                    Toast.makeText(getContext(), "Tên tài khoản hoặc mật khẩu không đúng.", Toast.LENGTH_SHORT).show();
                }
            }

            @Override
            public void onFailure(Call<String> call, Throwable t) {
                btnLogin.setEnabled(true);
                Log.e("LoginError", "API Call Failed: " + t.getMessage());
                Toast.makeText(getContext(), "Lỗi kết nối. Vui lòng thử lại.", Toast.LENGTH_SHORT).show();
            }
        });
    }

    /**
     * Phương thức lưu trữ tên người dùng và token vào SharedPreferences.
     */
    private void saveUserData(String username, String token) {
        // Lấy SharedPreferences object (tên file là "UserPrefs")
        SharedPreferences prefs = requireActivity().getSharedPreferences("UserPrefs", Context.MODE_PRIVATE);

        SharedPreferences.Editor editor = prefs.edit();

        // Lưu dữ liệu
        editor.putString("USERNAME", username);
        editor.putString("AUTH_TOKEN", token);

        // Áp dụng thay đổi
        editor.apply();
    }
}