package com.example.cnpmnc_appfood;

import android.os.Bundle;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.Menu;
import android.view.MenuInflater;
import android.view.MenuItem;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ListView;
import android.widget.Toast;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;
import java.util.List;

import com.example.cnpmnc_appfood.FoodDetailsFragment; // FIX: Lỗi không tìm thấy symbol

public class HomeFragment extends Fragment implements
        DishAdapter.OnDishClickListener,
        DishRepository.OnDishDataChangeListener,
        DishAdapter.OnCartClickListener {

    private ListView lvDishListHome;
    private DishAdapter adapter;
    private DishRepository dishRepository;

    @Override
    public void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        // Kích hoạt menu trong Toolbar (để hiển thị nút Giỏ hàng)
        setHasOptionsMenu(true);
    }

    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container,
                             @Nullable Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_home, container, false);

        lvDishListHome = view.findViewById(R.id.lvDishListHome);

        dishRepository = DishRepository.getInstance();

        List<Dish> activeDishes = dishRepository.getActiveDishes();

        // Khởi tạo Adapter với 5 tham số
        adapter = new DishAdapter(
                requireContext(),
                R.layout.item_dish,
                activeDishes,
                this,
                this
        );

        lvDishListHome.setAdapter(adapter);
        dishRepository.addListener(this);
        dishRepository.loadDishesFromServer();

        return view;
    }

    // --- LOGIC TOOLBAR/MENU GIỎ HÀNG ---

    @Override
    public void onCreateOptionsMenu(@NonNull Menu menu, @NonNull MenuInflater inflater) {
        inflater.inflate(R.menu.menu_cart, menu); // Đảm bảo bạn có file res/menu/menu_cart.xml
        super.onCreateOptionsMenu(menu, inflater);
    }

    @Override
    public boolean onOptionsItemSelected(@NonNull MenuItem item) {
        if (item.getItemId() == R.id.action_cart) {
            // Chuyển sang CartFragment khi nhấn nút Giỏ hàng
            navigateToCartFragment();
            return true;
        }
        return super.onOptionsItemSelected(item);
    }

    private void navigateToCartFragment() {
        if (getActivity() != null) {
            getActivity().getSupportFragmentManager().beginTransaction()
                    .replace(R.id.fragment_container, new CartFragment())
                    .addToBackStack("menu_to_cart")
                    .commit();
        }
    }

    // --- CÁC PHƯƠNG THỨC KHÁC ---

    @Override
    public void onDestroyView() {
        super.onDestroyView();
        if (dishRepository != null) {
            dishRepository.removeListener(this);
        }
    }

    @Override
    public void onDishDataChanged() {
        if (dishRepository != null && adapter != null) {
            List<Dish> newActiveDishes = dishRepository.getActiveDishes();
            ((DishAdapter)adapter).setDishList(newActiveDishes);
            Log.d("HomeFragment", "Tổng số món đang bán: " + newActiveDishes.size());
        }
    }

    @Override
    public void onDishClick(Dish dish) {
        FoodDetailsFragment detailsFragment = FoodDetailsFragment.newInstance(dish.getId());
        if (getActivity() != null) {
            getActivity().getSupportFragmentManager().beginTransaction()
                    .replace(R.id.fragment_container, detailsFragment)
                    .addToBackStack(null)
                    .commit();
        }
    }

    @Override
    public void onAddToCartClick(Dish dish) {
        CartManager.getInstance().addItemToCart(dish);
        Toast.makeText(getContext(), "Đã thêm " + dish.getName() + " vào giỏ hàng cục bộ.", Toast.LENGTH_SHORT).show();
    }
}