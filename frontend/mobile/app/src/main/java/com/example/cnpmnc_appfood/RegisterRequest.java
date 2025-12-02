// Trong file: RegisterRequest.java
package com.example.cnpmnc_appfood;

public class RegisterRequest {
    private String username;
    private String password;
    private String address; // <-- THÊM DÒNG NÀY
    private String email;

    // Sửa constructor để nhận 4 tham số
    public RegisterRequest(String username, String password, String email, String address) {
        this.username = username;
        this.password = password;
        this.email = email;
        this.address = address; // <-- THÊM DÒNG NÀY
    }

    // (Getter và Setter nếu cần, nhưng Retrofit thường không cần)
}