package com.example.cnpmnc_appfood;

import android.os.Bundle;
import android.util.Log;
import android.util.Patterns;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.EditText;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.core.graphics.Insets;
import androidx.core.view.ViewCompat;
import androidx.core.view.WindowInsetsCompat;
import androidx.fragment.app.Fragment;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class RegisterFragment extends Fragment {

    // Đã có edAddress, chính xác
    EditText edEmail, edSignInUsername, edSignInPassword, edConfirmPassword, edAddress;
    Button btnSignIn, btnBack;

    private ApiService apiService;

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_register, container, false);
        return view;
    }

    @Override
    public void onViewCreated(@NonNull View view, @Nullable Bundle savedInstanceState) {
        super.onViewCreated(view, savedInstanceState);

        // ... (bỏ qua Edge-to-Edge) ...

        // Tìm views (đã có edAddress, chính xác)
        edEmail = view.findViewById(R.id.edEmail);
        edSignInUsername = view.findViewById(R.id.edSignUpUsername);
        edSignInPassword = view.findViewById(R.id.edSignUpPassword);
        edConfirmPassword = view.findViewById(R.id.edConfirmPassword);
        edAddress = view.findViewById(R.id.edAddress);
        btnSignIn = view.findViewById(R.id.btnSignUp);
        btnBack = view.findViewById(R.id.btnBack);

        // Khởi tạo ApiService (chính xác)
        apiService = RetrofitClient.getApiService(); // Giả sử bạn dùng RetrofitClient

        btnSignIn.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                String email = edEmail.getText().toString().trim();
                String username = edSignInUsername.getText().toString().trim();
                String password = edSignInPassword.getText().toString();
                String confirmPassword = edConfirmPassword.getText().toString();
                // FIX 1: Lấy text từ edAddress
                String address = edAddress.getText().toString().trim();

                boolean isValid = true;

                // FIX 2: Thêm kiểm tra cho address
                if (address.isEmpty()) {
                    Toast.makeText(getContext(), "Vui lòng nhập địa chỉ", Toast.LENGTH_SHORT).show();
                    isValid = false;
                } else if (!isValidUsername(username)) {
                    Toast.makeText(getContext(), "Tên tài khoản không hợp lệ (ít nhất 6 ký tự)", Toast.LENGTH_SHORT).show();
                    isValid = false;
                } else if (!isValidPassword(password)) {
                    Toast.makeText(getContext(), "Mật khẩu phải từ 6-10 ký tự và chứa cả chữ và số", Toast.LENGTH_SHORT).show();
                    isValid = false;
                } else if (!password.equals(confirmPassword)) {
                    Toast.makeText(getContext(), "Mật khẩu không khớp", Toast.LENGTH_SHORT).show();
                    isValid = false;
                } else if (!isValidEmail(email)) {
                    Toast.makeText(getContext(), "Email không hợp lệ", Toast.LENGTH_SHORT).show();
                    isValid = false;
                }

                if (isValid) {
                    btnSignIn.setEnabled(false);
                    // FIX 3: Truyền 4 tham số
                    registerUser(username, password, email, address);
                } else {
                    edSignInPassword.setText("");
                    edConfirmPassword.setText("");
                }
            }
        });

        // ... (btnBack giữ nguyên) ...
    }

    // FIX 4: Sửa chữ ký hàm, thêm String address
    private void registerUser(String username, String password, String email, String address) {

        // FIX 5: Gọi constructor mới với 4 tham số
        RegisterRequest registerRequest = new RegisterRequest(username, password, email, address);

        Call<String> call = apiService.register(registerRequest);
        call.enqueue(new Callback<String>() {
            @Override
            public void onResponse(Call<String> call, Response<String> response) {
                btnSignIn.setEnabled(true);

                if (response.isSuccessful()) {
                    Toast.makeText(getContext(), "Đăng ký thành công!", Toast.LENGTH_SHORT).show();
                    if (getActivity() != null) {
                        getActivity().getSupportFragmentManager().popBackStack();
                    }
                } else {
                    Toast.makeText(getContext(), "Đăng ký thất bại. Tên tài khoản hoặc email có thể đã tồn tại.", Toast.LENGTH_LONG).show();
                }
            }

            @Override
            public void onFailure(Call<String> call, Throwable t) {
                btnSignIn.setEnabled(true);
                Log.e("RegisterError", "API Call Failed: " + t.getMessage());
                Toast.makeText(getContext(), "Lỗi kết nối. Vui lòng thử lại.", Toast.LENGTH_SHORT).show();
            }
        });
    }


    // --- Các hàm helper (giữ nguyên) ---

    private boolean isValidPassword(String password) {
        if (password.length() < 6 || password.length() > 10) {
            return false;
        }
        boolean hasLetter = false;
        boolean hasDigit = false;
        for (char c : password.toCharArray()) {
            if (Character.isLetter(c)) hasLetter = true;
            else if (Character.isDigit(c)) hasDigit = true;
        }
        return hasLetter && hasDigit;
    }

    private boolean isValidEmail(String email) {
        if (email == null || email.isEmpty()) return false;
        return Patterns.EMAIL_ADDRESS.matcher(email).matches();
    }

    private boolean isValidUsername(String username) {
        return !username.isEmpty() && username.length() >= 6;
    }
}