# Homepage Redesign And Slider Admin Design

Date: 2026-06-01
Project: `BaiTapWeb_Buoi5`

## Goal

Redesign lại homepage theo design system `Lumina Retail`, tách riêng homepage và trang sản phẩm, đồng thời bổ sung một module `Slider` riêng trong khu vực admin để quản lý ảnh carousel thật từ backend.

## Current State

- Homepage hiện tại đang bị trùng vai trò với trang sản phẩm vì `Products/Index` vừa làm storefront landing vừa làm catalog.
- Hệ thống đã có storefront, auth pages, admin product/category CRUD.
- Chưa có cơ chế quản lý slider trong database.
- Chưa có admin menu riêng cho slider.
- Một phần dữ liệu seed sản phẩm đang dùng SVG/minh họa cũ, nhưng yêu cầu mới là phần hero slider tuyệt đối phải dùng ảnh thật từ backend upload, không hard-code SVG minh họa cho carousel.

## Scope

Trong phạm vi thay đổi này:

1. Redesign homepage storefront theo visual system `Lumina Retail`.
2. Tách homepage khỏi product catalog.
3. `Home/Index` trở thành landing storefront nhiều section.
4. `Products/Index` trở thành catalog riêng theo layout giống mockup đã cung cấp.
5. Tạo entity và database support cho slider ảnh.
6. Tạo module admin `Slider` riêng để quản lý ảnh slider.
7. Homepage hero carousel phải lấy dữ liệu từ slider trong database.

Ngoài phạm vi thay đổi này:

- Không redesign toàn bộ admin product/category trong task này.
- Không thêm text/CTA động từ backend cho slider.
- Không thêm lịch hiển thị, bật/tắt, hay analytics cho slider.
- Không thiết kế lại toàn bộ tất cả trang khác ngoài homepage, product catalog, và phần cần thiết để gắn slider admin vào navigation.

## UX Design

### Homepage Structure

Homepage mới tại `Home/Index` sẽ có các section chính:

1. **Sticky header**
   - Logo `QLBanHang`
   - Primary nav
   - Search box tinh gọn
   - Auth actions

2. **Hero carousel**
   - Ảnh nền full-width từ slider backend
   - Overlay nhẹ để nội dung dễ đọc
   - Headline, supporting text, CTA là nội dung trình bày của homepage
   - Dot navigation / arrow controls

3. **Service benefits**
   - 3 card ngang: giao hàng, bảo hành, hỗ trợ
   - Icon nhẹ, nền sáng, shadow mềm

4. **Featured categories**
   - Grid bất đối xứng hoặc masonry nhẹ
   - Dùng ảnh category/product hiện có hoặc ảnh phù hợp trong dữ liệu

5. **Latest products**
   - Product card theo design system mới
   - Badge `New`
   - Hover elevation nhẹ

6. **Promotional banner**
   - Section visual lớn, dùng nền ảnh hoặc gradient cao cấp

7. **Newsletter**
   - Input email + CTA

8. **Footer**
   - Đồng bộ với system mới, gọn hơn, sạch hơn

### Product Catalog Structure

Trang `Products/Index` sẽ là một catalog riêng, không còn là homepage.

Catalog mới sẽ có:

1. **Compact hero / intro band**
   - Headline ngắn
   - Subtext giới thiệu
   - CTA phụ nếu cần

2. **Catalog layout 2 cột**
   - Sidebar trái
   - Product grid bên phải

3. **Sidebar filters**
   - Category list
   - Active category state
   - Price range UI block

4. **Catalog top bar**
   - Số lượng sản phẩm hiển thị
   - Nút đổi layout/view dạng icon

5. **Product grid**
   - 3 cột desktop
   - Card sản phẩm sáng, sạch, premium
   - Badge `New`
   - Giá coral nổi bật
   - CTA `Chi tiết`

6. **Pagination**
   - UI pagination ở cuối trang
   - Có thể là pagination trình bày trước, chưa cần server-side paging thật trong phạm vi đầu

### Visual Direction

- Bám theo palette `Lumina Retail`
- Primary coral: `#ba0036` / nhấn hover `#E00B41`
- Background sáng: off-white, cream, white layered surfaces
- Typography:
  - `Montserrat` cho headline
  - `Inter` cho body/UI
- Cảm giác tổng thể:
  - premium ecommerce
  - airy
  - clean
  - tactile
  - editorial nhưng vẫn bán hàng

## Slider Admin Design

### Admin Slider Module

Module `Slider` sẽ là một mục riêng trong admin navigation.

Các màn hình:

1. **Slider index**
   - Danh sách ảnh slider
   - Preview thumbnail
   - Display order
   - Nút thêm mới
   - Nút xóa
   - Form cập nhật thứ tự

2. **Slider create**
   - Upload 1 ảnh
   - Nhập `DisplayOrder`
   - Preview validation lỗi nếu thiếu ảnh

3. **Slider delete**
   - Confirm xóa
   - Hiển thị preview ảnh

### Admin Behavior

- Admin chỉ quản lý ảnh và thứ tự hiển thị.
- Không có title, subtitle, CTA, url trong slider data.
- Homepage đọc slider theo `DisplayOrder ASC`.
- Nếu không có slider nào, homepage dùng fallback layout tĩnh không crash.

## Data Design

### New Entity

Tạo entity `SliderImage` với các field:

- `Id`
- `ImageUrl`
- `DisplayOrder`
- `CreatedAt`

### Storage Strategy

- Metadata slider lưu trong database
- File ảnh lưu trong `wwwroot/uploads/sliders`
- Tên file chuẩn hóa bằng GUID + original safe filename

### DbContext

`ApplicationDbContext` sẽ có:

- `DbSet<SliderImage> SliderImages`

## Technical Design

### Files Expected To Be Added Or Updated

- Add model `Models/SliderImage.cs`
- Add migration for slider table
- Update `Models/ApplicationDbContext.cs`
- Add repository interface and implementation cho slider
- Update `Program.cs` để register repository
- Add admin controller `Areas/Admin/Controllers/SliderController.cs`
- Add admin views `Areas/Admin/Views/Slider/*`
- Update `Areas/Admin/Views/Shared/_AdminLayout.cshtml` để có menu `Slider`
- Update `HomeController` để homepage thực sự nằm ở `Home/Index`
- Add/update homepage view model để load slider + products + categories
- Replace homepage view with new landing-page composition
- Redesign `Products/Index` thành product catalog riêng
- Update `Products/Category` để dùng cùng visual language với catalog mới
- Update `wwwroot/css/site.css` theo `Lumina Retail`
- Optional: seed initial slider rows nếu có ảnh phù hợp cục bộ; nếu không có thì để admin tự upload

### Reuse Strategy

- Tái sử dụng logic lưu file từ `ProductController` làm pattern cho upload slider.
- Tái sử dụng admin table/form style hiện tại, nhưng mở rộng theo design system mới nếu cần cho riêng trang slider.

## Test Strategy

- Test repository/controller behavior cho slider upload/order/delete nếu có test harness phù hợp.
- Manual verification:
  - Upload ảnh slider từ admin
  - Ảnh xuất hiện ở homepage hero carousel tại `Home/Index`
  - Thay đổi `DisplayOrder` phản ánh đúng thứ tự hiển thị
  - Xóa ảnh slider làm homepage cập nhật đúng
  - Homepage mới responsive ở desktop/mobile
  - Trang `Sản phẩm` không còn giống homepage
  - `Products/Index` hiển thị catalog riêng giống mockup

## Risks

- Upload ảnh nhưng không có cleanup file khi xóa record sẽ để lại orphan files.
- Nếu không tách rõ `Home/Index` và `Products/Index`, người dùng sẽ tiếp tục bị lẫn giữa homepage và catalog.
- Nếu homepage phụ thuộc hoàn toàn vào slider mà DB chưa có dữ liệu, UI có thể rỗng; cần fallback state rõ.

## Acceptance Criteria

1. Homepage mới bám đúng tinh thần `Lumina Retail`.
2. `Home/Index` là landing page riêng.
3. `Products/Index` là product catalog riêng theo mockup.
4. Hero carousel dùng ảnh thật lấy từ backend slider data.
5. Admin có menu `Slider` riêng và quản lý được ảnh slider.
6. Admin upload/xóa/sắp xếp ảnh slider được.
7. Không dùng SVG minh họa hard-code cho hero carousel.
8. Homepage vẫn hoạt động khi slider chưa có dữ liệu.
