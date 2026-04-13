-- ==========================================
-- SCRIPT KHỞI TẠO CƠ SỞ DỮ LIỆU SQLITE
-- Đồ án: Chương trình theo dõi từ xa
-- ==========================================

-- Bật tính năng kiểm tra Khóa ngoại (Foreign Key) cho SQLite
PRAGMA foreign_keys = ON;

-- 1. Bảng Users: Lưu thông tin tài khoản đăng nhập của Client A 
CREATE TABLE IF NOT EXISTS Users (
    UserId INTEGER PRIMARY KEY AUTOINCREMENT, -- 
    Username TEXT UNIQUE NOT NULL,            -- 
    PasswordHash TEXT NOT NULL,               -- 
    Salt    TEXT NOT NULL,
    Role TEXT DEFAULT 'User',                 -- 
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP -- 
);

-- 2. Bảng Clients: Quản lý danh sách các máy bị theo dõi (Client B) 
CREATE TABLE IF NOT EXISTS Clients (
    ClientId INTEGER PRIMARY KEY AUTOINCREMENT, -- 
    MachineName TEXT NOT NULL,                  -- 
    IP TEXT,                                    -- 
    OwnerUserId INTEGER,                        -- Khóa ngoại trỏ về người sở hữu máy này 
    LastActive DATETIME,                        -- 
    FOREIGN KEY (OwnerUserId) REFERENCES Users(UserId) ON DELETE CASCADE
);

-- 3. Bảng ResourceHistory: Lưu lịch sử tài nguyên để vẽ biểu đồ 
CREATE TABLE IF NOT EXISTS ResourceHistory (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,       -- 
    ClientId INTEGER NOT NULL,                  -- 
    Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP, -- 
    CpuPercent REAL,                            -- 
    RamPercent REAL,                            -- 
    DiskPercent REAL,                           -- 
    NetworkDown REAL,                           -- 
    NetworkUp REAL,                             -- 
    AppList TEXT,                               -- Lưu danh sách process dưới dạng chuỗi (JSON hoặc Text phân tách)
    FOREIGN KEY (ClientId) REFERENCES Clients(ClientId) ON DELETE CASCADE
);

-- ==========================================
-- DỮ LIỆU MẪU (DUMMY DATA) ĐỂ TEST HỆ THỐNG
-- ==========================================

-- Thêm 2 tài khoản quản trị viên (Lưu ý: PasswordHash ở đây đang giả định là chuỗi băm SHA-256 của chữ "123456")
INSERT INTO Users (Username, PasswordHash, Salt, Role) 
VALUES 
('admin', 'b1676334c7a3905649ecc3ad90ba2ab18c4b3f5f430c5754a7ba84744b6c6954', 'UuctoxqY1HamIqACTRcipQ==', 'Admin'),

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
