package com.example.cnpmnc_appfood;

import android.content.Context;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.text.TextUtils;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.EditText;
import android.widget.TextView;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class ChangePasswordFragment extends Fragment {

    private EditText etOldPassword;
    private EditText etNewPassword;
    private EditText etConfirmNewPassword;
    private Button btnBack;
    private Button btnSave;
    private TextView tvResponse;

    private String currentIdentifier;
    private ApiService apiService;

    @Override
    public void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        // Lấy identifier (username)
        SharedPreferences prefs = requireActivity().getSharedPreferences("UserPrefs", Context.MODE_PRIVATE);
        // Giả sử "USERNAME" là identifier (có thể là email hoặc username)
        currentIdentifier = prefs.getString("USERNAME", "guest");
        apiService = RetrofitClient.getApiService();
    }

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container,
                             @Nullable Bundle savedInstanceState) {

        // Giả định layout file là change_password_page.xml
        View view = inflater.inflate(R.layout.change_password_page, container, false);

        etOldPassword = view.findViewById(R.id.editTextText); // Mật khẩu cũ
        etNewPassword = view.findViewById(R.id.editTextTextPassword); // Mật khẩu mới
        etConfirmNewPassword = view.findViewById(R.id.editTextTextPassword2); // Xác nhận mật khẩu mới
        btnBack = view.findViewById(R.id.btnback);
        btnSave = view.findViewById(R.id.btnsave);
        tvResponse = view.findViewById(R.id.textviewresponse);

        // Khởi tạo trạng thái response
        tvResponse.setText("Sẵn sàng đổi mật khẩu.");

        // Xử lý nút QUAY LẠI
        btnBack.setOnClickListener(v -> getParentFragmentManager().popBackStack());

        // Xử lý nút LƯU
        btnSave.setOnClickListener(v -> validateAndChangePassword());

        return view;
    }

    private void validateAndChangePassword() {
        String oldPass = etOldPassword.getText().toString();
        String newPass = etNewPassword.getText().toString();
        String confirmPass = etConfirmNewPassword.getText().toString();

        // 1. Kiểm tra đầu vào rỗng
        if (TextUtils.isEmpty(oldPass) || TextUtils.isEmpty(newPass) || TextUtils.isEmpty(confirmPass)) {
            tvResponse.setText("Vui lòng nhập đầy đủ các trường.");
            return;
        }

        // 2. Kiểm tra khớp mật khẩu mới (CLIENT-SIDE VALIDATION)
        if (!newPass.equals(confirmPass)) {
            // Hiển thị lỗi trên textviewresponse
            tvResponse.setText("Mật khẩu mới và xác nhận mật khẩu mới không giống nhau");
            return;
        }

        // 3. Kiểm tra user đã đăng nhập chưa
        if (currentIdentifier.equals("guest")) {
            tvResponse.setText("Bạn không thể đổi mật khẩu khi chưa đăng nhập.");
            return;
        }

        // 4. Gọi API
        callChangePasswordApi(oldPass, newPass);
    }

    private void callChangePasswordApi(String oldPassword, String newPassword) {
        tvResponse.setText("Đang xử lý...");

        ChangePasswordRequest request = new ChangePasswordRequest(
                currentIdentifier,
                oldPassword,
                newPassword
        );

        apiService.changePassword(request).enqueue(new Callback<ChangePasswordResponse>() {
            @Override
            public void onResponse(@NonNull Call<ChangePasswordResponse> call, @NonNull Response<ChangePasswordResponse> response) {
                if (response.isSuccessful() && response.body() != null) {
                    // API trả về thành công (HTTP 200) và success=true
                    if (response.body().isSuccess()) {
                        tvResponse.setText("Đổi mật khẩu thành công!");
                        Toast.makeText(requireContext(), "Mật khẩu đã được thay đổi!", Toast.LENGTH_LONG).show();
                        // Quay lại Fragment trước
                        getParentFragmentManager().popBackStack();
                    } else {
                        // API trả về HTTP 200 nhưng success=false (ví dụ: mật khẩu cũ sai)
                        tvResponse.setText(response.body().getMessage());
                    }
                } else {
                    // Lỗi HTTP (ví dụ: 400 Bad Request, 500 Server Error)
                    String errorMsg = "Lỗi đổi mật khẩu: " + response.code();
                    try {
                        // Cố gắng đọc thông báo lỗi từ body nếu có
                        errorMsg += " - " + response.errorBody().string();
                    } catch (Exception e) {
                        // Bỏ qua
                    }
                    Log.e("ChangePassword", errorMsg);
                    tvResponse.setText("Đổi mật khẩu thất bại. Mã lỗi: " + response.code());
                }
            }

            @Override
            public void onFailure(@NonNull Call<ChangePasswordResponse> call, @NonNull Throwable t) {
                Log.e("ChangePassword", "Lỗi kết nối: " + t.getMessage());
                tvResponse.setText("Lỗi kết nối mạng khi đổi mật khẩu.");
            }
        });
    }
}
