-- RaceDay_Schema.sql  (Part 3 - matches RaceDay.Api EF Core model)
-- Safe to re-run: every object is created only if missing, and the two
-- Azure Blob URL columns are added to older databases if they are absent.
-- Run in SSMS against your SQL Server instance (e.g. .\SQLEXPRESS).

IF DB_ID(N'RaceDayDb') IS NULL CREATE DATABASE RaceDayDb;
GO
USE RaceDayDb;
GO

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
CREATE TABLE dbo.Users (
    UserID          INT IDENTITY(1,1) PRIMARY KEY,
    FirstName       NVARCHAR(50)  NOT NULL,
    LastName        NVARCHAR(50)  NOT NULL,
    Email           NVARCHAR(100) NOT NULL,
    PasswordHash    NVARCHAR(255) NOT NULL,
    Role            NVARCHAR(20)  NOT NULL,
    Phone           NVARCHAR(20)  NULL,
    DateRegistered  DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    IsActive        BIT           NOT NULL DEFAULT 1,
    ProfileImageUrl NVARCHAR(500) NULL,            -- Azure Blob URL
    CONSTRAINT CK_Users_Role CHECK (Role IN ('Organiser','Participant'))
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_Email' AND object_id = OBJECT_ID('dbo.Users'))
    CREATE UNIQUE INDEX IX_Users_Email ON dbo.Users(Email);
GO

IF OBJECT_ID(N'dbo.EventTypes', N'U') IS NULL
CREATE TABLE dbo.EventTypes (
    EventTypeID INT IDENTITY(1,1) PRIMARY KEY,
    Name        NVARCHAR(30)  NOT NULL,
    Description NVARCHAR(200) NULL
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EventTypes_Name' AND object_id = OBJECT_ID('dbo.EventTypes'))
    CREATE UNIQUE INDEX IX_EventTypes_Name ON dbo.EventTypes(Name);
GO

IF OBJECT_ID(N'dbo.Events', N'U') IS NULL
CREATE TABLE dbo.Events (
    EventID         INT IDENTITY(1,1) PRIMARY KEY,
    EventName       NVARCHAR(100) NOT NULL,
    Description     NVARCHAR(500) NULL,
    EventDate       DATETIME2     NOT NULL,
    Location        NVARCHAR(150) NOT NULL,
    DistanceKm      DECIMAL(6,2)  NOT NULL,
    EventTypeID     INT           NOT NULL,
    OrganiserID     INT           NOT NULL,
    MaxParticipants INT           NULL,
    Status          NVARCHAR(20)  NOT NULL DEFAULT 'Open',
    CreatedDate     DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    BannerImageUrl  NVARCHAR(500) NULL,            -- Azure Blob URL
    CONSTRAINT FK_Events_EventTypes FOREIGN KEY (EventTypeID) REFERENCES dbo.EventTypes(EventTypeID),
    CONSTRAINT FK_Events_Organiser  FOREIGN KEY (OrganiserID) REFERENCES dbo.Users(UserID)
);
GO

IF OBJECT_ID(N'dbo.Categories', N'U') IS NULL
CREATE TABLE dbo.Categories (
    CategoryID   INT IDENTITY(1,1) PRIMARY KEY,
    EventID      INT           NOT NULL,
    CategoryName NVARCHAR(50)  NOT NULL,
    MinAge       INT           NULL,
    MaxAge       INT           NULL,
    EntryFee     DECIMAL(8,2)  NOT NULL DEFAULT 0,
    MaxEntries   INT           NULL,
    CONSTRAINT FK_Categories_Events FOREIGN KEY (EventID) REFERENCES dbo.Events(EventID) ON DELETE CASCADE
);
GO

IF OBJECT_ID(N'dbo.Enrolments', N'U') IS NULL
CREATE TABLE dbo.Enrolments (
    EnrolmentID   INT IDENTITY(1,1) PRIMARY KEY,
    ParticipantID INT          NOT NULL,
    EventID       INT          NOT NULL,
    CategoryID    INT          NOT NULL,
    EnrolmentDate DATETIME2    NOT NULL DEFAULT SYSUTCDATETIME(),
    Status        NVARCHAR(20) NOT NULL DEFAULT 'Confirmed',
    CONSTRAINT FK_Enrolments_Participant FOREIGN KEY (ParticipantID) REFERENCES dbo.Users(UserID),
    CONSTRAINT FK_Enrolments_Event       FOREIGN KEY (EventID)       REFERENCES dbo.Events(EventID),
    CONSTRAINT FK_Enrolments_Category    FOREIGN KEY (CategoryID)    REFERENCES dbo.Categories(CategoryID),
    CONSTRAINT UQ_Enrolment UNIQUE (ParticipantID, EventID)
);
GO

IF OBJECT_ID(N'dbo.Results', N'U') IS NULL
CREATE TABLE dbo.Results (
    ResultID     INT IDENTITY(1,1) PRIMARY KEY,
    EnrolmentID  INT           NOT NULL,
    FinishTime   TIME          NULL,
    Position     INT           NULL,
    Notes        NVARCHAR(200) NULL,
    CapturedBy   INT           NOT NULL,
    CapturedDate DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_Results_Enrolment  UNIQUE (EnrolmentID),
    CONSTRAINT FK_Results_Enrolment  FOREIGN KEY (EnrolmentID) REFERENCES dbo.Enrolments(EnrolmentID) ON DELETE CASCADE,
    CONSTRAINT FK_Results_CapturedBy FOREIGN KEY (CapturedBy)  REFERENCES dbo.Users(UserID)
);
GO

-- Upgrade path: databases created before Part 3 lack the Blob URL columns
IF COL_LENGTH('dbo.Events', 'BannerImageUrl') IS NULL
    ALTER TABLE dbo.Events ADD BannerImageUrl NVARCHAR(500) NULL;
IF COL_LENGTH('dbo.Users', 'ProfileImageUrl') IS NULL
    ALTER TABLE dbo.Users ADD ProfileImageUrl NVARCHAR(500) NULL;
GO

-- Seed lookup data (same IDs the API's EF seed uses)
IF NOT EXISTS (SELECT 1 FROM dbo.EventTypes)
BEGIN
    SET IDENTITY_INSERT dbo.EventTypes ON;
    INSERT INTO dbo.EventTypes (EventTypeID, Name, Description) VALUES
        (1, 'Running', 'Road running events'),
        (2, 'Walking', 'Race walking / fun walks'),
        (3, 'Cycling', 'Road cycling events');
    SET IDENTITY_INSERT dbo.EventTypes OFF;
END
GO
-- NOTE: no sample users are seeded. A placeholder password hash makes BCrypt
-- throw at login. Register Organiser/Participant accounts through the app.
