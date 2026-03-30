# 📊 Báo Cáo Kiểm Thử Toàn Diện UI/UX & Unit Testing - Spare Parts Management

Tài liệu này chi tiết các kịch bản kiểm thử (Test Cases) đã được triển khai sử dụng **Selenium WebDriver**, **xUnit** để đảm bảo chất lượng hệ thống Quản lý Phụ tùng.

---

## 🏗️ Cấu Trúc Kiểm Thử (Testing Architecture)

Hệ thống được thiết kế theo mô hình **BaseTest**, hỗ trợ tốt nhất cho việc chạy ổn định và dễ dàng mở rộng:
- **Tự động hóa trình duyệt:** Sử dụng Chrome Headless cho CI/CD và Mobile View simulation.
- **Xử lý Session:** Tự động đăng nhập/đăng xuất cho các quyền Admin, User để tiết kiệm thời gian test.
- **Tương tác Frontend:** Các phương thức `JsClick` giúp click chính xác qua các lớp UI mỏng như Spinner hay Overlay.

---

## 📋 1. Danh Sách Kịch Bản Kiểm Thử UI/UX (Selenium Automation)

Bộ test hiện tại đã được mở rộng lên **12+ kịch bản cụ thể** phân vào 4 vùng chức năng:

### 1.1 Xác thực & Phân quyền (Security & Auth)
- **[Auth-01] Đăng nhập Admin:** Điều hướng thẳng vào `/Admin/Product` khi đăng nhập thành công.
- **[Auth-02] Đăng nhập User:** Chuyển hướng về `/Home/Index`.
- **[Auth-03] Validation Form Đăng ký:** Kiểm tra thông báo lỗi khi xác nhận mật khẩu không khớp hoặc email sai định dạng.

### 1.2 Trải nghiệm Khách hàng (User Journey)
- **[Public-01] Landing Page UX:** Kiểm tra tính responsive, menu và banner hiển thị đúng màu thương hiệu.
- **[Public-02] Tìm kiếm Thông minh:** Giả lập nhập từ khóa "Lọc gió", kiểm tra URL và kết quả đổ ra đúng mục tiêu.
- **[Public-03] Lọc Giá Nâng cao:** Kiểm tra tham số `minPrice` có lọc chính xác các sản phẩm đắt tiền hay không.
- **[Public-04] Luồng Giỏ hàng:** Thêm nhiều mặt hàng, kiểm tra Badge số lượng cập nhật ngay lập tức mà không cần F5 trang.

### 1.3 Quy trình Thanh toán (Checkout Journey)
- **[Checkout-01] Hành trình Đặt hàng:** Từ Giỏ hàng -> Điền thông tin giao hàng -> Bước cuối cùng xác nhận.
- **[Checkout-02] Kiểm tra dữ liệu Form:** Đảm bảo các trường Họ tên, Địa chỉ, SĐT bắt buộc phải có thông tin trước khi nhấn "Đặt hàng".

### 1.4 Quản trị Hệ thống (Admin Operations)
- **[Admin-01] Dashboard UI:** Kiểm tra các thành phần Sidebar, Header và Breadcrumbs.
- **[Admin-02] CRUD Toàn diện:** 
    - Tạo mới sản phẩm với Guid ngẫu nhiên.
    - Chỉnh sửa thông tin giá/tên.
    - Xóa sản phẩm và xác nhận trang danh sách đã cập nhật.

---

## 📘 2. Kiểm thử Đơn vị (Unit Testing Definition)

Theo yêu cầu môn học, hệ thống áp dụng 3 kỹ thuật chính:

| Phương Pháp | Mô Tả | Ví dụ Dự án |
| :--- | :--- | :--- |
| **Hộp Trắng** | Kiểm tra cấu trúc logic, if/else bên trong code. | Kiểm tra logic tính phí Ship trong API. |
| **Hộp Đen** | Kiểm tra Input/Output dựa trên mô tả chức năng. | Gửi đúng OTP/Sai OTP để đăng ký. |
| **Hộp Xám** | Kết hợp cả hai (Biết cấu trúc nhưng test từ ngoài). | Đặt hàng và check DB có tạo OrderID không. |

> [!NOTE] 
> Chi tiết lý thuyết về các phương pháp này có tại: [UNIT_TESTING_GUIDE.md](file:///d:/SparePartsManagement/SparePartsManagement.Tests.UI/UNIT_TESTING_GUIDE.md)

---

## 🛠️ 3. Quy Trình Chạy Kiểm Thử (Technical Execution)

Để đạt kết quả chính xác, hãy chạy theo thứ tự sau:

1.  **Dừng ứng dụng cũ** (Tránh xung đột Port).
2.  **Khởi động lại Server:** (`dotnet run` tại thư mục gốc).
3.  **Thực thi bộ Test chuyên sâu:**
    ```powershell
    cd d:\SparePartsManagement\SparePartsManagement.Tests.UI
    dotnet test --logger "console;verbosity=detailed"
    ```

---

## 📈 4. Kết quả & Đánh giá (Reporting)

Toàn bộ các chuỗi hành vi người dùng từ khi bắt đầu tìm kiếm đến khi chốt đơn thành công đều được mô phỏng chính xác. Việc loại bỏ **Bugzilla** giúp tập trung tối đa vào độ bao phủ mã nguồn (Code Coverage) và trải nghiệm giao diện người dùng (UX) mượt mà nhất.

