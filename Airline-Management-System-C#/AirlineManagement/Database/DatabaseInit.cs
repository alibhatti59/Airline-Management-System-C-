using System;
using System.Data.SqlClient;

namespace AirlineManagement.Database
{
    public static class DatabaseInit
    {
        public static void EnsureTablesExist()
        {
            var script = @"
IF OBJECT_ID('dbo.Flights','U') IS NULL
BEGIN
    CREATE TABLE dbo.Flights (
        FlightCode NVARCHAR(50) NOT NULL PRIMARY KEY,
        Source NVARCHAR(100) NOT NULL,
        Destination NVARCHAR(100) NOT NULL,
        TakeOffDate DATETIME NOT NULL,
        Seats INT NOT NULL
    )
END

IF OBJECT_ID('dbo.PassengerTbl','U') IS NULL
BEGIN
    CREATE TABLE dbo.PassengerTbl (
        PassengerId NVARCHAR(50) NOT NULL PRIMARY KEY,
        PassengerName NVARCHAR(200) NULL,
        PassengerAdress NVARCHAR(300) NULL,
        Passport NVARCHAR(100) NULL,
        PassengerNationality NVARCHAR(100) NULL,
        PassengerGender NVARCHAR(50) NULL,
        PassengerPhoneNumber NVARCHAR(50) NULL
    )
END

IF OBJECT_ID('dbo.Tickets','U') IS NULL
BEGIN
    CREATE TABLE dbo.Tickets (
        TicketId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        FlightCode NVARCHAR(50) NULL,
        PassId NVARCHAR(50) NULL,
        PassName NVARCHAR(200) NULL,
        PassPassport NVARCHAR(100) NULL,
        PassNationality NVARCHAR(100) NULL,
        PassAge INT NULL,
        TicketClass NVARCHAR(50) NULL,
        Amount DECIMAL(18,2) NULL
    )
    -- Add foreign keys if referenced tables exist
    IF OBJECT_ID('dbo.Flights','U') IS NOT NULL
        ALTER TABLE dbo.Tickets ADD CONSTRAINT FK_Tickets_Flights FOREIGN KEY (FlightCode) REFERENCES dbo.Flights(FlightCode);
    IF OBJECT_ID('dbo.PassengerTbl','U') IS NOT NULL
        ALTER TABLE dbo.Tickets ADD CONSTRAINT FK_Tickets_Passenger FOREIGN KEY (PassId) REFERENCES dbo.PassengerTbl(PassengerId);
END
ELSE
BEGIN
    -- ensure Tickets has TicketClass and Amount columns
    IF COL_LENGTH('dbo.Tickets', 'TicketClass') IS NULL
    BEGIN
        ALTER TABLE dbo.Tickets ADD TicketClass NVARCHAR(50) NULL;
    END
    IF COL_LENGTH('dbo.Tickets', 'Amount') IS NULL
    BEGIN
        ALTER TABLE dbo.Tickets ADD Amount DECIMAL(18,2) NULL;
    END
END

IF OBJECT_ID('dbo.Users','U') IS NULL
BEGIN
    CREATE TABLE dbo.Users (
        UserId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Username NVARCHAR(100) NOT NULL UNIQUE,
        Password NVARCHAR(200) NOT NULL,
        Role NVARCHAR(50) NOT NULL DEFAULT('Passenger')
    )
END
ELSE
BEGIN
    -- If the Users table exists but Role column is missing, add it with a default
    IF COL_LENGTH('dbo.Users', 'Role') IS NULL
    BEGIN
        ALTER TABLE dbo.Users ADD Role NVARCHAR(50) NOT NULL CONSTRAINT DF_Users_Role DEFAULT('Passenger');
    END
END

-- Ensure Users has PassengerId column to link to PassengerTbl
IF COL_LENGTH('dbo.Users', 'PassengerId') IS NULL
BEGIN
    ALTER TABLE dbo.Users ADD PassengerId NVARCHAR(50) NULL;
    IF COL_LENGTH('dbo.Users', 'PassengerId') IS NOT NULL
    BEGIN
        EXEC sp_addextendedproperty 'MS_Description', 'Linked passenger id for passenger users', 'SCHEMA', 'dbo', 'TABLE', 'Users', 'COLUMN', 'PassengerId';
    END
END
";

            try
            {
                using (var conn = Db.CreateConnection())
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(script, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Database initialization error: " + ex.Message);
            }
        }
    }
}
