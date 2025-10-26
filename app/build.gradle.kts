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
    // Retrofit
    implementation("com.squareup.retrofit2:retrofit:2.9.0")
// Gson Converter (cho JSON request)
    implementation("com.squareup.retrofit2:converter-gson:2.9.0")
// Scalars Converter (cho text/plain response)
    implementation("com.squareup.retrofit2:converter-scalars:2.9.0")
// OkHttp (cần thiết để xử lý SSL)
    implementation("com.squareup.okhttp3:okhttp:4.9.3")
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