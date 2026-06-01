# Auth Pages Design

Date: 2026-06-01
Project: `BaiTapWeb_Buoi5`

## Goal

Đồng bộ giao diện `Login` và `Register` với ngôn ngữ thiết kế hiện tại của website, đồng thời cho phép người dùng đăng nhập bằng `email` hoặc `username`.

## Current State

- `Register` đã có file custom trong `Areas/Identity/Pages/Account/Register.cshtml` nhưng bố cục còn rất sơ khai và chưa đồng bộ với phần catalog.
- `Login` hiện chưa có file custom trong project, nên đang rơi về giao diện mặc định của ASP.NET Core Identity.
- Dữ liệu seed tạo tài khoản admin với cả `UserName = "admin"` và `Email = "admin@qlbanhang.local"`.
- Form đăng nhập mặc định yêu cầu trường email, khiến `admin` bị chặn ngay ở validation phía client.

## Scope

Trong phạm vi thay đổi này:

1. Tạo/cập nhật trang `Login` và `Register` để dùng cùng phong cách thị giác với layout hiện tại.
2. Đổi hành vi đăng nhập từ "chỉ email" sang "email hoặc username".
3. Giữ nguyên cơ chế Identity, role `Admin`/`Member`, và flow redirect sau đăng nhập/đăng ký.

Ngoài phạm vi thay đổi này:

- Không redesign toàn bộ các trang Identity khác như `ForgotPassword`, `Manage`, `AccessDenied`.
- Không thay đổi seed data ngoài việc bảo đảm tài khoản hiện có dùng được theo cả username và email.
- Không thay đổi logic phân quyền admin.

## UX Design

### Shared Visual Direction

- Dùng cùng container width, card surface, border radius, shadow, màu chủ đạo và spacing với các khối `.auth-card`, `.form-card`, `.simple-card` đã có trong site.
- Dùng nhãn tiếng Việt nhất quán.
- Giảm cảm giác "scaffold mặc định" bằng cách thêm tiêu đề, mô tả ngắn, nhóm field rõ ràng, CTA chính nổi bật.

### Login Page

- Tiêu đề: `Đăng nhập`
- Mô tả: hướng dẫn ngắn để vào khu vực mua sắm/quản trị.
- Một field định danh duy nhất:
  - Label: `Email hoặc tên đăng nhập`
  - Placeholder gợi ý: `admin hoặc admin@qlbanhang.local`
- Field mật khẩu.
- Checkbox ghi nhớ đăng nhập.
- Nút CTA chính: `Đăng nhập`
- Link phụ: `Tạo tài khoản`
- Không hiển thị khối "Use another service to log in" vì dự án không cấu hình external login.

### Register Page

- Tiêu đề: `Tạo tài khoản`
- Mô tả ngắn giải thích đây là tài khoản thành viên.
- Các field:
  - `Tên đăng nhập`
  - `Họ và tên`
  - `Địa chỉ`
  - `Email`
  - `Mật khẩu`
  - `Xác nhận mật khẩu`
- Nút CTA chính: `Đăng ký`
- Link phụ: `Đã có tài khoản? Đăng nhập`

## Behavior Design

### Login Identifier

Form login sẽ nhận một trường duy nhất tên logic là `Login`.

Luồng xử lý:

1. Nhận giá trị người dùng nhập.
2. Nếu chuỗi có dạng email, tìm user theo email.
3. Nếu không phải email, tìm user theo username.
4. Nếu tìm thấy user, dùng `PasswordSignInAsync(user.UserName, password, rememberMe, lockoutOnFailure: false)`.
5. Nếu không tìm thấy hoặc sai mật khẩu, trả thông báo lỗi chung để tránh lộ thông tin tài khoản.

### Validation

- Không dùng attribute `EmailAddress` cho field đăng nhập.
- Chỉ yêu cầu field không được rỗng.
- Register vẫn giữ validation email thật cho trường `Email`.

## Technical Design

### Files To Add Or Update

- Add: `Areas/Identity/Pages/Account/Login.cshtml`
- Add: `Areas/Identity/Pages/Account/Login.cshtml.cs`
- Update: `Areas/Identity/Pages/Account/Register.cshtml`
- Update: `Areas/Identity/Pages/Account/Register.cshtml.cs`
- Update: `wwwroot/css/site.css`

### CSS Strategy

- Tái sử dụng class card hiện có thay vì tạo một theme riêng cho Identity.
- Bổ sung một nhóm class auth-specific nhẹ:
  - `.auth-shell`
  - `.auth-card`
  - `.auth-form`
  - `.auth-actions`
  - `.auth-meta`
- Bảo đảm responsive tốt trên mobile.

### Identity Integration

- Giữ `RegisterModel` hiện có, chỉ đổi text hiển thị và cấu trúc markup nếu cần.
- Tạo `LoginModel` custom trong `Areas/Identity/Pages/Account` để override UI mặc định của Identity.
- Không đổi route mặc định của Identity; URL vẫn là `/Identity/Account/Login` và `/Identity/Account/Register`.

## Test Strategy

- Viết test cho hành vi đăng nhập bằng username và email trước khi sửa production code.
- Kiểm tra thủ công:
  - Đăng nhập bằng `admin`
  - Đăng nhập bằng `admin@qlbanhang.local`
  - Đăng nhập sai mật khẩu
  - Mở trang `Register` và xác nhận giao diện đồng bộ với site
  - Sau đăng nhập admin, truy cập được `/Admin/Product` và `/Admin/Category`

## Risks

- Nếu `Login` custom không copy đủ flow mặc định của Identity, có thể ảnh hưởng return URL.
- CSS auth mới nếu viết quá rộng có thể đè lên form khác trong site.
- Nếu message validation không được Việt hóa nhất quán, UX sẽ còn chắp vá.

## Acceptance Criteria

1. Người dùng có thể đăng nhập bằng cả `admin` và `admin@qlbanhang.local`.
2. Không còn lỗi validation "Email field is not a valid e-mail address" khi nhập username ở trang login.
3. `Login` và `Register` nhìn đồng bộ với phong cách hiện tại của website.
4. Sau đăng nhập admin, truy cập khu vực admin không bị chặn quyền.
