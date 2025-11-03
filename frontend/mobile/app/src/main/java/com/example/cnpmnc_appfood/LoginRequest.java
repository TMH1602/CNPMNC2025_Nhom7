package com.example.cnpmnc_appfood;

// Dùng để serialize tên biến thành JSON
import com.google.gson.annotations.SerializedName;

public class LoginRequest {

    // Tên biến phải khớp với JSON: "username" và "password"
    @SerializedName("username")
    private String username;

    @SerializedName("password")
    private String password;

    public LoginRequest(String username, String password) {
        this.username = username;
        this.password = password;
    }

    // Getters/Setters (có thể cần thiết cho Gson)
    public String getUsername() { return username; }
    public void setUsername(String username) { this.username = username; }
    public String getPassword() { return password; }
    public void setPassword(String password) { this.password = password; }
}