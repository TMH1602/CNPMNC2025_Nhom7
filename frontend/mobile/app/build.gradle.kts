plugins {
    alias(libs.plugins.android.application)
}

android {
    namespace = "com.example.cnpmnc_appfood"
    compileSdk = 36

    defaultConfig {
        applicationId = "com.example.cnpmnc_appfood"
        minSdk = 24
        targetSdk = 36
        versionCode = 1
        versionName = "1.0"

        testInstrumentationRunner = "androidx.test.runner.AndroidJUnitRunner"
    }

    buildTypes {
        release {
            isMinifyEnabled = false
            proguardFiles(
                getDefaultProguardFile("proguard-android-optimize.txt"),
                "proguard-rules.pro"
            )
        }
    }
    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_11
        targetCompatibility = JavaVersion.VERSION_11
    }
}

dependencies {
    implementation("com.cloudinary:cloudinary-android:2.4.0")
// Hoặc kiểm tra phiên bản mới nhất
    // OkHttp & Logging Interceptor (Dùng CÙNG phiên bản)
    implementation("com.squareup.okhttp3:okhttp:4.11.0")
    implementation("com.squareup.okhttp3:logging-interceptor:4.11.0") // Giữ 4.11.0

    // Retrofit (Chỉ cần 1 lần)
    implementation("com.squareup.retrofit2:retrofit:2.9.0")

    // Converters (Chỉ cần 1 lần mỗi loại)
    implementation("com.squareup.retrofit2:converter-gson:2.9.0")
    implementation("com.squareup.retrofit2:converter-scalars:2.9.0") // Giữ lại nếu bạn cần gọi API trả về text

    // Các thư viện khác của bạn (Giữ nguyên)
    implementation(libs.glide)
    annotationProcessor(libs.glide.compiler)
    implementation(libs.appcompat)
    implementation(libs.material)
    implementation(libs.activity)
    implementation(libs.constraintlayout)
    testImplementation(libs.junit)
    androidTestImplementation(libs.ext.junit)
    androidTestImplementation(libs.espresso.core)
}