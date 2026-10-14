/*==========================================================
  RaceDay Event Management System
  SQL Server Schema Modifications & Enhancements Script
  File: RaceDayDBChanges.sql
  Compatible with SQL Server / Visual Studio T-SQL Editor
==========================================================*/

USE RaceDayDB;
GO

BEGIN TRANSACTION;

PRINT 'Starting RaceDayDB schema update and database changes...';

-- =========================================================
-- 1. ENHANCE USERS TABLE
-- Ensure Email index is optimized and ensure Phone column length
-- =========================================================

IF NOT EXISTS (
    SELECT * FROM sys.indexes 
    WHERE name = 'IX_Users_Email' AND object_id = OBJECT_ID('Users')
)
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX IX_Users_Email 
    ON Users(email);
    PRINT 'Created unique index IX_Users_Email on Users table.';
END;
GO

-- =========================================================
-- 2. ENHANCE PARTICIPANT PROFILES TABLE
-- Add optional updated_at column for tracking profile edits
-- =========================================================

IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE Name = 'updated_at' AND Object_ID = OBJECT_ID('ParticipantProfiles')
)
BEGIN
    ALTER TABLE ParticipantProfiles 
    ADD updated_at DATETIME NULL;
    PRINT 'Added updated_at column to ParticipantProfiles table.';
END;
GO

-- =========================================================
-- 3. ENHANCE EVENTS TABLE
-- Add status column (e.g., Upcoming, Ongoing, Completed, Cancelled)
-- =========================================================

IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE Name = 'status' AND Object_ID = OBJECT_ID('Events')
)
BEGIN
    ALTER TABLE Events 
    ADD status VARCHAR(20) DEFAULT 'Upcoming' NOT NULL;
    PRINT 'Added status column to Events table.';
END;
GO

-- =========================================================
-- 4. ENHANCE ENROLMENTS TABLE
-- Add non-clustered index on Profile and Category for faster API lookups
-- =========================================================

IF NOT EXISTS (
    SELECT * FROM sys.indexes 
    WHERE name = 'IX_Enrolments_Profile_Category' AND object_id = OBJECT_ID('Enrolments')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Enrolments_Profile_Category 
    ON Enrolments(profile_id, category_id);
    PRINT 'Created non-clustered index IX_Enrolments_Profile_Category.';
END;
GO

-- =========================================================
-- 5. ENHANCE RESULTS TABLE
-- Add position check and time_seconds index for leaderboard queries
-- =========================================================

IF NOT EXISTS (
    SELECT * FROM sys.indexes 
    WHERE name = 'IX_Results_Position_Time' AND object_id = OBJECT_ID('Results')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Results_Position_Time 
    ON Results(position, time_seconds);
    PRINT 'Created index IX_Results_Position_Time on Results table.';
END;
GO

-- =========================================================
-- 6. DATA CORRECTIONS & API TEST DATA SEEDING
-- Adjust dates to align with current testing scenarios
-- =========================================================

-- Update existing event dates to ensure upcoming test events exist
UPDATE Events
SET event_date = DATEADD(month, 2, GETDATE())
WHERE event_name = 'Cape Town Marathon';

UPDATE Events
SET event_date = DATEADD(month, 3, GETDATE())
WHERE event_name = 'Table Mountain Trail Run';

PRINT 'Updated event dates to align with current API test suite schedule.';

COMMIT TRANSACTION;
PRINT 'RaceDayDBChanges.sql script executed successfully.';
GO

-- =========================================================
-- VERIFICATION QUERIES
-- =========================================================

SELECT 'Events Table Structure' AS QueryResult;
SELECT column_name, data_type, is_nullable 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE table_name = 'Events';

SELECT 'Indexes Created' AS QueryResult;
SELECT name AS IndexName, OBJECT_NAME(object_id) AS TableName 
FROM sys.indexes 
WHERE name LIKE 'IX_%';
