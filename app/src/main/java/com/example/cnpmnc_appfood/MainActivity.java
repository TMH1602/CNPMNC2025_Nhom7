package com.example.cnpmnc_appfood;

// ... các import khác ...
import android.os.Bundle;
import androidx.appcompat.app.AppCompatActivity;
import androidx.fragment.app.Fragment;
import com.google.android.material.bottomnavigation.BottomNavigationView;

public class MainActivity extends AppCompatActivity {

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main); // Đảm bảo activity_main có BottomNavigationView và fragment_container

        BottomNavigationView navView = findViewById(R.id.nav_view);
        navView.setOnNavigationItemSelectedListener(mOnNavigationItemSelectedListener);

        if (savedInstanceState == null) {
            loadFragment(new HomeFragment());
        }
    }

    private final BottomNavigationView.OnNavigationItemSelectedListener mOnNavigationItemSelectedListener
            = item -> {
        Fragment fragment = null;
        int itemId = item.getItemId();

        if (itemId == R.id.navigation_home) {
            fragment = new HomeFragment();
        } else if (itemId == R.id.navigation_cart) {
            fragment = new CartFragment();
        } else if (itemId == R.id.navigation_compare) {
            fragment = new ComparisonFragment();
        }
        // CẬP NHẬT: Thay thế SettingsFragment bằng DishManagementFragment
        else if (itemId == R.id.navigation_settings) {
            fragment = new DishManagementFragment(); // Mở trang thêm món ăn
        }

        if (fragment != null) {
            return loadFragment(fragment);
        }
        return false;
    };

    private boolean loadFragment(Fragment fragment) {
        if (fragment != null) {
            getSupportFragmentManager().beginTransaction()
                    .replace(R.id.fragment_container, fragment)
                    .commit();
            return true;
        }
        return false;
    }
}