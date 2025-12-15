IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'USERDB')
BEGIN
    CREATE DATABASE USERDB;
END
GO

USE USERDB;
GO

-- =============================================
-- BẢNG 1: PLAYERS (Người chơi)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PLAYERS')
BEGIN
    CREATE TABLE PLAYERS
    (
        ID INT IDENTITY(1,1) PRIMARY KEY,
        USERNAME NVARCHAR(50) NOT NULL UNIQUE,
        EMAIL NVARCHAR(100) NULL, 
        PHONE NVARCHAR(20) NULL,   
        PASSWORDHASH NVARCHAR(64) NOT NULL,
        SALT NVARCHAR(50) NOT NULL,
        USER_LEVEL INT DEFAULT 1,
        XP INT DEFAULT 0,
        TOTAL_XP INT DEFAULT 0,
        LAST_LOGIN DATETIME NULL,
        CREATED_AT DATETIME DEFAULT GETDATE()
    );
    PRINT '✅ Table PLAYERS created';
END
GO

-- Tạo filtered unique index cho EMAIL (cho phép nhiều NULL)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PLAYERS_EMAIL_UNIQUE' AND object_id = OBJECT_ID('PLAYERS'))
BEGIN
    CREATE UNIQUE INDEX IX_PLAYERS_EMAIL_UNIQUE ON PLAYERS(EMAIL) WHERE EMAIL IS NOT NULL;
    PRINT '✅ Filtered unique index for EMAIL created';
END
GO

-- Tạo filtered unique index cho PHONE (cho phép nhiều NULL)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PLAYERS_PHONE_UNIQUE' AND object_id = OBJECT_ID('PLAYERS'))
BEGIN
    CREATE UNIQUE INDEX IX_PLAYERS_PHONE_UNIQUE ON PLAYERS(PHONE) WHERE PHONE IS NOT NULL;
    PRINT '✅ Filtered unique index for PHONE created';
END
GO


-- =============================================
-- BẢNG 2: ROOMS (Phòng chơi) 
-- =============================================

-- Xóa bảng MATCHES trước (vì nó reference đến ROOMS)
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'MATCHES')
BEGIN
    DROP TABLE MATCHES;
    PRINT '🔄 Dropped MATCHES table (will recreate)';
END
GO

-- Xóa bảng ROOMS
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ROOMS')
BEGIN
    DROP TABLE ROOMS;
    PRINT '🔄 Dropped old ROOMS table';
END
GO

CREATE TABLE ROOMS
(
    ROOM_ID INT IDENTITY(1,1) PRIMARY KEY,
    
    -- Room Code: 6 chữ số (000000 - 999999)
    ROOM_CODE VARCHAR(6) NOT NULL UNIQUE,
    
    ROOM_NAME NVARCHAR(100) NOT NULL,
    ROOM_PASSWORD NVARCHAR(100) NULL,
    
    -- Players
    PLAYER1_USERNAME NVARCHAR(50) NULL,
    PLAYER2_USERNAME NVARCHAR(50) NULL,
    
    -- Trạng thái: WAITING, READY, PLAYING, FINISHED, EMPTY
    ROOM_STATUS NVARCHAR(20) DEFAULT 'WAITING',
    
    -- Timestamps
    CREATED_AT DATETIME DEFAULT GETDATE(),
    LAST_ACTIVITY DATETIME DEFAULT GETDATE(),
    
    -- Foreign Keys (NO ACTION để tránh multiple cascade paths)
    FOREIGN KEY (PLAYER1_USERNAME) REFERENCES PLAYERS(USERNAME) ON DELETE NO ACTION,
    FOREIGN KEY (PLAYER2_USERNAME) REFERENCES PLAYERS(USERNAME) ON DELETE NO ACTION,
);

-- Index cho tìm kiếm nhanh
CREATE INDEX IX_ROOMS_STATUS ON ROOMS(ROOM_STATUS);
CREATE INDEX IX_ROOMS_LAST_ACTIVITY ON ROOMS(LAST_ACTIVITY);

PRINT '✅ Table ROOMS created with ROOM_CODE and LAST_ACTIVITY';
GO

-- =============================================
-- BẢNG 3: GLOBAL_CHAT_HISTORY
-- Lưu lịch sử chat global (toàn server)
-- =============================================
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'GLOBAL_CHAT_HISTORY')
BEGIN
    DROP TABLE GLOBAL_CHAT_HISTORY;
    PRINT '🔄 Dropped old GLOBAL_CHAT_HISTORY table';
END
GO

CREATE TABLE GLOBAL_CHAT_HISTORY
(
    MESSAGE_ID INT IDENTITY(1,1) PRIMARY KEY,
    USERNAME NVARCHAR(50) NOT NULL,
    MESSAGE_TEXT NVARCHAR(MAX) NOT NULL,
    SENT_AT DATETIME DEFAULT GETDATE(),
    
    -- Foreign Key - NO ACTION để tránh xóa lịch sử khi user bị xóa
    FOREIGN KEY (USERNAME) REFERENCES PLAYERS(USERNAME) ON DELETE NO ACTION
);

-- Index cho truy vấn nhanh
CREATE INDEX IX_GLOBAL_CHAT_SENT_AT ON GLOBAL_CHAT_HISTORY(SENT_AT DESC);
CREATE INDEX IX_GLOBAL_CHAT_USERNAME ON GLOBAL_CHAT_HISTORY(USERNAME);

PRINT '✅ Table GLOBAL_CHAT_HISTORY created';
GO

-- =============================================
-- BẢNG 4: LOBBY_CHAT_HISTORY
-- Lưu lịch sử chat trong lobby của từng room
-- =============================================
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'LOBBY_CHAT_HISTORY')
BEGIN
    DROP TABLE LOBBY_CHAT_HISTORY;
    PRINT '🔄 Dropped old LOBBY_CHAT_HISTORY table';
END
GO

CREATE TABLE LOBBY_CHAT_HISTORY
(
    MESSAGE_ID INT IDENTITY(1,1) PRIMARY KEY,
    ROOM_CODE VARCHAR(6) NOT NULL,
    USERNAME NVARCHAR(50) NOT NULL,
    MESSAGE_TEXT NVARCHAR(MAX) NOT NULL,
    SENT_AT DATETIME DEFAULT GETDATE(),
    
    -- Foreign Key - NO ACTION để KHÔNG XÓA lịch sử khi room bị xóa
    FOREIGN KEY (ROOM_CODE) REFERENCES ROOMS(ROOM_CODE) ON DELETE NO ACTION,
    FOREIGN KEY (USERNAME) REFERENCES PLAYERS(USERNAME) ON DELETE NO ACTION
);

-- Index cho truy vấn nhanh
CREATE INDEX IX_LOBBY_CHAT_ROOM_CODE ON LOBBY_CHAT_HISTORY(ROOM_CODE);
CREATE INDEX IX_LOBBY_CHAT_SENT_AT ON LOBBY_CHAT_HISTORY(SENT_AT DESC);
CREATE INDEX IX_LOBBY_CHAT_USERNAME ON LOBBY_CHAT_HISTORY(USERNAME);

PRINT '✅ Table LOBBY_CHAT_HISTORY created';
GO

-- =============================================
-- BẢNG 5: MATCHES (Lịch sử trận đấu)
-- =============================================
CREATE TABLE MATCHES
(
    MATCH_ID INT IDENTITY(1,1) PRIMARY KEY,
    ROOM_ID INT NOT NULL,
    WINNER_USERNAME NVARCHAR(50) NULL,
    PLAYER1_WINS INT DEFAULT 0,
    PLAYER2_WINS INT DEFAULT 0,
    CURRENT_ROUND INT DEFAULT 1,
    TOTAL_ROUNDS INT DEFAULT 3,
    
    -- Health cuối trận
    HEALTH_PLAYER1 INT DEFAULT 100,
    HEALTH_PLAYER2 INT DEFAULT 100,
    
    -- Mana cuối trận
    PLAYER1_MANA INT DEFAULT 0,
    PLAYER2_MANA INT DEFAULT 0,
    
    -- Thống kê
    PARRY_COUNT INT DEFAULT 0,
    BLOCK_COUNT INT DEFAULT 0,
    SKILL_COUNT INT DEFAULT 0,
    
    -- Điểm và XP
    PLAYER1_SCORE INT DEFAULT 0,
    PLAYER2_SCORE INT DEFAULT 0,
    MATCH_XP_PLAYER1 INT DEFAULT 0,
    MATCH_XP_PLAYER2 INT DEFAULT 0,
    
    -- Thời gian
    MATCH_DURATION_SECONDS INT DEFAULT 0,
    CREATED_AT DATETIME DEFAULT GETDATE(),
    FINISHED_AT DATETIME NULL,
    
    -- Foreign Keys (NO ACTION để tránh cascade issues)
    FOREIGN KEY (ROOM_ID) REFERENCES ROOMS(ROOM_ID) ON DELETE NO ACTION,
    FOREIGN KEY (WINNER_USERNAME) REFERENCES PLAYERS(USERNAME) ON DELETE NO ACTION
);

PRINT '✅ Table MATCHES created';
GO

-- =============================================
-- TRIGGER: Cập nhật TOTAL_XP sau mỗi trận
-- =============================================
IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'TRG_UPDATE_TOTAL_XP')
    DROP TRIGGER TRG_UPDATE_TOTAL_XP;
GO

CREATE TRIGGER TRG_UPDATE_TOTAL_XP
ON MATCHES
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    -- Cập nhật XP cho Player 1
    UPDATE P
    SET P.TOTAL_XP = P.TOTAL_XP + I.MATCH_XP_PLAYER1,
        P.XP = P.XP + I.MATCH_XP_PLAYER1
    FROM PLAYERS P
    INNER JOIN ROOMS R ON P.USERNAME = R.PLAYER1_USERNAME
    INNER JOIN INSERTED I ON R.ROOM_ID = I.ROOM_ID
    WHERE I.MATCH_XP_PLAYER1 > 0;

    -- Cập nhật XP cho Player 2
    UPDATE P
    SET P.TOTAL_XP = P.TOTAL_XP + I.MATCH_XP_PLAYER2,
        P.XP = P.XP + I.MATCH_XP_PLAYER2
    FROM PLAYERS P
    INNER JOIN ROOMS R ON P.USERNAME = R.PLAYER2_USERNAME
    INNER JOIN INSERTED I ON R.ROOM_ID = I.ROOM_ID
    WHERE I.MATCH_XP_PLAYER2 > 0;
END
GO

PRINT '✅ Trigger TRG_UPDATE_TOTAL_XP created';
GO

-- =============================================
-- STORED PROCEDURE: Kiểm tra Room Code tồn tại
-- =============================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_CHECK_ROOM_CODE_EXISTS')
    DROP PROCEDURE SP_CHECK_ROOM_CODE_EXISTS;
GO

CREATE PROCEDURE SP_CHECK_ROOM_CODE_EXISTS
    @RoomCode VARCHAR(6)
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (SELECT 1 FROM ROOMS WHERE ROOM_CODE = @RoomCode)
        SELECT 1 AS [Exists];
    ELSE
        SELECT 0 AS [Exists];
END
GO

PRINT '✅ SP_CHECK_ROOM_CODE_EXISTS created';
GO

-- =============================================
-- STORED PROCEDURE: Tạo Room mới
-- =============================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_CREATE_ROOM_EMPTY')
    DROP PROCEDURE SP_CREATE_ROOM_EMPTY;
GO

CREATE PROCEDURE SP_CREATE_ROOM_EMPTY
    @RoomCode VARCHAR(6),
    @RoomName NVARCHAR(100),
    @RoomPassword NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Kiểm tra room code đã tồn tại
    IF EXISTS (SELECT 1 FROM ROOMS WHERE ROOM_CODE = @RoomCode)
    BEGIN
        SELECT 0 AS Success, N'Room code already exists' AS Message, NULL AS RoomId;
        RETURN;
    END
    
    -- Tạo room TRỐNG (không có player nào)
    INSERT INTO ROOMS (ROOM_CODE, ROOM_NAME, ROOM_PASSWORD, PLAYER1_USERNAME, PLAYER2_USERNAME, ROOM_STATUS, CREATED_AT, LAST_ACTIVITY)
    VALUES (@RoomCode, @RoomName, @RoomPassword, NULL, NULL, 'WAITING', GETDATE(), GETDATE());
    
    SELECT 1 AS Success, N'Room created successfully' AS Message, SCOPE_IDENTITY() AS RoomId;
END
GO

PRINT '✅ SP_CREATE_ROOM_EMPTY created';
GO
-- =============================================
-- STORED PROCEDURE: Tham gia Room
-- =============================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_JOIN_ROOM')
    DROP PROCEDURE SP_JOIN_ROOM;
GO

CREATE PROCEDURE SP_JOIN_ROOM
    @RoomCode VARCHAR(6),
    @Password NVARCHAR(100) = NULL,
    @Username NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @RoomId INT, @RoomPassword NVARCHAR(100);
    DECLARE @Player1 NVARCHAR(50), @Player2 NVARCHAR(50), @Status NVARCHAR(20);
    
    -- Lấy thông tin room
    SELECT @RoomId = ROOM_ID, 
           @RoomPassword = ROOM_PASSWORD,
           @Player1 = PLAYER1_USERNAME, 
           @Player2 = PLAYER2_USERNAME,
           @Status = ROOM_STATUS
    FROM ROOMS 
    WHERE ROOM_CODE = @RoomCode;
    
    -- Kiểm tra room tồn tại
    IF @RoomId IS NULL
    BEGIN
        SELECT 0 AS Success, N'Room not found' AS Message;
        RETURN;
    END
    
    -- Kiểm tra trạng thái room
    IF @Status NOT IN ('WAITING', 'READY')
    BEGIN
        SELECT 0 AS Success, N'Room is not available' AS Message;
        RETURN;
    END
    
    -- Kiểm tra mật khẩu
    IF @RoomPassword IS NOT NULL AND LEN(@RoomPassword) > 0
    BEGIN
        IF @Password IS NULL OR @Password != @RoomPassword
        BEGIN
            SELECT 0 AS Success, N'Incorrect password' AS Message;
            RETURN;
        END
    END
    
    -- Kiểm tra room đã đầy
    IF @Player1 IS NOT NULL AND @Player2 IS NOT NULL
    BEGIN
        SELECT 0 AS Success, N'Room is full' AS Message;
        RETURN;
    END
    
    -- Kiểm tra đã ở trong room
    IF @Player1 = @Username OR @Player2 = @Username
    BEGIN
        SELECT 0 AS Success, N'You are already in this room' AS Message;
        RETURN;
    END
    
    -- Kiểm tra username tồn tại
    IF NOT EXISTS (SELECT 1 FROM PLAYERS WHERE USERNAME = @Username)
    BEGIN
        SELECT 0 AS Success, N'Player not found' AS Message;
        RETURN;
    END
    
    -- Thêm player vào slot trống
    IF @Player1 IS NULL
    BEGIN
        UPDATE ROOMS 
        SET PLAYER1_USERNAME = @Username, LAST_ACTIVITY = GETDATE()
        WHERE ROOM_CODE = @RoomCode;
    END
    ELSE
    BEGIN
        UPDATE ROOMS 
        SET PLAYER2_USERNAME = @Username, 
            ROOM_STATUS = 'READY',
            LAST_ACTIVITY = GETDATE()
        WHERE ROOM_CODE = @RoomCode;
    END
    
    SELECT 1 AS Success, N'Joined room successfully' AS Message;
END
GO

PRINT '✅ SP_JOIN_ROOM created';
GO

-- =============================================
-- STORED PROCEDURE: Rời khỏi Room
-- =============================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_LEAVE_ROOM')
    DROP PROCEDURE SP_LEAVE_ROOM;
GO

CREATE PROCEDURE SP_LEAVE_ROOM
    @RoomCode VARCHAR(6),
    @Username NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Player1 NVARCHAR(50), @Player2 NVARCHAR(50);
    
    SELECT @Player1 = PLAYER1_USERNAME, @Player2 = PLAYER2_USERNAME
    FROM ROOMS 
    WHERE ROOM_CODE = @RoomCode;
    
    -- Xóa player khỏi room
    IF @Player1 = @Username
    BEGIN
        UPDATE ROOMS 
        SET PLAYER1_USERNAME = NULL, 
            ROOM_STATUS = 'WAITING',
            LAST_ACTIVITY = GETDATE()
        WHERE ROOM_CODE = @RoomCode;
    END
    ELSE IF @Player2 = @Username
    BEGIN
        UPDATE ROOMS 
        SET PLAYER2_USERNAME = NULL,
            ROOM_STATUS = CASE WHEN @Player1 IS NOT NULL THEN 'WAITING' ELSE 'WAITING' END,
            LAST_ACTIVITY = GETDATE()
        WHERE ROOM_CODE = @RoomCode;
    END
    
    SELECT 1 AS Success, N'Left room successfully' AS Message;
END
GO

PRINT '✅ SP_LEAVE_ROOM created';
GO

-- =============================================
-- STORED PROCEDURE: Lấy danh sách Room khả dụng
-- =============================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_GET_AVAILABLE_ROOMS')
    DROP PROCEDURE SP_GET_AVAILABLE_ROOMS;
GO

CREATE PROCEDURE SP_GET_AVAILABLE_ROOMS
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ROOM_ID AS RoomId,
        ROOM_CODE AS RoomCode,
        ROOM_NAME AS RoomName,
        ROOM_PASSWORD AS Password,
        PLAYER1_USERNAME AS Player1Username,
        PLAYER2_USERNAME AS Player2Username,
        ROOM_STATUS AS Status,
        CREATED_AT AS CreatedAt,
        LAST_ACTIVITY AS LastActivity,
        -- Đếm số player
        CASE 
            WHEN PLAYER1_USERNAME IS NOT NULL AND PLAYER2_USERNAME IS NOT NULL THEN 2
            WHEN PLAYER1_USERNAME IS NOT NULL OR PLAYER2_USERNAME IS NOT NULL THEN 1
            ELSE 0
        END AS PlayerCount
    FROM ROOMS
    WHERE ROOM_STATUS IN ('WAITING', 'READY')
    ORDER BY CREATED_AT DESC;
END
GO

PRINT '✅ SP_GET_AVAILABLE_ROOMS created';
GO

-- =============================================
-- STORED PROCEDURE: Lấy thông tin Room theo Code
-- =============================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_GET_ROOM_BY_CODE')
    DROP PROCEDURE SP_GET_ROOM_BY_CODE;
GO

CREATE PROCEDURE SP_GET_ROOM_BY_CODE
    @RoomCode VARCHAR(6)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ROOM_ID AS RoomId,
        ROOM_CODE AS RoomCode,
        ROOM_NAME AS RoomName,
        ROOM_PASSWORD AS Password,
        PLAYER1_USERNAME AS Player1Username,
        PLAYER2_USERNAME AS Player2Username,
        ROOM_STATUS AS Status,
        CREATED_AT AS CreatedAt,
        LAST_ACTIVITY AS LastActivity
    FROM ROOMS
    WHERE ROOM_CODE = @RoomCode;
END
GO

PRINT '✅ SP_GET_ROOM_BY_CODE created';
GO

-- =============================================
-- STORED PROCEDURE: Cập nhật trạng thái Room
-- =============================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_UPDATE_ROOM_STATUS')
    DROP PROCEDURE SP_UPDATE_ROOM_STATUS;
GO

CREATE PROCEDURE SP_UPDATE_ROOM_STATUS
    @RoomCode VARCHAR(6),
    @Status NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE ROOMS 
    SET ROOM_STATUS = @Status, LAST_ACTIVITY = GETDATE()
    WHERE ROOM_CODE = @RoomCode;
    
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

PRINT '✅ SP_UPDATE_ROOM_STATUS created';
GO

-- =============================================
-- STORED PROCEDURE: Dọn dẹp Room không hoạt động (7 ngày)
-- =============================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_CLEANUP_INACTIVE_ROOMS')
    DROP PROCEDURE SP_CLEANUP_INACTIVE_ROOMS;
GO

CREATE PROCEDURE SP_CLEANUP_INACTIVE_ROOMS
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @DeletedCount INT = 0;
    
    -- Xóa các room EMPTY không hoạt động > 7 ngày
    DELETE FROM ROOMS 
    WHERE ROOM_STATUS = 'EMPTY'
      AND LAST_ACTIVITY < DATEADD(DAY, -7, GETDATE());
    
    SET @DeletedCount = @DeletedCount + @@ROWCOUNT;
    
    -- Xóa các room không có ai và không hoạt động > 7 ngày
    DELETE FROM ROOMS 
    WHERE PLAYER1_USERNAME IS NULL 
      AND PLAYER2_USERNAME IS NULL
      AND LAST_ACTIVITY < DATEADD(DAY, -7, GETDATE());
    
    SET @DeletedCount = @DeletedCount + @@ROWCOUNT;
    
    SELECT @DeletedCount AS DeletedRooms;
END
GO

PRINT '✅ SP_CLEANUP_INACTIVE_ROOMS created';
GO

-- =============================================
-- STORED PROCEDURE: Lưu tin nhắn Global Chat
-- =============================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_SAVE_GLOBAL_CHAT_MESSAGE')
    DROP PROCEDURE SP_SAVE_GLOBAL_CHAT_MESSAGE;
GO

CREATE PROCEDURE SP_SAVE_GLOBAL_CHAT_MESSAGE
    @Username NVARCHAR(50),
    @MessageText NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Kiểm tra username tồn tại
    IF NOT EXISTS (SELECT 1 FROM PLAYERS WHERE USERNAME = @Username)
    BEGIN
        SELECT 0 AS Success, N'Player not found' AS Message;
        RETURN;
    END
    
    -- Lưu tin nhắn
    INSERT INTO GLOBAL_CHAT_HISTORY (USERNAME, MESSAGE_TEXT, SENT_AT)
    VALUES (@Username, @MessageText, GETDATE());
    
    SELECT 1 AS Success, N'Message saved' AS Message, SCOPE_IDENTITY() AS MessageId;
END
GO

PRINT '✅ SP_SAVE_GLOBAL_CHAT_MESSAGE created';
GO

-- =============================================
-- STORED PROCEDURE: Lấy lịch sử Global Chat
-- =============================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_GET_GLOBAL_CHAT_HISTORY')
    DROP PROCEDURE SP_GET_GLOBAL_CHAT_HISTORY;
GO

CREATE PROCEDURE SP_GET_GLOBAL_CHAT_HISTORY
    @Limit INT = 50  -- Lấy 50 tin nhắn gần nhất
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@Limit)
        MESSAGE_ID AS MessageId,
        USERNAME AS Username,
        MESSAGE_TEXT AS MessageText,
        SENT_AT AS SentAt
    FROM GLOBAL_CHAT_HISTORY
    ORDER BY SENT_AT DESC;
END
GO

PRINT '✅ SP_GET_GLOBAL_CHAT_HISTORY created';
GO

-- =============================================
-- STORED PROCEDURE: Lưu tin nhắn Lobby Chat
-- =============================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_SAVE_LOBBY_CHAT_MESSAGE')
    DROP PROCEDURE SP_SAVE_LOBBY_CHAT_MESSAGE;
GO

CREATE PROCEDURE SP_SAVE_LOBBY_CHAT_MESSAGE
    @RoomCode VARCHAR(6),
    @Username NVARCHAR(50),
    @MessageText NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Kiểm tra room tồn tại
    IF NOT EXISTS (SELECT 1 FROM ROOMS WHERE ROOM_CODE = @RoomCode)
    BEGIN
        SELECT 0 AS Success, N'Room not found' AS Message;
        RETURN;
    END
    
    -- Kiểm tra username tồn tại
    IF NOT EXISTS (SELECT 1 FROM PLAYERS WHERE USERNAME = @Username)
    BEGIN
        SELECT 0 AS Success, N'Player not found' AS Message;
        RETURN;
    END
    
    -- Lưu tin nhắn
    INSERT INTO LOBBY_CHAT_HISTORY (ROOM_CODE, USERNAME, MESSAGE_TEXT, SENT_AT)
    VALUES (@RoomCode, @Username, @MessageText, GETDATE());
    
    SELECT 1 AS Success, N'Message saved' AS Message, SCOPE_IDENTITY() AS MessageId;
END
GO

PRINT '✅ SP_SAVE_LOBBY_CHAT_MESSAGE created';
GO

-- =============================================
-- STORED PROCEDURE: Lấy lịch sử Lobby Chat
-- =============================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_GET_LOBBY_CHAT_HISTORY')
    DROP PROCEDURE SP_GET_LOBBY_CHAT_HISTORY;
GO

CREATE PROCEDURE SP_GET_LOBBY_CHAT_HISTORY
    @RoomCode VARCHAR(6),
    @Limit INT = 50  -- Lấy 50 tin nhắn gần nhất
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Kiểm tra room tồn tại
    IF NOT EXISTS (SELECT 1 FROM ROOMS WHERE ROOM_CODE = @RoomCode)
    BEGIN
        SELECT 0 AS Success, N'Room not found' AS Message;
        RETURN;
    END
    
    SELECT TOP (@Limit)
        MESSAGE_ID AS MessageId,
        ROOM_CODE AS RoomCode,
        USERNAME AS Username,
        MESSAGE_TEXT AS MessageText,
        SENT_AT AS SentAt
    FROM LOBBY_CHAT_HISTORY
    WHERE ROOM_CODE = @RoomCode
    ORDER BY SENT_AT DESC;
END
GO

PRINT '✅ SP_GET_LOBBY_CHAT_HISTORY created';
GO

-- =============================================
-- STORED PROCEDURE: Xóa lịch sử chat cũ (tùy chọn)
-- Có thể chạy định kỳ để dọn dẹp dữ liệu cũ
-- =============================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'SP_CLEANUP_OLD_CHAT_HISTORY')
    DROP PROCEDURE SP_CLEANUP_OLD_CHAT_HISTORY;
GO

CREATE PROCEDURE SP_CLEANUP_OLD_CHAT_HISTORY
    @DaysToKeep INT = 90  -- Giữ lại 90 ngày
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @DeletedGlobal INT = 0;
    DECLARE @DeletedLobby INT = 0;
    DECLARE @CutoffDate DATETIME = DATEADD(DAY, -@DaysToKeep, GETDATE());
    
    -- Xóa global chat cũ
    DELETE FROM GLOBAL_CHAT_HISTORY 
    WHERE SENT_AT < @CutoffDate;
    SET @DeletedGlobal = @@ROWCOUNT;
    
    -- Xóa lobby chat cũ
    DELETE FROM LOBBY_CHAT_HISTORY 
    WHERE SENT_AT < @CutoffDate;
    SET @DeletedLobby = @@ROWCOUNT;
    
    SELECT 
        @DeletedGlobal AS DeletedGlobalMessages,
        @DeletedLobby AS DeletedLobbyMessages,
        @CutoffDate AS CutoffDate;
END
GO

PRINT '✅ SP_CLEANUP_OLD_CHAT_HISTORY created';
GO

-- =============================================
-- HOÀN THÀNH
-- =============================================
PRINT '';
PRINT '========================================';
PRINT '✅ DATABASE USERDB SETUP COMPLETE!';
PRINT '========================================';
PRINT 'Tables: PLAYERS, CHARACTERS, ROOMS, MATCHES';
PRINT 'Stored Procedures:';
PRINT '  - SP_CHECK_ROOM_CODE_EXISTS';
PRINT '  - SP_CREATE_ROOM';
PRINT '  - SP_JOIN_ROOM';
PRINT '  - SP_LEAVE_ROOM';
PRINT '  - SP_GET_AVAILABLE_ROOMS';
PRINT '  - SP_GET_ROOM_BY_CODE';
PRINT '  - SP_UPDATE_ROOM_STATUS';
PRINT '  - SP_CLEANUP_INACTIVE_ROOMS';
PRINT '  - SP_SELECT_CHARACTER';
PRINT '========================================';
GO