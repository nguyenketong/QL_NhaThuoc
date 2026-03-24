-- Xóa bảng CHAT_MESSAGE khỏi database

USE QL_NhaThuoc;
GO

-- Kiểm tra bảng có tồn tại không
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'CHAT_MESSAGE')
BEGIN
    PRINT 'Đang xóa bảng CHAT_MESSAGE...';
    
    -- Xóa bảng
    DROP TABLE CHAT_MESSAGE;
    
    PRINT '✅ Đã xóa bảng CHAT_MESSAGE thành công!';
END
ELSE
BEGIN
    PRINT '⚠️ Bảng CHAT_MESSAGE không tồn tại.';
END
GO
