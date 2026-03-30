# 📘 Hướng Dẫn & Khái Niệm Kiểm Thử Đơn Vị (Unit Testing)

Tài liệu này cung cấp các kiến thức cơ bản về Kiểm thử đơn vị (Unit Testing) theo yêu cầu của đồ án, tập trung vào các phương pháp Hộp trắng, Hộp đen và Hộp xám.

---

## 1. Kiểm thử Đơn vị (Unit Testing) là gì?
Kiểm thử đơn vị là mức độ kiểm thử phần mềm thấp nhất, trong đó các phần nhỏ nhất của mã nguồn (hàm, phương thức, lớp) được kiểm tra một cách độc lập để đảm bảo chúng hoạt động đúng như mong đợi.

**Mục tiêu:**
- Phát hiện lỗi sớm trong quá trình phát triển.
- Đảm bảo mã nguồn dễ bảo trì và mở rộng.
- Cung cấp tài liệu sống về cách các hàm/phương thức hoạt động.

---

## 2. Các Phương Pháp Thực Hiện

### 2.1. Kiểm thử Hộp Trắng (White Box Testing)
- **Định nghĩa:** Người kiểm thử biết rõ cấu trúc mã nguồn bên trong của đơn vị đang test.
- **Cách tiếp cận:** Kiểm tra các luồng logic (if/else, switch case, vòng lặp) để đảm bảo mọi nhánh của code đều được thực thi (Code Coverage).
- **Ví dụ trong .NET:** Kiểm tra hàm `Login` trong `AccountController` để đảm bảo logic phân quyền Admin/User hoạt động đúng.

### 2.2. Kiểm thử Hộp Đen (Black Box Testing)
- **Định nghĩa:** Người kiểm thử không quan tâm đến mã nguồn bên trong, chỉ quan tâm đến **Đầu vào (Input)** và **Đầu ra (Output)** dựa trên Requirement.
- **Cách tiếp cận:** Kiểm tra các trường hợp biên (Boundary Value), phân vùng tương đương (Equivalence Partitioning).
- **Ví dụ trong .NET:** Nhập Email không đúng định dạng và kiểm tra xem hệ thống có trả về lỗi "Email không hợp lệ" hay không mà không cần biết code xử lý Regex như thế nào.

### 2.3. Kiểm thử Hộp Xám (Gray Box Testing)
- **Định nghĩa:** Là sự kết hợp giữa Hộp Trắng và Hộp Đen. Người kiểm thử có kiến thức hạn chế về cấu trúc bên trong (ví dụ: biết cấu trúc database hoặc API) nhưng thực hiện test từ bên ngoài.
- **Ví dụ trong .NET:** Kiểm tra tính năng đặt hàng, sau khi thực hiện xong thì vào Database kiểm tra xem bản ghi đã được tạo đúng hay chưa.

---

## 3. Công Cụ & Framework

### 🛠️ xUnit (C#/.NET)
Dù yêu cầu trong hình có nhắc đến **JUnit 5** (dành cho Java), nhưng vì dự án của chúng ta được xây dựng trên **ASP.NET Core (C#)**, chúng ta sẽ sử dụng **xUnit**.
- **xUnit** là công cụ kiểm thử hiện đại nhất cho .NET, có chức năng tương đương hoàn toàn với JUnit 5.
- Cấu trúc một Unit Test gồm 3 bước: **Arrange (Chuẩn bị) -> Act (Thực hiện) -> Assert (Kiểm chứng)**.

---

## 4. Ví Dụ Unit Test Cơ Bản (Mẫu)

```csharp
[Fact]
public void CalculateTotalPrice_ShouldReturnCorrectValue()
{
    // Arrange: Chuẩn bị dữ liệu
    var price = 100000;
    var quantity = 2;
    var expected = 200000;

    // Act: Thực hiện hàm cần test (giả định hàm này nằm trong service)
    var result = OrderService.CalculateTotal(price, quantity);

    // Assert: Kiểm chứng kết quả
    Assert.Equal(expected, result);
}
```

---

## 5. Lưu ý về Bugzilla
Theo yêu cầu mới nhất, phần quản lý bug trên **Bugzilla** sẽ được bỏ qua để tập trung sâu hơn vào việc triển khai bộ Selenium UI/UX và Unit Testing thực tế.

---

## 🏁 Kết luận
Việc kết hợp cả **Unit Testing** (để kiểm tra logic bên trong) và **Selenium Automation Testing** (để kiểm tra trải nghiệm người dùng) giúp dự án đạt độ tin cậy cao nhất, đáp ứng được tiêu chuẩn 25-40 trang báo cáo kỹ thuật.
