IF DB_ID(N'DBHotelBooking') IS NULL
BEGIN
    CREATE DATABASE [DBHotelBooking];
END;
GO
USE [DBHotelBooking];
GO

/*Tai khoan admin */
IF OBJECT_ID(N'dbo.AdminUser', N'U') IS NULL
BEGIN
CREATE TABLE dbo.AdminUser
( 
        ID           int           NOT NULL,
        NameUser     nvarchar(max) NULL,
        RoleUser     nvarchar(max) NULL,
        PasswordUser nchar(50)     NULL,
        CONSTRAINT PK_AdminUser PRIMARY KEY (ID)
    );
END;
GO

/* 2. Khach hang */
IF OBJECT_ID(N'dbo.Customer', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Customer
    (
        IDCus       int IDENTITY(1,1) NOT NULL,
        NameCus     nvarchar(max)     NULL,
        PhoneCus    nvarchar(15)      NULL,
        EmailCus    nvarchar(max)     NULL,
        PasswordCus nvarchar(255)     NULL,
        CONSTRAINT PK_Customer PRIMARY KEY (IDCus)
    );
END;
GO
/*Loai phong */
IF OBJECT_ID(N'dbo.RoomType', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.RoomType
    (
        Id       int IDENTITY(1,1) NOT NULL,
        IDType   nchar(20)         NOT NULL,
        NameType nvarchar(max)     NULL,
        CONSTRAINT PK_RoomType PRIMARY KEY (IDType)
    );
END;
GO
/* Phong khach san */
IF OBJECT_ID(N'dbo.HotelRoom', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.HotelRoom
    (
        RoomID          int IDENTITY(1,1) NOT NULL,
        NameRoom        nvarchar(max)     NULL,
        DescriptionRoom nvarchar(max)     NULL,
        RoomType        nchar(20)         NULL,
        Price           decimal(18,2)     NULL,
        ImageRoom       nvarchar(max)     NULL,
        CONSTRAINT PK_HotelRoom PRIMARY KEY (RoomID),
        CONSTRAINT FK_Room_RoomType FOREIGN KEY (RoomType)
            REFERENCES dbo.RoomType (IDType)
    );
END;
GO
/*Don dat phong */
IF OBJECT_ID(N'dbo.Booking', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Booking
    (
        ID              int IDENTITY(1,1) NOT NULL,
        DateBooking     date              NULL,
        IDCus           int               NULL,
        AddressDelivery nvarchar(max)     NULL,
        CheckOut        date              NULL,
        Status          nvarchar(50)      NOT NULL
            CONSTRAINT DF_Booking_Status DEFAULT (N'Đã đặt'),
        CONSTRAINT PK_Booking PRIMARY KEY (ID),
        CONSTRAINT FK_Booking_Customer FOREIGN KEY (IDCus)
            REFERENCES dbo.Customer (IDCus)
    );
END;
GO
/*Chi tiet don dat phong */
IF OBJECT_ID(N'dbo.BookingDetail', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BookingDetail
    (
        ID        int IDENTITY(1,1) NOT NULL,
        IDRoom    int               NULL,
        IDBooking int               NULL,
        Quantity  int               NULL,
        UnitPrice float             NULL,
        CONSTRAINT PK_BookingDetail PRIMARY KEY (ID),
        CONSTRAINT FK_BookingDetail_Booking FOREIGN KEY (IDBooking)
            REFERENCES dbo.Booking (ID),
        CONSTRAINT FK_BookingDetail_HotelRoom FOREIGN KEY (IDRoom)
            REFERENCES dbo.HotelRoom (RoomID)
    );
END;
/* Index ho tro truy van */
IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_BookingDetail_IDRoom_IDBooking'
      AND object_id = OBJECT_ID(N'dbo.BookingDetail')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_BookingDetail_IDRoom_IDBooking
        ON dbo.BookingDetail (IDRoom, IDBooking);
END;
GO
IF NOT EXISTS
(
    SELECT 1 FROM sys.indexes
    WHERE name = N'IX_Booking_DateBooking_CheckOut'
      AND object_id = OBJECT_ID(N'dbo.Booking')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Booking_DateBooking_CheckOut
        ON dbo.Booking (DateBooking, CheckOut);
END;
GO
/* dữ liệu test*/


/* Loai phong */
SET IDENTITY_INSERT dbo.RoomType ON;
IF NOT EXISTS (SELECT 1 FROM dbo.RoomType WHERE IDType = N'DLX')
    INSERT dbo.RoomType (Id, IDType, NameType) VALUES (1, N'DLX', N'Phòng Deluxe');
IF NOT EXISTS (SELECT 1 FROM dbo.RoomType WHERE IDType = N'STD')
    INSERT dbo.RoomType (Id, IDType, NameType) VALUES (2, N'STD', N'Phòng Standard');
IF NOT EXISTS (SELECT 1 FROM dbo.RoomType WHERE IDType = N'SUI')
    INSERT dbo.RoomType (Id, IDType, NameType) VALUES (3, N'SUI', N'Phòng Suite');
SET IDENTITY_INSERT dbo.RoomType OFF;
GO

/* Phong khach san */
SET IDENTITY_INSERT dbo.HotelRoom ON;
IF NOT EXISTS (SELECT 1 FROM dbo.HotelRoom WHERE RoomID = 1)
    INSERT dbo.HotelRoom (RoomID, NameRoom, DescriptionRoom, RoomType, Price, ImageRoom)
    VALUES (1, N'Phòng Deluxe 101', N'Phòng rộng, đầy đủ tiện nghi', N'DLX', 800000.00, N'~/Images/phong-doi.jpg');
IF NOT EXISTS (SELECT 1 FROM dbo.HotelRoom WHERE RoomID = 2)
    INSERT dbo.HotelRoom (RoomID, NameRoom, DescriptionRoom, RoomType, Price, ImageRoom)
    VALUES (2, N'Phòng Standard 102', N'Phòng tiêu chuẩn', N'STD', 500000.00, N'~/Images/phong-tieu-chuan.jpg');
IF NOT EXISTS (SELECT 1 FROM dbo.HotelRoom WHERE RoomID = 5)
    INSERT dbo.HotelRoom (RoomID, NameRoom, DescriptionRoom, RoomType, Price, ImageRoom)
    VALUES (5, N'phòng vuýt hehehe', NULL, N'STD', 20000000.00, NULL);
SET IDENTITY_INSERT dbo.HotelRoom OFF;
GO

/* Khach hang */
SET IDENTITY_INSERT dbo.Customer ON;
IF NOT EXISTS (SELECT 1 FROM dbo.Customer WHERE IDCus = 1)
    INSERT dbo.Customer (IDCus, NameCus, PhoneCus, EmailCus, PasswordCus)
    VALUES (1, N'Nguyễn Văn An', N'0901234567', N'an@gmail.com', NULL);
IF NOT EXISTS (SELECT 1 FROM dbo.Customer WHERE IDCus = 2)
    INSERT dbo.Customer (IDCus, NameCus, PhoneCus, EmailCus, PasswordCus)
    VALUES (2, N'Trần Thị Bình', N'0912345678', N'binh@gmail.com', NULL);
IF NOT EXISTS (SELECT 1 FROM dbo.Customer WHERE IDCus = 3)
    INSERT dbo.Customer (IDCus, NameCus, PhoneCus, EmailCus, PasswordCus)
    VALUES (3, N'Lê Văn Cường', N'0923456789', N'cuong@gmail.com', NULL);
IF NOT EXISTS (SELECT 1 FROM dbo.Customer WHERE IDCus = 4)
    INSERT dbo.Customer (IDCus, NameCus, PhoneCus, EmailCus, PasswordCus)
    VALUES (4, N'cavinhtien', N'09305800', N'cavinhtgrgien12@gmail.com', NULL);
IF NOT EXISTS (SELECT 1 FROM dbo.Customer WHERE IDCus = 5)
    INSERT dbo.Customer (IDCus, NameCus, PhoneCus, EmailCus, PasswordCus)
    VALUES (5, N'cavinhtien', N'0940390309430', N'cavinhtien2@gmail.com', N'tien123n');
IF NOT EXISTS (SELECT 1 FROM dbo.Customer WHERE IDCus = 6)
    INSERT dbo.Customer (IDCus, NameCus, PhoneCus, EmailCus, PasswordCus)
    VALUES (6, N'cavinhtien', N'0940390309430', N'cavinhtien2@gmail.com', N'tien123n');
IF NOT EXISTS (SELECT 1 FROM dbo.Customer WHERE IDCus = 8)
    INSERT dbo.Customer (IDCus, NameCus, PhoneCus, EmailCus, PasswordCus)
    VALUES (8, N'cjdsabjeb', N'534654654645', N'cavinhtien12@gmail.com', N'tien123n');
SET IDENTITY_INSERT dbo.Customer OFF;
GO
/* đơn đặt phòng */
SET IDENTITY_INSERT dbo.Booking ON;
IF NOT EXISTS (SELECT 1 FROM dbo.Booking WHERE ID = 1)
    INSERT dbo.Booking (ID, DateBooking, IDCus, AddressDelivery, CheckOut, Status)
    VALUES (1, '2026-11-08', 1, NULL, NULL, N'Đã đặt');
IF NOT EXISTS (SELECT 1 FROM dbo.Booking WHERE ID = 2)
    INSERT dbo.Booking (ID, DateBooking, IDCus, AddressDelivery, CheckOut, Status)
    VALUES (2, '2026-11-08', 1, N'', NULL, N'Đã đặt');
IF NOT EXISTS (SELECT 1 FROM dbo.Booking WHERE ID = 3)
    INSERT dbo.Booking (ID, DateBooking, IDCus, AddressDelivery, CheckOut, Status)
    VALUES (3, '2026-11-12', 3, NULL, NULL, N'Đã đặt');
IF NOT EXISTS (SELECT 1 FROM dbo.Booking WHERE ID = 4)
    INSERT dbo.Booking (ID, DateBooking, IDCus, AddressDelivery, CheckOut, Status)
    VALUES (4, '2026-11-08', 5, N'', '2026-11-11', N'Đã đặt');
IF NOT EXISTS (SELECT 1 FROM dbo.Booking WHERE ID = 5)
    INSERT dbo.Booking (ID, DateBooking, IDCus, AddressDelivery, CheckOut, Status)
    VALUES (5, '2026-11-11', 5, N'', '2026-11-19', N'Đã đặt');
IF NOT EXISTS (SELECT 1 FROM dbo.Booking WHERE ID = 6)
    INSERT dbo.Booking (ID, DateBooking, IDCus, AddressDelivery, CheckOut, Status)
    VALUES (6, '2026-11-10', 5, N'', '2026-11-11', N'Đã đặt');
IF NOT EXISTS (SELECT 1 FROM dbo.Booking WHERE ID = 7)
    INSERT dbo.Booking (ID, DateBooking, IDCus, AddressDelivery, CheckOut, Status)
    VALUES (7, '2026-11-09', 5, N'', '2026-11-13', N'Đã nhận phòng');
IF NOT EXISTS (SELECT 1 FROM dbo.Booking WHERE ID = 8)
    INSERT dbo.Booking (ID, DateBooking, IDCus, AddressDelivery, CheckOut, Status)
    VALUES (8, '2026-11-25', 8, N'', '2026-11-26', N'Đã đặt');
IF NOT EXISTS (SELECT 1 FROM dbo.Booking WHERE ID = 9)
    INSERT dbo.Booking (ID, DateBooking, IDCus, AddressDelivery, CheckOut, Status)
    VALUES (9, '2026-11-30', 8, N'', '2026-12-16', N'Đã đặt');
IF NOT EXISTS (SELECT 1 FROM dbo.Booking WHERE ID = 10)
    INSERT dbo.Booking (ID, DateBooking, IDCus, AddressDelivery, CheckOut, Status)
    VALUES (10, '2027-01-14', 8, N'', '2027-01-15', N'Đã đặt');
IF NOT EXISTS (SELECT 1 FROM dbo.Booking WHERE ID = 11)
    INSERT dbo.Booking (ID, DateBooking, IDCus, AddressDelivery, CheckOut, Status)
    VALUES (11, '2027-01-01', 8, N'', '2027-01-02', N'Đã đặt');
IF NOT EXISTS (SELECT 1 FROM dbo.Booking WHERE ID = 12)
    INSERT dbo.Booking (ID, DateBooking, IDCus, AddressDelivery, CheckOut, Status)
    VALUES (12, '2026-11-24', 8, N'', '2026-11-25', N'Đã đặt');
SET IDENTITY_INSERT dbo.Booking OFF;
GO

SET IDENTITY_INSERT dbo.BookingDetail ON;
IF NOT EXISTS (SELECT 1 FROM dbo.BookingDetail WHERE ID = 1)
    INSERT dbo.BookingDetail (ID, IDRoom, IDBooking, Quantity, UnitPrice) VALUES (1, 1, 1, 1, 800000);
IF NOT EXISTS (SELECT 1 FROM dbo.BookingDetail WHERE ID = 2)
    INSERT dbo.BookingDetail (ID, IDRoom, IDBooking, Quantity, UnitPrice) VALUES (2, 1, 2, 1, 800000);
IF NOT EXISTS (SELECT 1 FROM dbo.BookingDetail WHERE ID = 3)
    INSERT dbo.BookingDetail (ID, IDRoom, IDBooking, Quantity, UnitPrice) VALUES (3, 1, 3, 1, 800000);
IF NOT EXISTS (SELECT 1 FROM dbo.BookingDetail WHERE ID = 4)
    INSERT dbo.BookingDetail (ID, IDRoom, IDBooking, Quantity, UnitPrice) VALUES (4, 1, 4, 1, 800000);
IF NOT EXISTS (SELECT 1 FROM dbo.BookingDetail WHERE ID = 5)
    INSERT dbo.BookingDetail (ID, IDRoom, IDBooking, Quantity, UnitPrice) VALUES (5, 5, 4, 1, 20000000);
IF NOT EXISTS (SELECT 1 FROM dbo.BookingDetail WHERE ID = 6)
    INSERT dbo.BookingDetail (ID, IDRoom, IDBooking, Quantity, UnitPrice) VALUES (6, 1, 5, 1, 800000);
IF NOT EXISTS (SELECT 1 FROM dbo.BookingDetail WHERE ID = 7)
    INSERT dbo.BookingDetail (ID, IDRoom, IDBooking, Quantity, UnitPrice) VALUES (7, 1, 6, 1, 800000);
IF NOT EXISTS (SELECT 1 FROM dbo.BookingDetail WHERE ID = 8)
    INSERT dbo.BookingDetail (ID, IDRoom, IDBooking, Quantity, UnitPrice) VALUES (8, 1, 7, 1, 800000);
IF NOT EXISTS (SELECT 1 FROM dbo.BookingDetail WHERE ID = 9)
    INSERT dbo.BookingDetail (ID, IDRoom, IDBooking, Quantity, UnitPrice) VALUES (9, 1, 8, 1, 800000);
IF NOT EXISTS (SELECT 1 FROM dbo.BookingDetail WHERE ID = 10)
    INSERT dbo.BookingDetail (ID, IDRoom, IDBooking, Quantity, UnitPrice) VALUES (10, 1, 9, 1, 800000);
IF NOT EXISTS (SELECT 1 FROM dbo.BookingDetail WHERE ID = 11)
    INSERT dbo.BookingDetail (ID, IDRoom, IDBooking, Quantity, UnitPrice) VALUES (11, 1, 10, 1, 800000);
IF NOT EXISTS (SELECT 1 FROM dbo.BookingDetail WHERE ID = 12)
    INSERT dbo.BookingDetail (ID, IDRoom, IDBooking, Quantity, UnitPrice) VALUES (12, 1, 11, 1, 800000);
IF NOT EXISTS (SELECT 1 FROM dbo.BookingDetail WHERE ID = 13)
    INSERT dbo.BookingDetail (ID, IDRoom, IDBooking, Quantity, UnitPrice) VALUES (13, 1, 12, 1, 800000);
SET IDENTITY_INSERT dbo.BookingDetail OFF;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.AdminUser WHERE ID = 1)
BEGIN
    INSERT dbo.AdminUser (ID, NameUser, RoleUser, PasswordUser)
    VALUES (1, N'admin', N'Admin', N'Admin@123');
END;
GO

