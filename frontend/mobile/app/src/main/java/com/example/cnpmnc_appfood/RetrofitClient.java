package com.example.cnpmnc_appfood;

import javax.net.ssl.HostnameVerifier;
import javax.net.ssl.SSLContext;
import javax.net.ssl.SSLSession;
import javax.net.ssl.SSLSocketFactory;
import javax.net.ssl.TrustManager;
import javax.net.ssl.X509TrustManager;
import java.security.cert.CertificateException;
import okhttp3.OkHttpClient;
import retrofit2.Retrofit;
import retrofit2.converter.gson.GsonConverterFactory;
// FIX: THÊM IMPORT NÀY
import retrofit2.converter.scalars.ScalarsConverterFactory;
public class RetrofitClient {

    // CHỌN 1 TRONG 2 BASE_URL NÀY
    // 1. Dùng cho Máy ảo (Emulator)
    private static final String BASE_URL = "https://10.0.2.2:7132/";

    // 2. Dùng cho Máy thật (thay IP của bạn vào)
    // private static final String BASE_URL = "https://192.168.1.10:7132/";

    private static Retrofit retrofit = null;

    public static ApiService getApiService() {
        if (retrofit == null) {
            retrofit = new Retrofit.Builder()
                    .baseUrl(BASE_URL)
                    .client(getUnsafeOkHttpClient())

                    // FIX: THÊM DÒNG NÀY (PHẢI TRƯỚC GSON)
                    .addConverterFactory(ScalarsConverterFactory.create())

                    // Giữ lại dòng này cho các API trả về JSON
                    .addConverterFactory(GsonConverterFactory.create())

                    .build();
        }
        return retrofit.create(ApiService.class);
    }

    // HÀM QUAN TRỌNG ĐỂ VƯỢT QUA LỖI SSL
    private static OkHttpClient getUnsafeOkHttpClient() {
        try {
            // Tạo một TrustManager tin tưởng mọi chứng chỉ
            final TrustManager[] trustAllCerts = new TrustManager[]{
                    new X509TrustManager() {
                        @Override
                        public void checkClientTrusted(java.security.cert.X509Certificate[] chain, String authType) throws CertificateException {
                        }

                        @Override
                        public void checkServerTrusted(java.security.cert.X509Certificate[] chain, String authType) throws CertificateException {
                        }

                        @Override
                        public java.security.cert.X509Certificate[] getAcceptedIssuers() {
                            return new java.security.cert.X509Certificate[]{};
                        }
                    }
            };

            // Cài đặt SSLContext
            final SSLContext sslContext = SSLContext.getInstance("SSL");
            sslContext.init(null, trustAllCerts, new java.security.SecureRandom());

            // Tạo một SSLSocketFactory
            final SSLSocketFactory sslSocketFactory = sslContext.getSocketFactory();

            OkHttpClient.Builder builder = new OkHttpClient.Builder();
            builder.sslSocketFactory(sslSocketFactory, (X509TrustManager) trustAllCerts[0]);

            // Bỏ qua việc xác minh tên máy chủ (hostname)
            builder.hostnameVerifier(new HostnameVerifier() {
                @Override
                public boolean verify(String hostname, SSLSession session) {
                    return true;
                }
            });

            return builder.build();
        } catch (Exception e) {
            throw new RuntimeException(e);
        }
    }
}