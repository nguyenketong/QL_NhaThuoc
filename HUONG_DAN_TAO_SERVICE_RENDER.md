# Hướng dẫn tạo Web Service trên Render

## Bước 1: Truy cập Render Dashboard

🔗 **https://dashboard.render.com**

Đăng nhập bằng GitHub account

---

## Bước 2: Tạo Web Service mới

### 2.1. Click nút "New +"

Ở góc trên phải, click nút **New +**

### 2.2. Chọn "Web Service"

Trong menu dropdown, chọn **Web Service**

---

## Bước 3: Chọn nguồn deploy

### 3.1. Chọn "Deploy an existing image from a registry"

Bạn sẽ thấy 2 options:
- ❌ Build and deploy from a Git repository
- ✅ **Deploy an existing image from a registry** ← Chọn cái này

Click **Next**

---

## Bước 4: Nhập Image URL

### 4.1. Tìm ô "Image URL"

Bạn sẽ thấy form với ô input:

```
┌─────────────────────────────────────────────┐
│ Image URL                                   │
│ ┌─────────────────────────────────────────┐ │
│ │ docker.io/tong1603/ql-nhathuoc:latest   │ │ ← Nhập vào đây
│ └─────────────────────────────────────────┘ │
└─────────────────────────────────────────────┘
```

### 4.2. Nhập Image URL của bạn

```
docker.io/tong1603/ql-nhathuoc:latest
```

**Giải thích:**
- `docker.io` = Docker Hub registry
- `tong1603` = Docker username của bạn
- `ql-nhathuoc` = Repository name
- `latest` = Tag (phiên bản mới nhất)

---

## Bước 5: Cấu hình Service

### 5.1. Basic Information

```
┌─────────────────────────────────────────────┐
│ Name                                        │
│ ┌─────────────────────────────────────────┐ │
│ │ ql-nhathuoc                             │ │
│ └─────────────────────────────────────────┘ │
│                                             │
│ Region                                      │
│ ┌─────────────────────────────────────────┐ │
│ │ Singapore (Southeast Asia)              │ │ ← Gần VN nhất
│ └─────────────────────────────────────────┘ │
└─────────────────────────────────────────────┘
```

### 5.2. Instance Type

```
┌─────────────────────────────────────────────┐
│ Instance Type                               │
│ ○ Free                    $0/month          │ ← Chọn Free
│ ○ Starter                 $7/month          │
│ ○ Standard                $25/month         │
└─────────────────────────────────────────────┘
```

**Chọn Free** để test trước!

---

## Bước 6: Thêm Environment Variables

### 6.1. Scroll xuống phần "Environment Variables"

Click **Add Environment Variable**

### 6.2. Thêm các biến sau:

#### Variable 1: ASPNETCORE_ENVIRONMENT
```
Key:   ASPNETCORE_ENVIRONMENT
Value: Production
```

#### Variable 2: ASPNETCORE_URLS
```
Key:   ASPNETCORE_URLS
Value: http://+:8080
```

#### Variable 3: ConnectionStrings__DefaultConnection
```
Key:   ConnectionStrings__DefaultConnection
Value: Server=YOUR_SERVER;Database=QL_NhaThuoc;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;
```

⚠️ **Lưu ý:** 
- Dùng `__` (2 dấu gạch dưới) thay vì `:` trong key
- `ConnectionStrings:DefaultConnection` → `ConnectionStrings__DefaultConnection`

### 6.3. Thêm các biến khác (tùy chọn)

```
Infobip__ApiKey = YOUR_API_KEY
Infobip__SecretKey = YOUR_SECRET_KEY
BankInfo__AccountNo = YOUR_ACCOUNT_NO
```

---

## Bước 7: Advanced Settings (Tùy chọn)

### 7.1. Docker Command (để trống)

Render sẽ tự động dùng ENTRYPOINT từ Dockerfile

### 7.2. Health Check Path

```
Health Check Path: /
```

### 7.3. Auto-Deploy

```
✅ Auto-Deploy: Yes
```

Mỗi khi push code lên GitHub, Render sẽ tự động deploy!

---

## Bước 8: Create Web Service

Click nút **Create Web Service** ở cuối trang

---

## Bước 9: Lấy Service ID

### 9.1. Sau khi tạo service

Bạn sẽ được chuyển đến trang service details

### 9.2. Xem URL

URL sẽ có dạng:
```
https://dashboard.render.com/web/srv-xxxxxxxxxxxxx
                                    ↑
                                    Service ID
```

**Ví dụ:**
```
URL: https://dashboard.render.com/web/srv-ct1a2b3c4d5e6f7g8h9i
Service ID: srv-ct1a2b3c4d5e6f7g8h9i
```

### 9.3. Copy Service ID

Copy phần `srv-xxxxxxxxxxxxx` và thêm vào GitHub Secrets:

```
RENDER_SERVICE_ID = srv-xxxxxxxxxxxxx
```

---

## Bước 10: Lấy Render API Key

### 10.1. Vào Account Settings

Click vào avatar → **Account Settings**

### 10.2. Tạo API Key

1. Chọn **API Keys** (bên trái)
2. Click **Create API Key**
3. Name: `GitHub Actions CI/CD`
4. Click **Create API Key**
5. Copy key (dạng: `rnd_xxxxxxxxxxxxx`)

### 10.3. Thêm vào GitHub Secrets

```
RENDER_API_KEY = rnd_xxxxxxxxxxxxx
```

---

## Bước 11: Kiểm tra cấu hình

### 11.1. GitHub Secrets phải có:

```
✅ DOCKER_USERNAME = tong1603
✅ DOCKER_PASSWORD = dckr_pat_xxxxx
✅ RENDER_API_KEY = rnd_xxxxx
✅ RENDER_SERVICE_ID = srv-xxxxx
```

### 11.2. Render Service phải có:

```
✅ Image URL: docker.io/tong1603/ql-nhathuoc:latest
✅ Region: Singapore
✅ Instance: Free
✅ Environment Variables đã thêm
✅ Auto-Deploy: Yes
```

---

## Bước 12: Deploy lần đầu

### 12.1. Push code lên GitHub

```bash
git add .
git commit -m "Setup CI/CD"
git push origin main
```

### 12.2. Theo dõi GitHub Actions

1. Vào GitHub repository
2. Click tab **Actions**
3. Xem workflow "CI/CD Pipeline" đang chạy
4. Đợi ~5-10 phút

### 12.3. Theo dõi Render Deployment

1. Vào Render Dashboard
2. Click vào service `ql-nhathuoc`
3. Xem tab **Logs**
4. Đợi deployment hoàn tất

---

## Bước 13: Truy cập ứng dụng

Sau khi deploy thành công, truy cập:

```
https://ql-nhathuoc.onrender.com
```

Hoặc URL Render cung cấp cho bạn!

---

## Tóm tắt các URL quan trọng

```
Docker Hub:     https://hub.docker.com/u/tong1603
GitHub Actions: https://github.com/YOUR_USERNAME/QL_NhaThuoc/actions
Render Service: https://dashboard.render.com/web/srv-xxxxx
Live App:       https://ql-nhathuoc.onrender.com
```

---

## Lưu ý quan trọng

### Free Plan Render:
- ✅ Miễn phí
- ✅ 750 giờ/tháng
- ✅ SSL tự động
- ❌ Service sleep sau 15 phút không dùng
- ❌ Wake up mất 30-60 giây

### Database:
- ⚠️ Render Free không hỗ trợ SQL Server
- Cần dùng external database hoặc đổi sang PostgreSQL

---

Chúc bạn deploy thành công! 🚀
