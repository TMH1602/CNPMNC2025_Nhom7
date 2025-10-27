package com.example.cnpmnc_appfood;

import android.annotation.SuppressLint;
import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.net.Uri;
import android.os.Bundle;
// Thêm import Log
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ImageView;
import android.widget.Toast;
// Thêm import ProgressBar (nếu bạn muốn hiển thị tiến trình)
// import android.widget.ProgressBar;

import androidx.activity.result.ActivityResultLauncher;
import androidx.activity.result.contract.ActivityResultContracts;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;
import com.bumptech.glide.Glide;
// Thêm import Cloudinary
import com.cloudinary.android.MediaManager;
import com.cloudinary.android.callback.ErrorInfo;
import com.cloudinary.android.callback.UploadCallback;
import java.util.Map; // Import Map

public class DishManagementFragment extends Fragment {

    private static final String TAG = "DishManagementFragment"; // Thêm TAG để log dễ hơn

    private EditText etName, etDescription, etPrice;
    private ImageView ivDishPreview;
    private Button btnAdd, btnSelectImage;
    // private ProgressBar progressBar; // Thêm ProgressBar nếu muốn
    private String selectedImageUriString = null; // Vẫn lưu URI dạng String sau khi chọn
    private Uri imageUriToUpload = null; // Biến lưu Uri để tải lên

    private DishRepository dishRepository;

    // --- Sửa lại Launcher để an toàn hơn và lưu Uri ---
    private final ActivityResultLauncher<Intent> imagePickerLauncher = registerForActivityResult(
            new ActivityResultContracts.StartActivityForResult(),
            result -> {
                // Thêm kiểm tra null kỹ hơn
                if (result.getResultCode() == Activity.RESULT_OK
                        && result.getData() != null
                        && result.getData().getData() != null) {

                    imageUriToUpload = result.getData().getData(); // Lưu Uri trực tiếp
                    selectedImageUriString = imageUriToUpload.toString(); // Vẫn lưu String để kiểm tra null

                    // Hiển thị ảnh preview (thêm kiểm tra null)
                    if (getContext() != null && ivDishPreview != null) {
                        Glide.with(getContext())
                                .load(imageUriToUpload) // Load từ Uri
                                .placeholder(android.R.drawable.ic_menu_gallery) // Ảnh chờ tải
                                .error(android.R.drawable.ic_dialog_alert) // Ảnh khi lỗi
                                .into(ivDishPreview);
                    }
                    Log.d(TAG, "Ảnh đã chọn: " + selectedImageUriString);
                } else {
                    Log.w(TAG, "Người dùng hủy chọn ảnh hoặc có lỗi.");
                    // Giữ nguyên ảnh preview cũ hoặc xóa ảnh preview
                    // imageUriToUpload = null; // Có thể reset nếu muốn
                    // selectedImageUriString = null;
                }
            }
    );

    @SuppressLint("MissingInflatedId")
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_dish_management, container, false);

        // Ánh xạ views
        etName = view.findViewById(R.id.etDishName);
        etDescription = view.findViewById(R.id.etDishDescription);
        etPrice = view.findViewById(R.id.etDishPrice);
        ivDishPreview = view.findViewById(R.id.ivDishPreview);
        btnAdd = view.findViewById(R.id.btnAddDish);
        btnSelectImage = view.findViewById(R.id.btnSelectImage);
        // progressBar = view.findViewById(R.id.progressBarUpload); // Ánh xạ ProgressBar nếu có

        // Lấy instance của Repository
        dishRepository = DishRepository.getInstance();

        // Gán sự kiện click
        btnSelectImage.setOnClickListener(v -> openImageChooser());
        btnAdd.setOnClickListener(v -> addDish());

        return view;
    }

    // Mở trình chọn ảnh (Giữ nguyên)
    private void openImageChooser() {
        Intent intent = new Intent(Intent.ACTION_GET_CONTENT);
        intent.setType("image/*");
        imagePickerLauncher.launch(intent);
    }

    // --- Sửa lại hoàn toàn hàm addDish ---
    private void addDish() {
        // Lấy thông tin từ EditText
        String name = etName.getText().toString().trim();
        String desc = etDescription.getText().toString().trim();
        String priceStr = etPrice.getText().toString().trim();
        double price;

        // Kiểm tra thông tin nhập (Quan trọng: kiểm tra imageUriToUpload thay vì selectedImageUriString)
        if (name.isEmpty() || priceStr.isEmpty() || desc.isEmpty() || imageUriToUpload == null) {
            Toast.makeText(getContext(), "Vui lòng nhập đầy đủ thông tin và chọn ảnh.", Toast.LENGTH_LONG).show();
            return;
        }

        try {
            price = Double.parseDouble(priceStr);
        } catch (NumberFormatException e) {
            Toast.makeText(getContext(), "Giá không hợp lệ.", Toast.LENGTH_SHORT).show();
            return;
        }

        // --- BẮT ĐẦU TẢI ẢNH LÊN CLOUDINARY ---
        setLoadingState(true); // Hiển thị trạng thái đang tải
        Log.d(TAG, "Bắt đầu tải ảnh lên Cloudinary: " + imageUriToUpload.toString());

        // Sử dụng Uri đã lưu để tải lên
        String requestId = MediaManager.get().upload(imageUriToUpload)
                // QUAN TRỌNG: Vào Cloudinary -> Settings -> Upload -> Upload Presets
                // Tạo một preset mới, chọn Signing Mode là "Unsigned", ghi lại tên preset đó.
                .unsigned("ml_default") // <-- THAY TÊN UPLOAD PRESET (UNSIGNED) CỦA BẠN VÀO ĐÂY
                .option("resource_type", "image") // Chỉ định loại tài nguyên là ảnh
                .callback(new UploadCallback() {
                    @Override
                    public void onStart(String requestId) {
                        Log.d(TAG, "Bắt đầu tải lên Cloudinary (onStart): " + requestId);
                        // UI đã ở trạng thái loading
                    }

                    @Override
                    public void onProgress(String requestId, long bytes, long totalBytes) {
                        // Cập nhật tiến trình nếu muốn
                        // int progress = (int) ((double) bytes / totalBytes * 100);
                        // Log.d(TAG, "Tiến trình tải lên Cloudinary: " + progress + "%");
                    }

                    @Override
                    public void onSuccess(String requestId, Map resultData) {
                        setLoadingState(false); // Tắt trạng thái đang tải
                        Log.d(TAG, "Tải lên Cloudinary thành công. Data: " + resultData.toString());

                        // Lấy URL công khai từ kết quả trả về
                        String publicUrl = (String) resultData.get("secure_url"); // Ưu tiên HTTPS
                        if (publicUrl == null) {
                            publicUrl = (String) resultData.get("url"); // Thử HTTP nếu không có HTTPS
                        }

                        if (publicUrl != null) {
                            Log.i(TAG, "URL ảnh Cloudinary: " + publicUrl);

                            // *** CHỈ GỌI REPOSITORY SAU KHI CÓ URL CÔNG KHAI ***
                            dishRepository.addDish(name, desc, price, publicUrl);

                            Toast.makeText(getContextSafe(), "Đã thêm món ăn '" + name + "' thành công!", Toast.LENGTH_LONG).show();

                            // Xóa form sau khi mọi thứ thành công
                            clearForm();
                        } else {
                            // Lỗi không mong muốn: Cloudinary không trả về URL
                            Log.e(TAG, "Không tìm thấy 'secure_url' hoặc 'url' trong kết quả Cloudinary.");
                            Toast.makeText(getContextSafe(), "Lỗi: Không lấy được URL ảnh sau khi tải lên.", Toast.LENGTH_LONG).show();
                        }
                    }

                    @Override
                    public void onError(String requestId, ErrorInfo error) {
                        setLoadingState(false); // Tắt trạng thái đang tải
                        Log.e(TAG, "Lỗi tải lên Cloudinary: " + error.getDescription() + ", Code: " + error.getCode());
                        Toast.makeText(getContextSafe(), "Lỗi tải ảnh lên: " + error.getDescription(), Toast.LENGTH_LONG).show();
                    }

                    @Override
                    public void onReschedule(String requestId, ErrorInfo error) {
                        setLoadingState(false); // Tắt trạng thái đang tải
                        Log.w(TAG, "Tải lên Cloudinary bị hoãn: " + error.getDescription());
                        Toast.makeText(getContextSafe(), "Tải ảnh lên bị hoãn, vui lòng thử lại.", Toast.LENGTH_LONG).show();
                    }
                })
                .dispatch(); // Gửi yêu cầu tải lên
    }

    // Hàm dọn dẹp form sau khi thêm thành công hoặc khi cần reset
    private void clearForm() {
        if (etName != null) etName.setText("");
        if (etDescription != null) etDescription.setText("");
        if (etPrice != null) etPrice.setText("");
        selectedImageUriString = null;
        imageUriToUpload = null; // Reset cả Uri
        if (ivDishPreview != null && getContext() != null) {
            // Đặt lại ảnh preview về mặc định
            Glide.with(getContext()).clear(ivDishPreview); // Xóa ảnh cũ
            ivDishPreview.setImageResource(android.R.drawable.ic_menu_gallery); // Đặt ảnh placeholder
        }
        Log.d(TAG, "Form đã được xóa.");
    }

    // Hàm quản lý trạng thái đang tải (ví dụ: vô hiệu hóa nút, hiển thị ProgressBar)
    private void setLoadingState(boolean isLoading) {
        if (btnAdd != null) {
            btnAdd.setEnabled(!isLoading); // Vô hiệu hóa/Kích hoạt nút Add
        }
        if (btnSelectImage != null) {
            btnSelectImage.setEnabled(!isLoading); // Vô hiệu hóa/Kích hoạt nút Chọn Ảnh
        }
        // if (progressBar != null) {
        //     progressBar.setVisibility(isLoading ? View.VISIBLE : View.GONE); // Hiển thị/Ẩn ProgressBar
        // }
        // Bạn cũng có thể thêm lớp phủ mờ lên màn hình
    }

    // Hàm lấy Context an toàn, tránh lỗi khi Fragment đã detach
    private Context getContextSafe() {
        Context context = getContext();
        if (context == null && getActivity() != null) {
            context = getActivity().getApplicationContext();
        }
        return context;
    }
}