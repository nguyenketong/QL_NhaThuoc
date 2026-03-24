# Hướng dẫn Deploy lên Render với CI/CD

## Tổng quan quy trình

```
Code Push → GitHub → GitHub Actions → Build Docker → Push Docker Hub → Deploy Render
```

---

## Bước 1: Chuẩn bị tài khoản

### 1.1. Tạo tài khoản GitHub
- Truy cập: https://github.com
- Đăng ký tài khoản miễn phí

### 1.2. Tạo tài khoản Docker Hub
- Truy cập: https://hub.docker.com
- Đăng ký tài khoản miễn phí
- Tạo repository: `ql-nhathuoc`

### 1.3. Tạo tài khoản Render
- Truy cập: https://render.com
- Đăng ký bằng GitHub account
- Plan: Free (miễn phí)

---

## Bước 2: Tạo GitHub Repository

### 2.1. Tạo repository mới
```bash
# Trên GitHub, tạo repository mới: QL_NhaThuoc
```

### 2.2. Push code lên GitHub
```bash
# Trong thư mục dự án
git init
git add .
git commit -m "Initial commit"
git branch -M main
git remote add origin https://github.com/YOUR_USERNAME/QL_NhaThuoc.git
git push -u origin main
```

---

## Bước 3: Cấu hình GitHub Secrets

Vào repository → **Settings** → **Secrets and variables** → **Actions** → **New repository secret**

Thêm các secrets sau:

### 3.1. Docker Hub Credentials
```
DOCKER_USERNAME: your_dockerhub_username
DOCKER_PASSWORD: your_dockerhub_password
```

### 3.2. Render Credentials
```
RENDER_API_KEY: your_render_api_key
RENDER_SERVICE_ID: your_render_service_id
```

**Lấy Render API Key:**
1. Vào Render Dashboard
2. Click vào avatar → **Account Settings**
3. Chọn **API Keys** → **Create API Key**
4. Copy key rnd_rhmHxYTOcLxCn1xCgo02OjFM0GTP

---

## Bước 4: Tạo Web Service trên Render

### 4.1. Tạo service mới
1. Vào Render Dashboard: https://dashboard.render.com
2. Click **New** → **Web Service**
3. Chọn **Deploy an existing image from a registry**

### 4.2. Cấu hình service
```
Name: ql-nhathuoc
Region: Singapore (gần Việt Nam nhất)
Image URL: docker.io/YOUR_USERNAME/ql-nhathuoc:latest
Instance Type: Free
```

### 4.3. Thêm Environment Variables
```
ASPNETCORE_ENVIRONMENT = Production
ASPNETCORE_URLS = http://+:8080
ConnectionStrings__DefaultConnection = YOUR_SQL_SERVER_CONNECTION_STRING
```

**Lưu ý:** Với Free plan, bạn cần:
- Sử dụng external database (không thể host SQL Server trên Render Free)
- Hoặc dùng PostgreSQL/MySQL miễn phí từ Render

### 4.4. Lấy Service ID
- Sau khi tạo service, vào service details
- URL sẽ có dạng: `https://dashboard.render.com/web/srv-XXXXX`
- `srv-XXXXX` là RENDER_SERVICE_ID

---

## Bước 5: Cấu hình Database

### Option 1: Sử dụng SQL Server hiện tại (Khuyến nghị cho dev)
```
ConnectionStrings__DefaultConnection = Server=YOUR_PUBLIC_IP,1433;Database=QL_NhaThuoc;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;
```

**Lưu ý:** 
- Cần mở port 1433 ra internet (không an toàn)
- Hoặc dùng VPN/Tunnel

### Option 2: Sử dụng PostgreSQL trên Render (Miễn phí)
1. Tạo PostgreSQL database trên Render
2. Cài package: `Npgsql.EntityFrameworkCore.PostgreSQL`
3. Đổi connection string
4. Update `Program.cs`:
```csharp
builder.Services.AddDbContext<QL_NhaThuocDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
```

### Option 3: Sử dụng Azure SQL Database
- Tạo Azure SQL Database (có free tier)
- Lấy connection string
- Thêm vào Render environment variables

---

## Bước 6: Test CI/CD Pipeline

### 6.1. Trigger deployment
```bash
# Thay đổi code
git add .
git commit -m "Test CI/CD"
git push origin main
```

### 6.2. Theo dõi deployment
1. Vào GitHub repository → **Actions**
2. Xem workflow đang chạy
3. Kiểm tra từng step

### 6.3. Kiểm tra Render
1. Vào Render Dashboard
2. Xem service logs
3. Đợi deployment hoàn tất (3-5 phút)

---

## Bước 7: Truy cập ứng dụng

Sau khi deploy thành công:
```
https://ql-nhathuoc.onrender.com
```

**Lưu ý Free Plan:**
- Service sẽ sleep sau 15 phút không hoạt động
- Lần đầu truy cập sau khi sleep sẽ mất 30-60 giây để wake up
- Bandwidth: 100GB/tháng
- Build time: 500 giờ/tháng

---

## Bước 8: Cấu hình Custom Domain (Tùy chọn)

### 8.1. Thêm custom domain
1. Vào Render service → **Settings** → **Custom Domains**
2. Thêm domain của bạn: `nhathuoc.com`

### 8.2. Cấu hình DNS
Thêm CNAME record:
```
Type: CNAME
Name: www (hoặc @)
Value: ql-nhathuoc.onrender.com
```

---

## Troubleshooting

### Lỗi: Docker build failed
```bash
# Kiểm tra Dockerfile local
docker build -t ql-nhathuoc .
docker run -p 8080:8080 ql-nhathuoc
```

### Lỗi: Database connection failed
- Kiểm tra connection string
- Kiểm tra firewall/security group
- Kiểm tra database có accessible từ internet không

### Lỗi: Application crashed
- Xem logs trên Render Dashboard
- Kiểm tra environment variables
- Kiểm tra port (phải là 8080)

### Service sleep quá lâu
- Upgrade lên Paid plan ($7/tháng)
- Hoặc dùng cron job để ping service mỗi 10 phút

---

## Monitoring và Logs

### Xem logs real-time
```bash
# Trên Render Dashboard
Service → Logs → Live logs
```

### Xem deployment history
```bash
# Trên Render Dashboard
Service → Events
```

---

## Best Practices

### 1. Bảo mật
- ✅ Không commit secrets vào Git
- ✅ Sử dụng environment variables
- ✅ Enable HTTPS (Render tự động)
- ✅ Sử dụng strong passwords

### 2. Performance
- ✅ Enable caching trong Docker build
- ✅ Optimize Docker image size
- ✅ Sử dụng CDN cho static files

### 3. Monitoring
- ✅ Setup health check endpoint
- ✅ Monitor application logs
- ✅ Setup alerts cho errors

---

## Chi phí

### Free Plan (Đủ cho demo/testing)
- ✅ 750 giờ/tháng
- ✅ 100GB bandwidth
- ✅ Automatic SSL
- ❌ Service sleep sau 15 phút
- ❌ Shared CPU/RAM

### Starter Plan ($7/tháng)
- ✅ Always on (không sleep)
- ✅ 0.5 CPU, 512MB RAM
- ✅ 100GB bandwidth
- ✅ Faster builds

---

## Nâng cấp

### Thêm Redis Cache
```yaml
# render.yaml
services:
  - type: redis
    name: ql-nhathuoc-cache
    plan: free
```

### Thêm Background Worker
```yaml
# render.yaml
services:
  - type: worker
    name: ql-nhathuoc-worker
    env: docker
```

---

## Liên hệ hỗ trợ

- GitHub Issues: https://github.com/YOUR_USERNAME/QL_NhaThuoc/issues
- Render Support: https://render.com/docs
- Docker Hub: https://hub.docker.com

---

🎉 Chúc bạn deploy thành công!
