package com.example.cnpmnc_appfood;

import android.app.Application;
import android.util.Log; // Thêm import Log
import com.cloudinary.android.MediaManager;
import java.util.HashMap;
import java.util.Map;

public class MyApplication extends Application {
    private static final String TAG = "MyApplication"; // Thêm TAG để log

    @Override
    public void onCreate() {
        super.onCreate();

        // Cấu hình Cloudinary (Thay bằng thông tin thật của bạn)
        Map<String, String> config = new HashMap<>(); // Sử dụng kiểu cụ thể
        config.put("cloud_name", "du42rq1ki");         // Từ appsettings.json
        config.put("api_key", "357939164477565");      // Từ appsettings.json
        config.put("api_secret", "uyoPUP_qjeChCHtHgoE45JiP9fM"); // Từ appsettings.json
        // Tùy chọn: config.put("secure", "true"); // Sử dụng https

        try {
            MediaManager.init(this, config);
            Log.i(TAG, "Cloudinary initialized successfully."); // Log thành công
        } catch (IllegalStateException e) {
            // Xử lý trường hợp đã được khởi tạo (có thể xảy ra nếu onCreate() gọi lại)
            Log.w(TAG, "MediaManager already initialized or error during init: " + e.getMessage());
        } catch (Exception e) {
            // Bắt các lỗi khởi tạo khác
            Log.e(TAG, "Error initializing Cloudinary: ", e);
        }
    }
}