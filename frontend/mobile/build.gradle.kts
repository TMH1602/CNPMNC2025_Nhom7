// Top-level build file where you can add configuration options common to all sub-projects/modules.
plugins {
    // This line already declares your Android Application plugin.
    // The version is typically managed in your version catalog (libs.versions.toml)
    alias(libs.plugins.android.application) apply false
    // If you need to explicitly declare the Kotlin Gradle Plugin here,
    // (though it's often applied in the module-level build.gradle.kts)
    // you would do it like this, ensuring the version is in your catalog:
    // alias(libs.plugins.kotlin.android) apply false
}

// You can usually remove the dependencies block with classpath if
// your plugins are managed through the plugins {} block above.
/*
dependencies {
    // These are likely redundant if you're using the plugins {} block correctly.
    // classpath("com.android.tools.build:gradle:8.0.2")
    // classpath("org.jetbrains.kotlin:kotlin-gradle-plugin:1.9.0")
}
*/

