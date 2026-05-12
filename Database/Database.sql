-- ==========================================
-- SCRIPT KHỞI TẠO CƠ SỞ DỮ LIỆU SQLITE
-- Đồ án: Chương trình theo dõi từ xa an toàn
-- ==========================================

-- Bật tính năng kiểm tra Khóa ngoại (Foreign Key) cho SQLite
PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS Users (
    UserId          INTEGER PRIMARY KEY AUTOINCREMENT, 
    Username        TEXT UNIQUE NOT NULL,            
    PasswordHash    TEXT NOT NULL,               
    Salt            TEXT NOT NULL,
    Role            TEXT DEFAULT 'User',                 
    CreatedDate     DATETIME DEFAULT CURRENT_TIMESTAMP 
);

CREATE TABLE IF NOT EXISTS Clients (
    ClientId        INTEGER PRIMARY KEY AUTOINCREMENT, 
    MachineName     TEXT UNIQUE NOT NULL,         
    IP              TEXT,                                    
    OwnerUserId     INTEGER,                        
    LastActive      DATETIME DEFAULT CURRENT_TIMESTAMP,                        
    FOREIGN KEY (OwnerUserId) REFERENCES Users(UserId) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS ResourceHistory (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,       
    ClientId        INTEGER NOT NULL,                  
    Timestamp       DATETIME DEFAULT CURRENT_TIMESTAMP, 
    CpuPercent      REAL,                            
    RamPercent      REAL,                            
    DiskPercent     REAL,                           
    NetworkDown     REAL,                           
    NetworkUp       REAL,                             
    AppList         TEXT,       
    FOREIGN KEY (ClientId) REFERENCES Clients(ClientId) ON DELETE CASCADE
);

-- Dữ liệu mẫu Admin
INSERT INTO Users (Username, PasswordHash, Salt, Role) 
VALUES ('admin', 'b1676334c7a3905649ecc3ad90ba2ab18c4b3f5f430c5754a7ba84744b6c6954', 'UuctoxqY1HamIqACTRcipQ==', 'Admin');