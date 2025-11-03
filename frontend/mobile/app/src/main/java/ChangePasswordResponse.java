package com.example.cnpmnc_appfood;

// Giả định API trả về một thông báo thành công hoặc thất bại.
public class ChangePasswordResponse {
    private boolean success;
    private String message;

    public boolean isSuccess() { return success; }
    public String getMessage() { return message; }
}
