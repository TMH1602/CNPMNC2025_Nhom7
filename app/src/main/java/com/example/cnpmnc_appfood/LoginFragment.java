package com.example.cnpmnc_appfood;

import android.content.Intent;
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

    // SỬA ĐỔI 1: Đổi tên biến từ etEmail sang etUsername
    private EditText etUsername, etPassword;
    private Button btnLogin;
    private TextView tvToRegister;
    private TextView tvSkipLogin;

    private ApiService apiService;

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_login, container, false);

        // SỬA ĐỔI 2: Ánh xạ ID mới từ file XML (etLoginUsername)
        etUsername = view.findViewById(R.id.etLoginUsername);
        etPassword = view.findViewById(R.id.etLoginPassword);
        btnLogin = view.findViewById(R.id.btnLogin);
        tvToRegister = view.findViewById(R.id.tvToRegister);
        tvSkipLogin = view.findViewById(R.id.tvSkipLogin);

        // Khởi tạo ApiService (Giả sử bạn đã sửa RetrofitClientInstance thành RetrofitClient)
        apiService = RetrofitClient.getApiService();

        // Xử lý sự kiện Đăng nhập
        btnLogin.setOnClickListener(v -> {
            // SỬA ĐỔI 3: Lấy text từ etUsername
            String username = etUsername.getText().toString().trim();
            String password = etPassword.getText().toString();

            // SỬA ĐỔI 4: Kiểm tra username.isEmpty()
            if (username.isEmpty() || password.isEmpty()) {
                Toast.makeText(getContext(), "Vui lòng nhập đầy đủ thông tin.", Toast.LENGTH_SHORT).show();
                return;
            }

            btnLogin.setEnabled(false);

            // SỬA ĐỔI 5: Truyền 'username' vào hàm
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
            Intent intent = new Intent(getActivity(), MainActivity.class);
            startActivity(intent);
            if (getActivity() != null) {
                getActivity().finish();
            }
        });

        return view;
    }

    // SỬA ĐỔI 6: Đổi tên tham số từ 'email' sang 'username'
    private void loginUser(String username, String password) {

        // SỬA ĐỔI 7: Khởi tạo LoginRequest với username (giờ đã đúng)
        LoginRequest loginRequest = new LoginRequest(username, password);

        Call<String> call = apiService.login(loginRequest);

        call.enqueue(new Callback<String>() {
            @Override
            public void onResponse(Call<String> call, Response<String> response) {
                btnLogin.setEnabled(true);

                if (response.isSuccessful()) {
                    String responseString = response.body();
                    Toast.makeText(getContext(), "Đăng nhập thành công!", Toast.LENGTH_SHORT).show();

                    // TODO: Lưu token (responseString) vào SharedPreferences
                    // Ví dụ: saveToken(responseString);

                    Intent intent = new Intent(getActivity(), MainActivity.class);
                    startActivity(intent);
                    if (getActivity() != null) {
                        getActivity().finish();
                    }

                } else {
                    // SỬA ĐỔI 8: Sửa thông báo lỗi cho rõ ràng
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
}