

/*==========================================================
  RaceDay Event Management System
  Database Enhancements
  Database: RaceDayDB
  Prerequisite: InitialCreate migration must be applied.
==========================================================*/

USE RaceDayDB;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    PRINT 'Starting RaceDayDB enhancements...';

    -- 1. Verify required tables exist
    IF OBJECT_ID('dbo.Users', 'U') IS NULL
    BEGIN
        THROW 50001, 'Users table is missing. Apply InitialCreate first.', 1;
    END;

    IF OBJECT_ID('dbo.ParticipantProfiles', 'U') IS NULL
    BEGIN
        THROW 50002, 'ParticipantProfiles table is missing.', 1;
    END;

    IF OBJECT_ID('dbo.Events', 'U') IS NULL
    BEGIN
        THROW 50003, 'Events table is missing.', 1;
    END;

    IF OBJECT_ID('dbo.Enrolments', 'U') IS NULL
    BEGIN
        THROW 50004, 'Enrolments table is missing.', 1;
    END;

    IF OBJECT_ID('dbo.Results', 'U') IS NULL
    BEGIN
        THROW 50005, 'Results table is missing.', 1;
    END;


    -- 2. Users email index
    -- InitialCreate already creates IX_Users_email.
    IF NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE object_id = OBJECT_ID('dbo.Users')
          AND name = 'IX_Users_email'
    )
    BEGIN
        CREATE UNIQUE NONCLUSTERED INDEX IX_Users_email
        ON dbo.Users(email);

        PRINT 'Created Users email index.';
    END;


    -- 3. ParticipantProfiles updated_at column
    IF COL_LENGTH('dbo.ParticipantProfiles', 'updated_at') IS NULL
    BEGIN
        ALTER TABLE dbo.ParticipantProfiles
        ADD updated_at DATETIME NULL;

        PRINT 'Added ParticipantProfiles.updated_at.';
    END;


    -- 4. Events status column
    IF COL_LENGTH('dbo.Events', 'status') IS NULL
    BEGIN
        ALTER TABLE dbo.Events
        ADD status VARCHAR(20) NOT NULL
            CONSTRAINT DF_Events_Status DEFAULT ('Upcoming');

        PRINT 'Added Events.status.';
    END;


    -- 5. Enrolments index
    -- InitialCreate already creates a unique composite
    -- index on profile_id and category_id.
    IF NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE object_id = OBJECT_ID('dbo.Enrolments')
          AND name = 'IX_Enrolments_profile_id_category_id'
    )
    BEGIN
        CREATE UNIQUE NONCLUSTERED INDEX
            IX_Enrolments_profile_id_category_id
        ON dbo.Enrolments(profile_id, category_id);

        PRINT 'Created Enrolments composite index.';
    END;


    -- 6. Results leaderboard index
    IF NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE object_id = OBJECT_ID('dbo.Results')
          AND name = 'IX_Results_Position_Time'
    )
    BEGIN
        CREATE NONCLUSTERED INDEX IX_Results_Position_Time
        ON dbo.Results(position, time_seconds);

        PRINT 'Created Results leaderboard index.';
    END;


    -- 7. Existing migration already creates:
    -- Events.DistanceKm
    -- Events.EventType
    -- ParticipantProfiles.ProfilePictureUrl
    -- Do not add these columns again.


    -- 8. Optional: Update test event dates.
    -- Uncomment only if you want to change existing test data.

    /*
    UPDATE dbo.Events
    SET event_date = DATEADD(MONTH, 2, GETDATE())
    WHERE event_name = 'Cape Town Marathon';

    UPDATE dbo.Events
    SET event_date = DATEADD(MONTH, 3, GETDATE())
    WHERE event_name = 'Table Mountain Trail Run';
    */


    COMMIT TRANSACTION;

    PRINT 'RaceDayDB enhancements completed successfully.';

END TRY
BEGIN CATCH

    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    PRINT 'Database update failed.';
    PRINT ERROR_MESSAGE();

    THROW;

END CATCH;


-- 9. Verify database tables
SELECT
    TABLE_SCHEMA,
    TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;


-- 10. Verify important columns
SELECT
    TABLE_NAME,
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME IN (
    'Users',
    'ParticipantProfiles',
    'Events',
    'Enrolments',
    'Results'
)
ORDER BY TABLE_NAME, ORDINAL_POSITION;


-- 11. Verify indexes
SELECT
    name AS IndexName,
    OBJECT_NAME(object_id) AS TableName,
    is_unique AS IsUnique
FROM sys.indexes
WHERE name LIKE 'IX[_]%'
ORDER BY TableName, IndexName;
GO
