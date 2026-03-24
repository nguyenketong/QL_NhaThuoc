# Quick Start - Deploy lên Render

## Bước 1: Chạy SQL xóa bảng CHAT_MESSAGE

```bash
# Mở file XOA_BANG_CHAT_MESSAGE.sql trong SSMS và chạy (F5)
```

## Bước 2: Push code lên GitHub

```bash
git add .
git commit -m "Setup CI/CD with Docker and Render"
git push origin main
```

## Bước 3: Cấu hình GitHub Secrets

Vào: `https://github.com/YOUR_USERNAME/QL_NhaThuoc/settings/secrets/actions`

Thêm secrets:
```
DOCKER_USERNAME = your_dockerhub_username
DOCKER_PASSWORD = your_dockerhub_password
RENDER_API_KEY = your_render_api_key
RENDER_SERVICE_ID = srv-xxxxx
```

## Bước 4: Tạo service trên Render

1. Vào: https://dashboard.render.com
2. Click **New** → **Web Service**
3. Chọn **Deploy an existing image**
4. Image URL: `docker.io/YOUR_USERNAME/ql-nhathuoc:latest`
5. Region: Singapore
6. Instance: Free

## Bước 5: Thêm Environment Variables trên Render

```
ASPNETCORE_ENVIRONMENT = Production
ASPNETCORE_URLS = http://+:8080
ConnectionStrings__DefaultConnection = YOUR_CONNECTION_STRING
```

## Bước 6: Deploy

Push code → GitHub Actions tự động build → Deploy lên Render!

---

Xem hướng dẫn chi tiết: [HUONG_DAN_DEPLOY.md](HUONG_DAN_DEPLOY.md)
