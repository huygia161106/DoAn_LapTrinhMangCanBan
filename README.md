## 🔒 Cấu hình mTLS (Mutual TLS)

Dự án sử dụng mTLS với ECC (P-256) để bảo mật kết nối.  
Các file chứng chỉ (`.pfx`) **không được commit lên repo**, bạn cần tự tạo bằng OpenSSL.

---

## ⚙️ Yêu cầu
- Cài đặt OpenSSL (Windows / Git Bash / WSL)
- Mở terminal tại thư mục trống

---

## 🧩 Các bước thực hiện

### 1. Tạo Root CA
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

