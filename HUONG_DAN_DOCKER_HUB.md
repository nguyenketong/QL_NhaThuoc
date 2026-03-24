# Hướng dẫn lấy Docker Hub Username và Password

## Bước 1: Đăng ký Docker Hub (Miễn phí)

### 1.1. Truy cập Docker Hub
🔗 **https://hub.docker.com/signup**

### 1.2. Điền thông tin đăng ký
- **Docker ID**: Chọn username (ví dụ: `nguyenketong`)
- **Email**: Email của bạn
- **Password**: Mật khẩu mạnh

### 1.3. Xác nhận email
- Kiểm tra email
- Click vào link xác nhận

---

## Bước 2: Lấy Username

### DOCKER_USERNAME

Đây chính là **Docker ID** bạn vừa đăng ký.

**Ví dụ:**
```
DOCKER_USERNAME = nguyenketong
```

**Cách kiểm tra:**
1. Đăng nhập vào https://hub.docker.com
2. Click vào avatar góc trên phải
3. Xem **Docker ID** của bạn

---

## Bước 3: Lấy Password/Access Token

### Option 1: Sử dụng Password (Không khuyến nghị)

```
DOCKER_PASSWORD = your_account_password
```

⚠️ **Không an toàn** - Password có thể bị lộ

### Option 2: Sử dụng Access Token (Khuyến nghị) ✅

#### 3.1. Tạo Access Token

1. Đăng nhập vào https://hub.docker.com
2. Click vào avatar → **Account Settings**
3. Chọn **Security** → **Personal Access Tokens**
4. Click **New Access Token**

#### 3.2. Cấu hình token

```
Token Description: GitHub Actions CI/CD
Access permissions: Read, Write, Delete
```

#### 3.3. Copy token

- Token có dạng: `dckr_pat_xxxxxxxxxxxxxxxxxxxxx`
- ⚠️ **LƯU Ý:** Token chỉ hiển thị 1 lần, hãy copy ngay!
- Lưu token vào nơi an toàn

#### 3.4. Sử dụng token

```
DOCKER_PASSWORD = dckr_pat_xxxxxxxxxxxxxxxxxxxxx
```

---

## Bước 4: Tạo Docker Repository

### 4.1. Tạo repository mới

1. Vào https://hub.docker.com
2. Click **Repositories** → **Create Repository**

### 4.2. Cấu hình repository

```
Repository Name: ql-nhathuoc
Visibility: Public (miễn phí) hoặc Private (cần paid plan)
Description: Hệ thống quản lý nhà thuốc
```

### 4.3. Lưu repository name

```
Repository: your_username/ql-nhathuoc
```

**Ví dụ:**
```
Repository: nguyenketong/ql-nhathuoc
```

---

## Bước 5: Thêm vào GitHub Secrets

### 5.1. Mở GitHub repository

Truy cập:
```
https://github.com/YOUR_USERNAME/QL_NhaThuoc/settings/secrets/actions
```

### 5.2. Thêm DOCKER_USERNAME

1. Click **New repository secret**
2. Name: `DOCKER_USERNAME`
3. Value: `nguyenketong` (Docker ID của bạn)
4. Click **Add secret**

### 5.3. Thêm DOCKER_PASSWORD

1. Click **New repository secret**
2. Name: `DOCKER_PASSWORD`
3. Value: `dckr_pat_xxxxxxxxxxxxxxxxxxxxx` (Access Token)
4. Click **Add secret**

---

## Bước 6: Kiểm tra

### 6.1. Xem secrets đã thêm

Vào: `Settings` → `Secrets and variables` → `Actions`

Bạn sẽ thấy:
```
✅ DOCKER_USERNAME
✅ DOCKER_PASSWORD
```

### 6.2. Test Docker login local

Mở terminal và chạy:

```bash
# Login vào Docker Hub
docker login -u YOUR_USERNAME -p YOUR_TOKEN

# Nếu thành công sẽ thấy:
# Login Succeeded
```

---

## Tóm tắt

### Thông tin cần có:

```
DOCKER_USERNAME = Docker ID của bạn (ví dụ: nguyenketong)
DOCKER_PASSWORD = Access Token (dckr_pat_xxxxx...)
DOCKER_REPOSITORY = your_username/ql-nhathuoc
```

### Ví dụ cụ thể:

```
DOCKER_USERNAME = nguyenketong
DOCKER_PASSWORD = dckr_pat_AbCdEf123456789XyZ
DOCKER_REPOSITORY = nguyenketong/ql-nhathuoc
```

---

## Troubleshooting

### Lỗi: "unauthorized: incorrect username or password"
- Kiểm tra username có đúng không
- Kiểm tra token còn hiệu lực không
- Tạo token mới nếu cần

### Lỗi: "denied: requested access to the resource is denied"
- Repository chưa tồn tại
- Tạo repository trên Docker Hub trước

### Quên Access Token?
- Không thể xem lại token cũ
- Tạo token mới:
  1. Xóa token cũ
  2. Tạo token mới
  3. Cập nhật GitHub Secret

---

## Bảo mật

⚠️ **QUAN TRỌNG:**

1. ✅ Sử dụng Access Token thay vì password
2. ✅ Không commit token vào Git
3. ✅ Chỉ cấp quyền cần thiết cho token
4. ✅ Xóa token không dùng nữa
5. ✅ Rotate token định kỳ (3-6 tháng)

---

## Chi phí

### Docker Hub Free Plan

✅ **Miễn phí** với:
- 1 private repository
- Unlimited public repositories
- 200 container pulls/6 giờ
- 5GB storage

Đủ cho dự án cá nhân và học tập!

---

## Liên kết hữu ích

- 🐳 Docker Hub: https://hub.docker.com
- 📚 Docker Docs: https://docs.docker.com
- 🔑 Access Tokens: https://hub.docker.com/settings/security
- 📦 Repositories: https://hub.docker.com/repositories

---

Sau khi có DOCKER_USERNAME và DOCKER_PASSWORD, quay lại file [QUICK_START_DEPLOY.md](QUICK_START_DEPLOY.md) để tiếp tục!
