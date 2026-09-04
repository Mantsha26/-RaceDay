-- =========================================================
-- DATA6222 - DATABASE (INTERMEDIATE)
-- ASSIGNMENT 1
-- ART GALLERY DATABASE
-- =========================================================
-- =========================================================
-- RESET DATABASE FOR TESTING
-- =========================================================

USE master;
GO

IF DB_ID(N'ArtGalleryDB') IS NOT NULL
BEGIN
    ALTER DATABASE ArtGalleryDB
    SET SINGLE_USER
    WITH ROLLBACK IMMEDIATE;

    DROP DATABASE ArtGalleryDB;
END
GO

CREATE DATABASE ArtGalleryDB;
GO

USE ArtGalleryDB;
GO

-- =========================================================
-- STEP 1: CREATE DATABASE
-- =========================================================

IF DB_ID(N'ArtGalleryDB') IS NULL
BEGIN
    CREATE DATABASE ArtGalleryDB;
END
GO

USE ArtGalleryDB;
GO


-- =========================================================
-- OPTIONAL: REMOVE OLD TABLES
-- This makes the script safe to run again during testing.
-- =========================================================

IF OBJECT_ID(N'Entry', N'U') IS NOT NULL
    DROP TABLE Entry;
GO

IF OBJECT_ID(N'Artwork', N'U') IS NOT NULL
    DROP TABLE Artwork;
GO

IF OBJECT_ID(N'Exhibition', N'U') IS NOT NULL
    DROP TABLE Exhibition;
GO

IF OBJECT_ID(N'Artist', N'U') IS NOT NULL
    DROP TABLE Artist;
GO

IF OBJECT_ID(N'Genre', N'U') IS NOT NULL
    DROP TABLE Genre;
GO


-- =========================================================
-- STEP 1.1: CREATE GENRE TABLE
-- =========================================================

CREATE TABLE Genre
(
    GenreID INT PRIMARY KEY,
    Description VARCHAR(100) NOT NULL
);
GO


-- =========================================================
-- STEP 1.2: CREATE ARTIST TABLE
-- =========================================================

CREATE TABLE Artist
(
    ArtistID INT IDENTITY(1,1) PRIMARY KEY,
    Name VARCHAR(50) NOT NULL,
    Surname VARCHAR(50) NOT NULL
);
GO


-- =========================================================
-- STEP 1.3: CREATE ARTWORK TABLE
-- =========================================================

CREATE TABLE Artwork
(
    ArtworkID INT IDENTITY(1,1) PRIMARY KEY,
    GenreID INT NOT NULL,
    ArtistID INT NOT NULL,
    Title VARCHAR(150) NOT NULL,

    CONSTRAINT FK_Artwork_Genre
        FOREIGN KEY (GenreID)
        REFERENCES Genre(GenreID),

    CONSTRAINT FK_Artwork_Artist
        FOREIGN KEY (ArtistID)
        REFERENCES Artist(ArtistID)
);
GO


-- =========================================================
-- STEP 1.4: CREATE EXHIBITION TABLE
-- =========================================================

CREATE TABLE Exhibition
(
    ExhibitionID INT IDENTITY(1,1) PRIMARY KEY,
    Description VARCHAR(150) NOT NULL
);
GO


-- =========================================================
-- STEP 1.5: CREATE ENTRY JUNCTION TABLE
-- =========================================================

CREATE TABLE Entry
(
    EntryID INT IDENTITY(1,1) PRIMARY KEY,
    ArtworkID INT NOT NULL,
    ExhibitionID INT NOT NULL,

    CONSTRAINT FK_Entry_Artwork
        FOREIGN KEY (ArtworkID)
        REFERENCES Artwork(ArtworkID)
        ON DELETE CASCADE,

    CONSTRAINT FK_Entry_Exhibition
        FOREIGN KEY (ExhibitionID)
        REFERENCES Exhibition(ExhibitionID)
);
GO


-- =========================================================
-- STEP 2: POPULATE DATABASE
-- =========================================================


-- =========================================================
-- STEP 2.1: INSERT GENRES
-- =========================================================

INSERT INTO Genre (GenreID, Description)
VALUES
(1, 'Surrealism'),
(2, 'Impressionism'),
(3, 'Abstract'),
(4, 'Realism'),
(5, 'Pop Art');
GO


-- =========================================================
-- STEP 2.2: INSERT ARTISTS
-- =========================================================

INSERT INTO Artist (Name, Surname)
VALUES
('Vincent', 'van Gogh'),
('Pablo', 'Picasso'),
('Claude', 'Monet'),
('Salvador', 'Dali'),
('Frida', 'Kahlo');
GO


-- =========================================================
-- STEP 2.3: INSERT ARTWORKS
-- Minimum required: 20
-- =========================================================

INSERT INTO Artwork (GenreID, ArtistID, Title)
VALUES
(2, 1, 'Starry Night'),
(2, 1, 'Sunflowers'),
(2, 1, 'The Potato Eaters'),
(2, 1, 'Irises'),

(3, 2, 'Guernica'),
(3, 2, 'The Weeping Woman'),
(3, 2, 'Les Demoiselles d''Avignon'),
(3, 2, 'The Old Guitarist'),

(2, 3, 'Water Lilies'),
(2, 3, 'Impression, Sunrise'),
(2, 3, 'Woman with a Parasol'),
(2, 3, 'The Magpie'),

(1, 4, 'The Persistence of Memory'),
(1, 4, 'Swans Reflecting Elephants'),
(1, 4, 'The Burning Giraffe'),
(1, 4, 'Metamorphosis of Narcissus'),

(4, 5, 'The Two Fridas'),
(4, 5, 'Self-Portrait with Thorn Necklace'),
(4, 5, 'The Broken Column'),
(4, 5, 'The Wounded Deer');
GO


-- =========================================================
-- STEP 2.4: INSERT EXHIBITIONS
-- Minimum required: 15
-- =========================================================

INSERT INTO Exhibition (Description)
VALUES
('Spring Art Expo'),
('Modern Masterpieces'),
('Surreal Visions'),
('Impressionist Era'),
('Global Canvas 2026'),
('Abstract Expressions'),
('Parisian Retrospective'),
('Classical Realism'),
('Contemporary Echoes'),
('African Art Summit'),
('Heritage & Vision'),
('Avant-Garde Showcase'),
('Colors of Summer'),
('The Masterworks'),
('Annual Fine Arts Gala');
GO


-- =========================================================
-- STEP 2.5: INSERT ENTRIES
-- Artwork 1 appears in multiple exhibitions.
-- This is important for the HAVING query.
-- =========================================================

INSERT INTO Entry (ArtworkID, ExhibitionID)
VALUES
(1, 1),
(1, 4),
(1, 14),

(2, 1),
(3, 2),
(4, 4),

(5, 2),
(6, 6),
(7, 6),
(8, 2),

(9, 4),
(10, 4),
(11, 13),
(12, 4),

(13, 3),
(14, 3),
(15, 3),
(16, 3),

(17, 8),
(18, 8),
(19, 8),
(20, 8);
GO


-- =========================================================
-- STEP 3: UPDATE STATEMENT
-- =========================================================

UPDATE Artwork
SET Title = 'The Persistence of Memory (Restored Edition)'
WHERE ArtworkID = 13;
GO


-- =========================================================
-- STEP 4: DELETE STATEMENT
-- Delete Artwork 20.
--
-- Because Entry has ON DELETE CASCADE, deleting the
-- artwork automatically deletes its related Entry record.
-- =========================================================

DELETE FROM Artwork
WHERE ArtworkID = 20;
GO


-- =========================================================
-- STEP 5: ARTWORK REPORT
-- ORDER BY
--
-- Displays:
-- Genre
-- Artwork Title
-- Artist Name
--
-- Results are sorted alphabetically by Genre and Title.
-- =========================================================

SELECT
    g.Description AS [Genre],
    a.Title AS [Artwork Title],
    CONCAT(ar.Name, ' ', ar.Surname) AS [Artist Name]
FROM Artwork AS a
INNER JOIN Genre AS g
    ON a.GenreID = g.GenreID
INNER JOIN Artist AS ar
    ON a.ArtistID = ar.ArtistID
ORDER BY
    g.Description ASC,
    a.Title ASC;
GO


-- =========================================================
-- STEP 6: GROUP BY REPORT
--
-- Displays the total number of artworks created by
-- each artist.
--
-- LEFT JOIN ensures that an artist will still appear
-- even if they have no artwork.
-- =========================================================

SELECT
    CONCAT(ar.Name, ' ', ar.Surname) AS [Artist Name],
    COUNT(a.ArtworkID) AS [Total Artworks]
FROM Artist AS ar
LEFT JOIN Artwork AS a
    ON ar.ArtistID = a.ArtistID
GROUP BY
    ar.ArtistID,
    ar.Name,
    ar.Surname
ORDER BY
    [Total Artworks] DESC,
    [Artist Name] ASC;
GO


-- =========================================================
-- STEP 7: HAVING CLAUSE REPORT
--
-- Displays artworks that appear in MORE THAN ONE
-- exhibition.
-- =========================================================

SELECT
    a.ArtworkID,
    a.Title AS [Artwork Title],
    COUNT(e.ExhibitionID) AS [Exhibition Count]
FROM Artwork AS a
INNER JOIN Entry AS e
    ON a.ArtworkID = e.ArtworkID
GROUP BY
    a.ArtworkID,
    a.Title
HAVING COUNT(e.ExhibitionID) > 1
ORDER BY
    [Exhibition Count] DESC,
    [Artwork Title] ASC;
GO


-- =========================================================
-- STEP 8: JOINS REPORT
--
-- Displays a complete catalogue containing:
-- Artwork
-- Artist
-- Genre
-- Exhibition
--
-- LEFT JOIN is used for Entry and Exhibition so that
-- artworks without an exhibition can still be displayed.
-- =========================================================

SELECT
    a.ArtworkID,
    a.Title AS [Artwork Title],
    CONCAT(ar.Name, ' ', ar.Surname) AS [Artist Name],
    g.Description AS [Genre],
    ex.Description AS [Exhibition Name]
FROM Artwork AS a
INNER JOIN Artist AS ar
    ON a.ArtistID = ar.ArtistID
INNER JOIN Genre AS g
    ON a.GenreID = g.GenreID
LEFT JOIN Entry AS en
    ON a.ArtworkID = en.ArtworkID
LEFT JOIN Exhibition AS ex
    ON en.ExhibitionID = ex.ExhibitionID
ORDER BY
    a.ArtworkID ASC;
GO


-- =========================================================
-- STEP 9: VERIFY THE DATA
-- =========================================================

SELECT * FROM Genre;
SELECT * FROM Artist;
SELECT * FROM Artwork;
SELECT * FROM Exhibition;
SELECT * FROM Entry;
GO