package com.example.cnpmnc_appfood;

import android.annotation.SuppressLint;
import android.content.Context;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.ListView;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;

import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Date;
import java.util.List;
import java.util.Locale;
import java.text.NumberFormat;

import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class DishManagementFragment extends Fragment {

    private ListView lvUserProfile;
    private ListView lvOrderHistory;
    private Button btnLogout;
    private Button btnHome;
    private Button btnChangePassword; // KHAI BÁO BIẾN CHO NÚT ĐỔI MẬT KHẨU
    private String currentUsername;
    private ApiService apiService;

    @Override
    public void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        SharedPreferences prefs = requireActivity().getSharedPreferences("UserPrefs", Context.MODE_PRIVATE);
        currentUsername = prefs.getString("USERNAME", "guest");
        apiService = RetrofitClient.getApiService();
    }

    @SuppressLint("MissingInflatedId")
    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container,
                             @Nullable Bundle savedInstanceState) {

        View view = inflater.inflate(R.layout.fragment_dish_management, container, false);

        lvUserProfile = view.findViewById(R.id.lvUserProfile);
        lvOrderHistory = view.findViewById(R.id.lvOrderHistory);
        btnLogout = view.findViewById(R.id.button);
        btnHome = view.findViewById(R.id.btnHome);
        // 🎯 THÊM DÒNG TÌM KIẾM NÚT ĐỔI MẬT KHẨU
        btnChangePassword = view.findViewById(R.id.btnChangePassword);

        // Gán Listener Đăng Xuất
        if (btnLogout != null) {
            btnLogout.setOnClickListener(v -> handleLogout());
        } else {
            Log.e("DishManagement", "Nút Đăng xuất (R.id.button) không tìm thấy!");
        }

        // GÁN LISTENER HOME
        if (btnHome != null) {
            btnHome.setOnClickListener(v -> navigateToHome());
        } else {
            Log.e("DishManagement", "Nút Home (R.id.btnHome) không tìm thấy!");
        }

        // 🎯 GÁN LISTENER CHO NÚT ĐỔI MẬT KHẨU
        if (btnChangePassword != null) {
            btnChangePassword.setOnClickListener(v -> navigateToChangePassword());
        } else {
            Log.e("DishManagement", "Nút Đổi mật khẩu (R.id.btnChangePassword) không tìm thấy!");
        }


        loadUserProfile();
        loadOrderHistory();

        return view;
    }

    /**
     * Chuyển về HomeFragment.
     */
    private void navigateToHome() {
        if (getActivity() != null) {
            Fragment homeFragment = new HomeFragment();
            getActivity().getSupportFragmentManager().beginTransaction()
                    .replace(R.id.fragment_container, homeFragment)
                    .commit();
        }
    }

    /**
     * Chuyển đến ChangePasswordFragment.
     */
    private void navigateToChangePassword() {
        if (getActivity() != null) {
            Fragment changePasswordFragment = new ChangePasswordFragment();
            getActivity().getSupportFragmentManager().beginTransaction()
                    .replace(R.id.fragment_container, changePasswordFragment)
                    .addToBackStack(null) // Thêm vào stack để có thể quay lại
                    .commit();
        }
    }

    /**
     * Xử lý ĐĂNG XUẤT: Xóa SharedPreferences và chuyển về LoginFragment.
     */
    private void handleLogout() {
        SharedPreferences prefs = requireActivity().getSharedPreferences("UserPrefs", Context.MODE_PRIVATE);
        SharedPreferences.Editor editor = prefs.edit();

        editor.remove("USERNAME");
        editor.apply();

        Toast.makeText(requireContext(), "Đã đăng xuất thành công!", Toast.LENGTH_SHORT).show();

        if (getActivity() != null) {
            Fragment loginFragment = new LoginFragment();
            getActivity().getSupportFragmentManager().beginTransaction()
                    .replace(R.id.fragment_container, loginFragment)
                    .commit();
        }
    }

    // --- TẢI HỒ SƠ NGƯỜI DÙNG ---

    private void loadUserProfile() {
        if (currentUsername.equals("guest")) {
            displayUserProfile(null);
            return;
        }

        apiService.getUserProfile(currentUsername).enqueue(new Callback<UserProfileResponse>() {
            @Override
            public void onResponse(@NonNull Call<UserProfileResponse> call, @NonNull Response<UserProfileResponse> response) {
                if (response.isSuccessful() && response.body() != null) {
                    displayUserProfile(response.body());
                } else {
                    Log.e("UserManagement", "Lỗi tải hồ sơ: " + response.code());
                    displayUserProfile(null);
                }
            }

            @Override
            public void onFailure(@NonNull Call<UserProfileResponse> call, @NonNull Throwable t) {
                Log.e("UserManagement", "Lỗi kết nối tải hồ sơ: " + t.getMessage());
                displayUserProfile(null);
            }
        });
    }

    private void displayUserProfile(@Nullable UserProfileResponse profile) {
        List<String> profileData = new ArrayList<>();
        if (profile != null) {
            String formattedDate = profile.getCreatedDate();
            try {
                SimpleDateFormat apiFormat = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss", Locale.getDefault());
                Date date = apiFormat.parse(profile.getCreatedDate());

                SimpleDateFormat displayFormat = new SimpleDateFormat("dd/MM/yyyy", Locale.getDefault());
                formattedDate = displayFormat.format(date);
            } catch (Exception e) {
                Log.e("UserManagement", "Lỗi định dạng ngày: " + e.getMessage());
                formattedDate = "Không rõ";
            }

            profileData.add("Tên tài khoản: " + profile.getDisplayName());
            profileData.add("Email: " + profile.getEmail());
            profileData.add("Ngày tham gia: " + formattedDate);
        } else {
            profileData.add("Không thể tải hồ sơ người dùng.");
            profileData.add("Vui lòng thử lại hoặc đăng nhập.");
        }

        ArrayAdapter<String> adapter = new ArrayAdapter<>(requireContext(), android.R.layout.simple_list_item_1, profileData);
        lvUserProfile.setAdapter(adapter);
        setListViewHeightBasedOnItems(lvUserProfile);
    }

    // --- TẢI LỊCH SỬ MUA HÀNG ---

    private void loadOrderHistory() {
        if (currentUsername.equals("guest")) {
            displayOrderHistory(new ArrayList<>());
            return;
        }

        apiService.getOrderHistory(currentUsername).enqueue(new Callback<List<OrderHistoryResponse>>() {
            @Override
            public void onResponse(@NonNull Call<List<OrderHistoryResponse>> call, @NonNull Response<List<OrderHistoryResponse>> response) {
                if (response.isSuccessful() && response.body() != null) {
                    displayOrderHistory(response.body());
                } else {
                    Log.e("UserManagement", "Lỗi tải lịch sử: " + response.code());
                    displayOrderHistory(new ArrayList<>());
                }
            }

            @Override
            public void onFailure(@NonNull Call<List<OrderHistoryResponse>> call, @NonNull Throwable t) {
                Log.e("UserManagement", "Lỗi kết nối tải lịch sử: " + t.getMessage());
                displayOrderHistory(new ArrayList<>());
            }
        });
    }

    private void displayOrderHistory(List<OrderHistoryResponse> historyList) {
        List<String> historySummary = new ArrayList<>();
        if (historyList.isEmpty()) {
            historySummary.add("Không có lịch sử mua hàng nào.");
        } else {
            NumberFormat nf = NumberFormat.getInstance(new Locale("vi", "VN"));
            for (OrderHistoryResponse order : historyList) {
                String statusVn = mapStatusToVietnamese(order.getStatus());

                // Tránh lỗi khi items rỗng (nếu API có trả về đơn hàng không có mục)
                String itemSummary = "";
                if (order.getItems() != null && !order.getItems().isEmpty()) {
                    itemSummary = order.getItems().get(0).getProductName()
                            + (order.getItems().size() > 1 ? " và " + (order.getItems().size() - 1) + " món khác" : "");
                }

                String summary = String.format(
                        "Đơn hàng #%d | %s\nTổng tiền: %s VNĐ | Trạng thái: %s",
                        order.getOrderId(),
                        order.getOrderDate().substring(0, 10),
                        nf.format(order.getTotalAmount()),
                        statusVn
                );
                historySummary.add(summary);
            }
        }

        // Sử dụng simple_list_item_1 thay vì simple_list_item_2 để tránh lỗi NullPointerException
        ArrayAdapter<String> adapter = new ArrayAdapter<>(requireContext(), android.R.layout.simple_list_item_1, historySummary);
        lvOrderHistory.setAdapter(adapter);
        setListViewHeightBasedOnItems(lvOrderHistory);
    }

    private String mapStatusToVietnamese(String status) {
        switch (status) {
            case "Paid": return "Đã Thanh Toán";
            case "Pending": return "Chờ Xử Lý";
            case "Done": return "Hoàn Thành";
            case "PaymentFailed": return "Thanh Toán Thất Bại";
            default: return status;
        }
    }

    /**
     * Phương thức giúp tính toán và đặt lại chiều cao của ListView (Phiên bản an toàn).
     * KHẮC PHỤC: Sử dụng kiểm tra chiều rộng an toàn để tránh lỗi đo lường.
     */
    public static boolean setListViewHeightBasedOnItems(ListView listView) {
        ArrayAdapter listAdapter = (ArrayAdapter) listView.getAdapter();
        if (listAdapter == null) {
            return false;
        }

        int totalHeight = 0;

        // Trường hợp 2: Lỗi xảy ra nếu ListView chưa được vẽ (width = 0)
        int desiredWidth = listView.getWidth() > 0 ? listView.getWidth() : View.MeasureSpec.makeMeasureSpec(0, View.MeasureSpec.UNSPECIFIED);
        if (desiredWidth == 0) {
            // Dùng giá trị mặc định nếu chưa đo được
            desiredWidth = View.MeasureSpec.makeMeasureSpec(listView.getResources().getDisplayMetrics().widthPixels, View.MeasureSpec.AT_MOST);
        } else {
            desiredWidth = View.MeasureSpec.makeMeasureSpec(desiredWidth, View.MeasureSpec.AT_MOST);
        }

        for (int i = 0; i < listAdapter.getCount(); i++) {
            View listItem = listAdapter.getView(i, null, listView);
            listItem.measure(desiredWidth, View.MeasureSpec.UNSPECIFIED);
            totalHeight += listItem.getMeasuredHeight();
        }

        ViewGroup.LayoutParams params = listView.getLayoutParams();
        params.height = totalHeight + (listView.getDividerHeight() * (listAdapter.getCount() - 1));
        listView.setLayoutParams(params);
        listView.requestLayout();
        return true;
    }
}
