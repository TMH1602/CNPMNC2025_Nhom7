package com.example.cnpmnc_appfood;

import android.os.Bundle;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ListView;
import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;
import java.util.List;

/**
 * HomeFragment hiển thị danh sách món ăn.
 * Nó lắng nghe thay đổi dữ liệu từ DishRepository.
 */
public class HomeFragment extends Fragment implements DishAdapter.OnDishClickListener, DishRepository.OnDishDataChangeListener {

    private ListView lvDishListHome;
    private DishAdapter adapter;

    // SỬA 1: Khai báo biến Repository
    private DishRepository dishRepository;

    @Nullable // Thêm annotation
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container, // Thêm annotations
                             @Nullable Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_home, container, false);

        lvDishListHome = view.findViewById(R.id.lvDishListHome);

        // SỬA 2: Lấy instance của Repository (thay vì gọi static)
        dishRepository = DishRepository.getInstance();

        // Lấy dữ liệu món ăn (lúc này sẽ là danh sách rỗng)
        List<Dish> allDishes = dishRepository.getAllDishes(); // SỬA 3: Gọi qua instance

        // Khởi tạo Adapter
        adapter = new DishAdapter(
                requireContext(),
                allDishes,
                this
        );

        lvDishListHome.setAdapter(adapter);

        // SỬA 4: Đăng ký listener qua instance
        dishRepository.addListener(this);

        // SỬA 5: KÍCH HOẠT TẢI DỮ LIỆU TỪ API
        // Đây là bước quan trọng nhất
        dishRepository.loadDishesFromServer();

        return view;
    }

    @Override
    public void onDestroyView() {
        super.onDestroyView();
        // SỬA 6: Hủy đăng ký listener qua instance
        if (dishRepository != null) {
            dishRepository.removeListener(this);
        }
    }

    // Triển khai phương thức xử lý click (Giữ nguyên)
    @Override
    public void onDishClick(Dish dish) {
        FoodDetailsFragment detailsFragment = FoodDetailsFragment.newInstance(dish.getId());

        // Thêm kiểm tra null cho getActivity()
        if (getActivity() != null) {
            getActivity().getSupportFragmentManager().beginTransaction()
                    .replace(R.id.fragment_container, detailsFragment)
                    .addToBackStack(null)
                    .commit();
        }
    }

    // Triển khai phương thức từ OnDishDataChangeListener (Giữ nguyên)
    @Override
    public void onDishDataChanged() {
        // Cập nhật Adapter với dữ liệu mới từ Repository
        // Thêm kiểm tra null để tăng độ ổn định
        if (dishRepository != null && adapter != null) {
            List<Dish> newDishes = dishRepository.getAllDishes(); // SỬA 7: Gọi qua instance
            adapter.clear();
            adapter.addAll(newDishes);
            adapter.notifyDataSetChanged();

            // ** CÁCH GỌI HÀM ĐẾM **
            int soMonDangBan = dishRepository.getActiveDishCount();

            // Hiển thị lên Log hoặc một TextView
            Log.d("HomeFragment", "Tổng số món đang bán: " + soMonDangBan);
        }
    }
}