# ASM5 BookDB Design

## Muc tieu

Thay han domain hien tai `Grade/Student` bang domain `Book/Category` de xay dung mot trang quan li sach theo yeu cau ASM5. Ung dung phai bam sat cac anh mau ve cau truc chuc nang, nhung giao dien phai tuan thu `DESIGN.md` cua repo thay vi sao chep truc tiep bo cuc mac dinh trong slide.

## Pham vi

Trong vong lam viec nay, ung dung se:

- Loai bo luong chuc nang `Grade/Student` khoi luong MVC chinh.
- Chuyen `ApplicationDbContext` sang quan ly `Book` va `Category`.
- Tao lai migration va seed du lieu BookDB.
- Xay dung CRUD cho `Book`.
- Hien thi danh sach `Category` o sidebar ben trai kem so luong sach theo tung chu de.
- Ho tro loc sach theo `Category` tren trang danh sach.
- Ho tro upload anh bia khi tao va sua sach, kiem tra o ca client va server.
- Tao trang chi tiet sach theo bo cuc 2 cot, trong do layout va visual treatment phai theo token trong `DESIGN.md`.

Ngoai pham vi vong nay:

- Khong xay CRUD rieng cho `Category`.
- Khong them dang nhap, gio hang, dat mua thuc su; cac nut o trang chi tiet chi dong vai tro UI.
- Khong doi Docker SQL Server hien co, chi tai su dung no cho schema moi.

## Rang buoc thiet ke

Giao dien phai theo he thong style trong `DESIGN.md`:

- Theme light, nen sang, border dam, shadow offset nhe.
- Accent chinh la `#a3e635` cho CTA va trang thai active.
- Typography uu tien `Satoshi`, fallback system-ui.
- Khong de Bootstrap mac dinh chi phoi ket qua cuoi.
- Card, input, button, sidebar va detail pane phai dung token mau, radius, shadow va spacing da duoc dinh nghia.

Anh mau la nguon tham chieu cho thong tin, luong su dung va bo cuc tong quat. `DESIGN.md` la nguon quyet dinh cho visual language cuoi cung.

## Kien truc tong the

Ung dung tiep tuc dung ASP.NET Core MVC + EF Core + SQL Server.

Thanh phan chinh:

- `Models/Category.cs`: model chu de.
- `Models/Book.cs`: model sach.
- `Models/ApplicationDbContext.cs`: `DbSet<Book>`, `DbSet<Category>` va cau hinh quan he/seed neu can.
- `Controllers/BookController.cs`: chiu trach nhiem cho danh sach, chi tiet, tao, sua, xoa.
- `ViewModels/BookIndexViewModel.cs`: truyen du lieu cho man danh sach, gom books, categories, selectedCategoryId.
- `Views/Book/*`: cac Razor view cho CRUD va trang chi tiet.
- `wwwroot/Content/ImagesBook`: noi luu anh bia upload.
- `wwwroot/css/site.css` hoac css lien quan: dua token tu `DESIGN.md` vao giao dien thuc te.

## Mo hinh du lieu

### Category

- `CategoryId`: int, khoa chinh.
- `CategoryName`: string, bat buoc.
- `Books`: collection navigation.

### Book

- `Id`: int, khoa chinh.
- `Title`: string, bat buoc, toi da 150 ky tu.
- `Author`: string, bat buoc, toi da 150 ky tu.
- `Price`: decimal(18, 0) hoac decimal(18, 2). Khuyen dung `decimal(18, 0)` de gan voi anh mau VND nguyen.
- `Description`: string, bat buoc.
- `Image`: string, luu ten file hoac duong dan tuong doi.
- `CategoryId`: int, khoa ngoai.
- `Category`: navigation.

Quan he:

- Mot `Category` co nhieu `Book`.
- Mot `Book` thuoc duy nhat mot `Category`.

## Du lieu seed

Seed ban dau se tao it nhat 3 chu de:

- `Cuoc song`
- `Lap trinh`
- `Suc khoe`

Seed sach mau toi thieu 4 cuon de tai hien anh:

- `Cho toi xin mot ve di tuoi tho`
- `Lap trinh C#`
- `Java Fundamentals`
- `Cuoc song rat giong cuoc doi`

Moi sach phai co:

- tieu de
- tac gia
- gia
- mo ta ngan
- anh bia mau
- `CategoryId`

Anh seed phai duoc dat san trong `wwwroot/Content/ImagesBook` de trang danh sach va chi tiet hien thi ngay sau khi migrate.

## Man hinh va luong chuc nang

### 1. Layout chung

Layout se duoc chinh lai de hop voi `DESIGN.md`:

- Navbar sang, border ro, bong offset nhe.
- Brand text o trai.
- Cac muc dieu huong toi thieu: `Trang chu` va `Them moi`.
- Khung noi dung can giua, max width hop ly.
- Footer nhe, dung token `Pale Ash` neu can tach section.

### 2. Trang danh sach sach `/Book/Index`

Muc tieu:

- Tai hien bo cuc sidebar trai + grid sach ben phai nhu anh mau.
- Sidebar hien danh sach chu de va so luong sach theo tung chu de.
- Khi chon mot chu de, danh sach sach duoc loc theo `CategoryId`.

Cau truc:

- Cot trai: `Chu de`
  - danh sach category click duoc
  - item dang chon duoc highlight bang accent xanh
  - moi item hien `TenChuDe (count)`
- Cot phai:
  - thanh tieu de hoac action row co nut `Them moi`
  - grid card sach 2-3 cot tuy viewport
  - moi card hien `Title`, `Author`, `CategoryName`, anh bia, link `Edit`, nut `Chi tiet`, nut `Xoa`

Hanh vi:

- Khong co category filter: hien tat ca sach.
- Co category filter: hien sach thuoc chu de duoc chon.
- Neu khong co sach: hien empty state ro rang.

### 3. Trang chi tiet `/Book/Details/{id}`

Muc tieu:

- Bam bo cuc 2 cot trong anh mau.
- Van trung thanh voi token trong `DESIGN.md`.

Cot trai:

- anh bia lon cua sach trong card co border ro rang.

Cot phai:

- tieu de sach noi bat
- tac gia
- nha xuat ban neu muon them o seed, neu khong co thi bo qua de tranh scope creep
- mo ta
- gia sach nhan manh bang mau noi bat
- cum CTA gom `Them vao gio hang` va `Mua ngay` o muc UI-only

Ghi chu:

- Nut CTA khong can backend gio hang.
- Muc tieu la tai hien man hinh chi tiet nhu anh, khong mo rong logic mua hang.

### 4. Trang tao sach `/Book/Create`

Form gom:

- Title
- Author
- Price
- Description
- CategoryId (dropdown)
- ImageFile (upload)

Yeu cau upload:

- Client: `accept="image/*"`, thong bao khi chua chon file neu file la bat buoc.
- Server: chi nhan file anh hop le, sinh ten file an toan, luu vao `wwwroot/Content/ImagesBook`.

Ket qua:

- Tao thanh cong -> quay ve `Index`.
- That bai validation -> giu lai form va hien loi.

### 5. Trang sua sach `/Book/Edit/{id}`

Form tuong tu `Create`, nhung:

- hien anh hien tai
- co the giu nguyen anh cu neu nguoi dung khong upload anh moi
- neu upload anh moi, ghi de logic tham chieu sang anh moi, co the giu file cu de tranh phat sinh scope xoa file o vong dau

### 6. Xoa sach `/Book/Delete/{id}`

Phien ban don gian:

- co trang xac nhan xoa hoac nut xoa + confirm don gian
- sau khi xoa -> quay ve `Index`

## Xu ly du lieu va service rules

### Loc theo chu de

`BookController.Index(int? categoryId)`:

- query `Categories` kem count sach
- query `Books` kem `Category`
- neu `categoryId` co gia tri thi them filter
- sap xep sach theo `Id` giam dan hoac `Title`; khuyen dung `Id` de on dinh voi du lieu seed

### Upload anh

Quy tac:

- Thu muc dich: `wwwroot/Content/ImagesBook`
- Ten file luu thuc te: `{guid}_{safeOriginalName}`
- Gia tri luu DB: duong dan tuong doi de render trong Razor, vi du `Content/ImagesBook/abc.jpg`
- Neu upload that bai -> form bao loi, khong tao/sua DB dang do

### Validation

- `Title`, `Author`, `Description`, `CategoryId` bat buoc.
- `Price` phai >= 0.
- `Image` bat buoc o `Create`.
- `Edit` cho phep khong upload anh moi.

## Dinh huong giao dien theo DESIGN.md

### Sidebar

- Nen trang hoac pale ash.
- Border dam `#171717`.
- Muc dang chon dung accent green.
- Shadow nhe 1-2 px.

### Card sach

- Nen trang, border ro, shadow offset nhe.
- Tieu de dam, thong tin metadata nho hon.
- Anh bia co khung on dinh, ti le nhat quan.
- Hanh dong `Chi tiet`, `Xoa`, `Edit` ro rang, khong dung button Bootstrap mac dinh.

### Form Create/Edit

- Input border xam dung token.
- Nut primary dung accent green.
- Nut secondary dung ghost style.
- Khoang cach dung scale 16/24/32 px.

### Details page

- Tieu de lon va day.
- Gia sach noi bat, co the dung do/den ket hop accent nhung van phai ton trong palette tong the.
- Khoi thong tin dat trong card surface sach se.

## Rui ro va cach khong che

- Migration cu `Grade/Student` dang ton tai: can thay bang schema moi theo cach sach, tranh tron domain cu vao bai ASM5.
- Anh seed khong co san: can them file anh placeholder hoac asset that su vao `wwwroot/Content/ImagesBook`.
- Layout hien tai dang don gian: can chinh lai nhung khong nen doi vuot pham vi BookDB.
- Warning nullable trong model hien tai: nen xu ly khi chuyen sang model moi de giam no ky thuat.

## Ke hoach xac minh sau khi implement

- `dotnet build` pass.
- Chay migration tao schema BookDB moi va seed thanh cong.
- DBeaver thay duoc bang `Books` va `Categories` cung du lieu seed.
- `curl` hoac truy cap trinh duyet xac nhan:
  - `/Book`
  - `/Book/Details/{id}`
  - `/Book/Create`
  - `/Book/Edit/{id}`
- Kiem tra tao sach moi co upload anh.
- Kiem tra sua sach khong doi anh va co doi anh.
- Kiem tra loc theo category va hien so luong sach tren sidebar.

## Quyet dinh thiet ke

- Domain `Grade/Student` se bi thay the khoi luong chinh cua ung dung.
- `Category` chi dung de phan loai va loc, khong co CRUD rieng trong vong dau.
- `Book` la aggregate trung tam cho toan bo man hinh ASM5.
- Visual language uu tien `DESIGN.md`; anh slide chi quy dinh cau truc thong tin va luong su dung.
