package com.example.cnpmnc_appfood;

import java.util.List;

import retrofit2.Call;
import retrofit2.http.Body;
import retrofit2.http.GET;
import retrofit2.http.Headers;
import retrofit2.http.POST;
import retrofit2.http.Path;
import retrofit2.http.Query;
public interface ApiService {

    // Phương thức login từ trước
    @Headers({
            "accept: text/plain",
            "Content-Type: application/json"
    })
    @POST("api/Auth/login")
    Call<String> login(@Body LoginRequest loginRequest);


    // THÊM PHƯƠNG THỨC NÀY:
    @Headers({
            "accept: text/plain",
            "Content-Type: application/json"
    })
    @POST("api/Auth/register")
    Call<String> register(@Body RegisterRequest registerRequest);
    @Headers({
            "accept: application/json"
    })
    @GET("api/Menu")
    Call<List<Dish>> getMenu();

    // ********************************************
    // *** THÊM PHƯƠNG THỨC NÀY VÀO ***
    // ********************************************
    /**
     * Gửi một đối tượng Dish mới lên server qua HTTP POST.
     * @Body newDish: Retrofit sẽ tự động chuyển đối tượng Dish này thành JSON.
     * Trả về Call<Dish> (giả sử server trả lại đối tượng đã được tạo,
     * bao gồm cả 'id' mới).
     */
    @POST("api/Menu")
    Call<Dish> createDish(@Body Dish newDish);
    @Headers({
            "accept: */*",
            "Content-Type: application/json"
    })
    @POST("api/Cart/add")
    Call<com.example.cnpmnc_appfood.AddToCartResponse> addToCart(@Body AddToCartRequest request);
    @Headers({
            "accept: text/plain"
    })
    @GET("api/Cart/{username}")
    Call<com.example.cnpmnc_appfood.CartApiResponse> getCartDetails(@Path("username") String username);
    @Headers({
            "accept: text/plain"
    })
    @GET("api/Auth/account/{username}")
    Call<com.example.cnpmnc_appfood.UserProfileResponse> getUserProfile(@Path("username") String username);

    // THÊM PHƯƠNG THỨC NÀY: GET Order History
    @Headers({
            "accept: text/plain"
    })
    @GET("api/Cart/history/{username}")
    Call<List<com.example.cnpmnc_appfood.OrderHistoryResponse>> getOrderHistory(@Path("username") String username);
    @Headers({
            "accept: text/plain",
            "Content-Type: application/json"
    })
    @POST("api/Auth/change-password")
    Call<com.example.cnpmnc_appfood.ChangePasswordResponse> changePassword(@Body com.example.cnpmnc_appfood.ChangePasswordRequest request);
    @Headers({
            "accept: */*" // Hoặc "text/plain" nếu API trả về text
    })
    @GET("api/VnPay/CreatePayment2") // API dùng GET với Query Parameter
    Call<com.example.cnpmnc_appfood.VnPayCreatePaymentResponse> createVnPayPayment(@Query("orderId") int orderId,@Query("source") String source);
    @POST("api/Cart/checkout")
    Call<CheckoutApiResponse> checkoutCart(@Query("username") String username);
}