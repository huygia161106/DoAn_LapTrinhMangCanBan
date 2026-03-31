-- ==========================================
-- SCRIPT KHỞI TẠO CƠ SỞ DỮ LIỆU SQLITE
-- Đồ án: Chương trình theo dõi từ xa
-- ==========================================

-- Bật tính năng kiểm tra Khóa ngoại (Foreign Key) cho SQLite
PRAGMA foreign_keys = ON;

-- 1. Bảng Users: Lưu thông tin tài khoản đăng nhập của Client A [cite: 44]
CREATE TABLE IF NOT EXISTS Users (
    UserId INTEGER PRIMARY KEY AUTOINCREMENT, -- 
    Username TEXT UNIQUE NOT NULL,            -- 
    PasswordHash TEXT NOT NULL,               -- Bắt buộc lưu mã băm (VD: SHA-256) 
    Role TEXT DEFAULT 'User',                 -- [cite: 45, 61]
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP -- [cite: 45, 61]
);

-- 2. Bảng Clients: Quản lý danh sách các máy bị theo dõi (Client B) [cite: 46]
CREATE TABLE IF NOT EXISTS Clients (
    ClientId INTEGER PRIMARY KEY AUTOINCREMENT, -- [cite: 47, 62]
    MachineName TEXT NOT NULL,                  -- [cite: 47, 62]
    IP TEXT,                                    -- [cite: 47, 62]
    OwnerUserId INTEGER,                        -- Khóa ngoại trỏ về người sở hữu máy này [cite: 47, 62]
    LastActive DATETIME,                        -- [cite: 62]
    FOREIGN KEY (OwnerUserId) REFERENCES Users(UserId) ON DELETE CASCADE
);

-- 3. Bảng ResourceHistory: Lưu lịch sử tài nguyên để vẽ biểu đồ [cite: 48]
CREATE TABLE IF NOT EXISTS ResourceHistory (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,       -- [cite: 49]
    ClientId INTEGER NOT NULL,                  -- [cite: 49]
    Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP, -- [cite: 49]
    CpuPercent REAL,                            -- [cite: 49]
    RamPercent REAL,                            -- [cite: 49]
    DiskPercent REAL,                           -- [cite: 49]
    NetworkDown REAL,                           -- [cite: 49]
    NetworkUp REAL,                             -- [cite: 49]
    AppList TEXT,                               -- Lưu danh sách process dưới dạng chuỗi (JSON hoặc Text phân tách)
    FOREIGN KEY (ClientId) REFERENCES Clients(ClientId) ON DELETE CASCADE
);

-- ==========================================
-- DỮ LIỆU MẪU (DUMMY DATA) ĐỂ TEST HỆ THỐNG
-- ==========================================

-- Thêm 2 tài khoản quản trị viên (Lưu ý: PasswordHash ở đây đang giả định là chuỗi băm SHA-256 của chữ "123456")
INSERT INTO Users (Username, PasswordHash, Role) 
VALUES 
('admin', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', 'Admin'),
('manager', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', 'User');

-- Thêm 2 máy Client B mẫu thuộc sở hữu của 'admin' (UserId = 1)
INSERT INTO Clients (MachineName, IP, OwnerUserId, LastActive)
VALUES
('LAPTOP-DEV-01', '192.168.1.10', 1, CURRENT_TIMESTAMP),
('SERVER-DB-02', '192.168.1.50', 1, CURRENT_TIMESTAMP);

-- Thêm 1 vài dòng lịch sử tài nguyên mẫu cho LAPTOP-DEV-01 (ClientId = 1)
INSERT INTO ResourceHistory (ClientId, CpuPercent, RamPercent, DiskPercent, NetworkDown, NetworkUp, AppList)
VALUES
(1, 45.5, 60.2, 30.0, 1024, 512, 'chrome.exe|Google Chrome;devenv.exe|Visual Studio'),
(1, 50.0, 62.1, 30.0, 2048, 1024, 'chrome.exe|Google Chrome;devenv.exe|Visual Studio;Spotify.exe|Spotify');