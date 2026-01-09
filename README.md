# QL_NhaThuoc - Hệ thống Quản lý Nhà Thuốc

## Yêu cầu hệ thống

- .NET 6.0 SDK trở lên
- SQL Server 2019 trở lên
- Visual Studio 2022 hoặc VS Code

---

## Hướng dẫn Clone dự án

### Bước 1: Clone repository

```bash
git clone https://github.com/nguyenketong/QL_NhaThuoc.git
```

### Bước 2: Di chuyển vào thư mục dự án

```bash
cd QL_NhaThuoc
```

---

## Hướng dẫn tạo Database "QL_NhaThuoc"

### Sử dụng SQL Server Management Studio (SSMS)

1. Mở **SQL Server Management Studio**
2. Kết nối đến SQL Server của bạn
3. Click chuột phải vào **Databases** → **New Database...**
4. Nhập tên database: `QL_NhaThuoc`
5. Click **OK**
6. Click chuột phải chọn restose:
7. Mở file `Database/ql_nhathuoc.bak`
7. Chọn file `ql_nhathuoc.bak` và nhấn **Execute** (F5)



## Cấu hình Connection String

Mở file `appsettings.json` và cập nhật connection string theo máy của bạn:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=TEN_SERVER_CUA_BAN;Database=QL_NhaThuoc;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

**Lưu ý:** Thay `TEN_SERVER_CUA_BAN` bằng tên SQL Server instance của bạn (ví dụ: `localhost`, `.\SQLEXPRESS`, `DESKTOP-ABC\SQLEXPRESS`)

---

## Cấu trúc dự án

```
QL_NhaThuoc/
├── Areas/Admin/          # Khu vực quản trị
├── Controllers/          # Các controller
├── Data/                 # DbContext
├── Database/             # File SQL tạo database
├── Models/               # Các model/entity
├── Services/             # Các service
├── ViewModels/           # View models
├── Views/                # Giao diện
└── wwwroot/              # Static files (CSS, JS, Images)
```
