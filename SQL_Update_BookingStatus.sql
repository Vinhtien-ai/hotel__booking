/* Chạy script này một lần trên đúng database HotelBooking đang được Web.config kết nối. */
IF COL_LENGTH('dbo.Booking', 'Status') IS NULL
BEGIN
    ALTER TABLE dbo.Booking
    ADD [Status] NVARCHAR(50) NOT NULL
        CONSTRAINT DF_Booking_Status DEFAULT (N'Đã đặt') WITH VALUES;
END;
GO

/* Chuẩn hóa các đơn cũ (nếu trước đây cột đã tồn tại nhưng rỗng). */
UPDATE dbo.Booking
SET [Status] = N'Đã đặt'
WHERE [Status] IS NULL OR LTRIM(RTRIM([Status])) = N'';
GO

/* Index hỗ trợ truy vấn kiểm tra phòng và khoảng ngày. */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_BookingDetail_IDRoom_IDBooking' AND object_id = OBJECT_ID('dbo.BookingDetail'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_BookingDetail_IDRoom_IDBooking
    ON dbo.BookingDetail (IDRoom, IDBooking);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Booking_DateBooking_CheckOut' AND object_id = OBJECT_ID('dbo.Booking'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Booking_DateBooking_CheckOut
    ON dbo.Booking (DateBooking, CheckOut);
END;
GO
