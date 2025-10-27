package com.example.cnpmnc_appfood;

import java.security.cert.CertificateException;
import java.security.cert.X509Certificate;
import javax.net.ssl.HostnameVerifier;
import javax.net.ssl.SSLContext;
import javax.net.ssl.SSLSession;
import javax.net.ssl.SSLSocketFactory;
import javax.net.ssl.TrustManager;
import javax.net.ssl.X509TrustManager;
import okhttp3.OkHttpClient;

// *** 1. THÊM IMPORT NÀY VÀO ***
import okhttp3.logging.HttpLoggingInterceptor;

/**
 * Cung cấp một OkHttpClient "không an toàn" BỎ QUA SSL
 * VÀ "IN" TẤT CẢ LOG MẠNG RA LOGCAT.
 */
public class UnsafeOkHttpClient {
    public static OkHttpClient getUnsafeOkHttpClient() {
        try {
            // Tạo một TrustManager không kiểm tra chuỗi chứng chỉ (Code của bạn đã có)
            final TrustManager[] trustAllCerts = new TrustManager[]{
                    new X509TrustManager() {
                        @Override
                        public void checkClientTrusted(X509Certificate[] chain, String authType) throws CertificateException {
                        }

                        @Override
                        public void checkServerTrusted(X509Certificate[] chain, String authType) throws CertificateException {
                        }

                        @Override
                        public X509Certificate[] getAcceptedIssuers() {
                            return new X509Certificate[]{};
                        }
                    }
            };

            // Cài đặt TrustManager (Code của bạn đã có)
            final SSLContext sslContext = SSLContext.getInstance("SSL");
            sslContext.init(null, trustAllCerts, new java.security.SecureRandom());
            final SSLSocketFactory sslSocketFactory = sslContext.getSocketFactory();

            OkHttpClient.Builder builder = new OkHttpClient.Builder();
            builder.sslSocketFactory(sslSocketFactory, (X509TrustManager) trustAllCerts[0]);

            // Tắt kiểm tra Hostname (Code của bạn đã có)
            builder.hostnameVerifier(new HostnameVerifier() {
                @Override
                public boolean verify(String hostname, SSLSession session) {
                    return true;
                }
            });

            // =======================================================
            // *** 2. THÊM 3 DÒNG "IN LOG" VÀO ĐÂY ***
            // =======================================================
            HttpLoggingInterceptor logging = new HttpLoggingInterceptor();
            logging.setLevel(HttpLoggingInterceptor.Level.BODY); // In cả Header và Body
            builder.addInterceptor(logging);
            // =======================================================

            return builder.build();
        } catch (Exception e) {
            throw new RuntimeException(e);
        }
    }
}