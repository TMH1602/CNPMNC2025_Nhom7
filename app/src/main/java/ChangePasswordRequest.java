package com.example.cnpmnc_appfood;

public class ChangePasswordRequest {
    private String identifier;
    private String oldPassword;
    private String newPassword;

    public ChangePasswordRequest(String identifier, String oldPassword, String newPassword) {
        this.identifier = identifier;
        this.oldPassword = oldPassword;
        this.newPassword = newPassword;
    }
}
