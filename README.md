# 🖥️ Remote Monitoring System

Hệ thống giám sát và điều khiển máy tính từ xa trong mạng LAN, ứng dụng bảo mật mTLS và kiến trúc giao tiếp JSON.

---

## 🚀 Hướng dẫn chạy dự án (Dành cho thành viên sau khi Clone)

Vì lý do bảo mật và môi trường mạng, sau khi `git clone` dự án về máy, bạn **KHÔNG THỂ** bấm chạy (Start) ngay lập tức. Vui lòng thực hiện đúng 4 bước thiết lập dưới đây:

### Bước 1: Khôi phục thư viện (Restore NuGet Packages)
Dự án sử dụng các thư viện bên ngoài như `Newtonsoft.Json` và `System.Data.SQLite`. Khi tải code về, các thư viện này không đi kèm.
- Mở file Solution (`.sln`) trong Visual Studio.
- Nhấp chuột phải vào Solution (ở cột Solution Explorer) -> Chọn **Restore NuGet Packages**.
- Chờ Visual Studio tải xong các thư viện.

### Bước 2: Cấu hình địa chỉ IP máy chủ (Rất quan trọng)
Hiện tại code đang trỏ về một IP nội bộ cụ thể. Để chạy trong mạng LAN hiện tại của bạn:
1. Người chạy **Server** mở `cmd`, gõ `ipconfig` để lấy địa chỉ IPv4 (VD: `192.168.1.15`).
2. Những người chạy **Client** mở Visual Studio, dùng tính năng *Find and Replace* (Ctrl + Shift + F) để tìm IP cũ (ví dụ: `192.168.31.198` hoặc `127.0.0.1`) và thay bằng IP mới của Server.
3. Các file bắt buộc phải kiểm tra và đổi IP:
   - `ClientA/MainForm.cs`
   - `ClientA/LoginForm.cs`
   - `ClientA/RegisterForm.cs`
   - `ClientB/MonitoringForm.cs`

### Bước 3: Cấu hình Database SQLite
File CSDL thực tế không được đẩy lên GitHub để tránh phình dung lượng và xung đột.
- Vui lòng liên hệ với người quản lý dự án để lấy file `RemoteMonitor.db` (có sẵn dữ liệu mẫu).
- Hoặc tự tạo file `RemoteMonitor.db` bằng DB Browser for SQLite và chạy script SQL khởi tạo.
- Copy file `.db` đặt vào thư mục `Server/bin/Debug/net...` (cùng chỗ với file `.exe` của Server).

### Bước 4: Thêm Chứng chỉ bảo mật mTLS (.pfx)
- Liên hệ trưởng nhóm để lấy 2 file: `ServerCertECC.pfx` và `ClientCertECC.pfx` (Password: `NT106.Q23`).
- Copy và dán vào các thư mục Debug theo hướng dẫn ở phần dưới.

---

## 🔒 Cấu hình mTLS (Mutual TLS) - (Dành cho người setup ban đầu)

Dự án sử dụng mTLS với ECC (P-256) để bảo mật kết nối.  
Các file chứng chỉ (`.pfx`) **không được commit lên repo**, bạn cần tự tạo bằng OpenSSL.

### ⚙️ Yêu cầu
- Cài đặt OpenSSL (Windows / Git Bash / WSL)
- Mở terminal tại thư mục trống

### 🧩 Các bước thực hiện

#### 1. Tạo Root CA
```bash
openssl ecparam -genkey -name prime256v1 -out ca_ecc.key
openssl req -new -x509 -days 3650 -key ca_ecc.key -out ca_ecc.crt -subj "/CN=UIT_ECC_RootCA"
```
### 2. Tạo chứng chỉ Server

``` bash

openssl ecparam -genkey -name prime256v1 -out server_ecc.key

openssl req -new -key server_ecc.key -out server_ecc.csr -subj "/CN=RemoteMonitorServer"

openssl x509 -req -days 365 -in server_ecc.csr -CA ca_ecc.crt -CAkey ca_ecc.key -CAcreateserial -out server_ecc.crt

openssl pkcs12 -export -out ServerCertECC.pfx -inkey server_ecc.key -in server_ecc.crt -certfile ca_ecc.crt -passout pass:NT106.Q23

```



### 3. Tạo chứng chỉ Client

``` bash

openssl ecparam -genkey -name prime256v1 -out client_ecc.key

openssl req -new -key client_ecc.key -out client_ecc.csr -subj "/CN=RemoteMonitorClient"

openssl x509 -req -days 365 -in client_ecc.csr -CA ca_ecc.crt -CAkey ca_ecc.key -CAcreateserial -out client_ecc.crt

openssl pkcs12 -export -out ClientCertECC.pfx -inkey client_ecc.key -in client_ecc.crt -certfile ca_ecc.crt -passout pass:NT106.Q23

```



---



## 📦 Tích hợp vào project



Copy các file .pfx vào đúng thư mục:



- ServerCertECC.pfx → Server/bin/Debug/

- ClientCertECC.pfx → 

    - ClientA/bin/Debug/

    - ClientB/bin/Debug/
