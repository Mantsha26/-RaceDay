/*==========================================================
  RaceDay Event Management System
  SQL Server Database Script
  POE Section C
  Compatible with SQL Server Management Studio (SSMS)
==========================================================*/

-- ==========================================
-- CREATE DATABASE
-- ==========================================

IF DB_ID('RaceDayDB') IS NOT NULL
BEGIN
    DROP DATABASE RaceDayDB;
END;
GO

CREATE DATABASE RaceDayDB;
GO

USE RaceDayDB;
GO

/*==========================================================
  TABLE: Users
==========================================================*/

CREATE TABLE Users (
    user_id INT IDENTITY(1,1) PRIMARY KEY,
    first_name VARCHAR(50) NOT NULL,
    last_name VARCHAR(50) NOT NULL,
    email VARCHAR(100) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    role VARCHAR(20) NOT NULL
        CHECK (role IN ('Admin','Organiser','Participant')),
    phone VARCHAR(20) UNIQUE,
    created_at DATETIME DEFAULT GETDATE()
);

GO

/*==========================================================
  TABLE: Organisers
==========================================================*/

CREATE TABLE Organisers (
    organiser_id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NOT NULL UNIQUE,
    organisation_name VARCHAR(100) NOT NULL,
    description VARCHAR(255),
    created_at DATETIME DEFAULT GETDATE(),

    CONSTRAINT FK_Organiser_User
        FOREIGN KEY(user_id)
        REFERENCES Users(user_id)
);

GO

/*==========================================================
  TABLE: ParticipantProfiles
==========================================================*/

CREATE TABLE ParticipantProfiles (
    profile_id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NOT NULL UNIQUE,
    date_of_birth DATE NOT NULL,
    gender VARCHAR(20)
        CHECK (gender IN ('Male','Female','Other')),
    address VARCHAR(150),
    emergency_contact VARCHAR(100),
    created_at DATETIME DEFAULT GETDATE(),

    CONSTRAINT FK_Profile_User
        FOREIGN KEY(user_id)
        REFERENCES Users(user_id)
);

GO

/*==========================================================
  TABLE: Events
==========================================================*/

CREATE TABLE Events (
    event_id INT IDENTITY(1,1) PRIMARY KEY,
    organiser_id INT NOT NULL,
    event_name VARCHAR(100) NOT NULL,
    event_date DATE NOT NULL,
    location VARCHAR(100) NOT NULL,
    description VARCHAR(255),
    event_image_url VARCHAR(255),
    created_at DATETIME DEFAULT GETDATE(),

    CONSTRAINT FK_Event_Organiser
        FOREIGN KEY(organiser_id)
        REFERENCES Organisers(organiser_id)
);

GO

/*==========================================================
  TABLE: Categories
==========================================================*/

CREATE TABLE Categories (
    category_id INT IDENTITY(1,1) PRIMARY KEY,
    event_id INT NOT NULL,
    name VARCHAR(50) NOT NULL,
    description VARCHAR(255),
    min_age INT NOT NULL,
    max_age INT NOT NULL,
    created_at DATETIME DEFAULT GETDATE(),

    CONSTRAINT FK_Category_Event
        FOREIGN KEY(event_id)
        REFERENCES Events(event_id),

    CONSTRAINT CHK_AgeRange
        CHECK (min_age <= max_age)
);

GO

/*==========================================================
  TABLE: Enrolments
==========================================================*/

CREATE TABLE Enrolments (
    enrolment_id INT IDENTITY(1,1) PRIMARY KEY,
    profile_id INT NOT NULL,
    category_id INT NOT NULL,
    enrolment_date DATETIME DEFAULT GETDATE(),
    status VARCHAR(20) DEFAULT 'Registered'
        CHECK (status IN ('Registered','Completed','Cancelled')),

    CONSTRAINT FK_Enrolment_Profile
        FOREIGN KEY(profile_id)
        REFERENCES ParticipantProfiles(profile_id),

    CONSTRAINT FK_Enrolment_Category
        FOREIGN KEY(category_id)
        REFERENCES Categories(category_id),

    CONSTRAINT UQ_Profile_Category
        UNIQUE(profile_id, category_id)
);

GO

/*==========================================================
  TABLE: Results
==========================================================*/

CREATE TABLE Results (
    result_id INT IDENTITY(1,1) PRIMARY KEY,
    enrolment_id INT NOT NULL UNIQUE,
    position INT CHECK(position > 0),
    time_seconds DECIMAL(8,2),
    points INT DEFAULT 0,
    created_at DATETIME DEFAULT GETDATE(),

    CONSTRAINT FK_Result_Enrolment
        FOREIGN KEY(enrolment_id)
        REFERENCES Enrolments(enrolment_id)
);

GO

/*==========================================================
  SAMPLE DATA
==========================================================*/

-------------------------
-- USERS
-------------------------

INSERT INTO Users
(first_name,last_name,email,password_hash,role,phone)
VALUES
('Sarah','Mokoena',
'[email protected]','hash123','Organiser','0821112233'),

('David','Naidoo',
'[email protected]','hash456','Organiser','0832223344'),

('John','Smith',
'[email protected]','hash789','Participant','0715551111'),

('Amina','Khan',
'[email protected]','hash987','Participant','0726662222'),

('Admin','User',
'[email protected]','adminhash','Admin','0800000000');

GO

-------------------------
-- ORGANISERS
-------------------------

INSERT INTO Organisers
(user_id,organisation_name,description)
VALUES
(1,'Cape Striders Club',
'Community road running organiser'),

(2,'Durban Athletics Association',
'Official athletics organiser');

GO

-------------------------
-- PARTICIPANT PROFILES
-------------------------

INSERT INTO ParticipantProfiles
(user_id,date_of_birth,gender,address,emergency_contact)
VALUES
(3,'2004-05-18','Male',
'12 Beach Road, Cape Town',
'Mary Smith - 0718881111'),

(4,'2006-11-02','Female',
'45 Palm Avenue, Durban',
'Ahmed Khan - 0729993333');

GO

-------------------------
-- EVENTS
-------------------------

INSERT INTO Events
(organiser_id,event_name,event_date,location,description,event_image_url)
VALUES
(1,'Cape Town Marathon',
'2026-10-10',
'Cape Town Stadium',
'Annual city marathon.',
'images/capetown.jpg'),

(1,'Table Mountain Trail Run',
'2026-11-01',
'Table Mountain',
'Mountain trail running event.',
'images/tablemountain.jpg'),

(2,'Durban Summer Fun Run',
'2026-12-05',
'Durban Beachfront',
'Family fun run along the beach.',
'images/durban.jpg');

GO

-------------------------
-- CATEGORIES
-------------------------

INSERT INTO Categories
(event_id,name,description,min_age,max_age)
VALUES

(1,'5 KM',
'Short distance marathon.',
12,60),

(1,'10 KM',
'Intermediate marathon category.',
15,65),

(1,'21 KM Half Marathon',
'Half marathon challenge.',
18,70),

(2,'10 KM Trail',
'Mountain trail race.',
18,60),

(2,'21 KM Trail',
'Long distance trail race.',
21,65),

(3,'3 KM Fun Run',
'Family friendly run.',
8,70),

(3,'5 KM Fun Run',
'Beachfront running category.',
12,65);

GO

-------------------------
-- ENROLMENTS
-------------------------

INSERT INTO Enrolments
(profile_id,category_id,status)
VALUES
(1,2,'Registered'),

(1,4,'Completed'),

(2,6,'Registered'),

(2,7,'Completed');

GO

-------------------------
-- RESULTS
-------------------------

INSERT INTO Results
(enrolment_id,position,time_seconds,points)
VALUES
(2,3,4520.50,18),

(4,1,1450.25,25);

GO
PRINT 'RaceDay Database Created Successfully';

SELECT COUNT(*) AS Users FROM Users;
SELECT COUNT(*) AS Organisers FROM Organisers;
SELECT COUNT(*) AS Participants FROM ParticipantProfiles;
SELECT COUNT(*) AS Events FROM Events;
SELECT COUNT(*) AS Categories FROM Categories;
SELECT COUNT(*) AS Enrolments FROM Enrolments;
SELECT COUNT(*) AS Results FROM Results;
/*==========================================================
  TEST QUERIES
==========================================================*/

-- View Users
SELECT * FROM Users;

-- View Organisers
SELECT * FROM Organisers;

-- View Events with Organiser
SELECT
E.event_name,
O.organisation_name,
E.location,
E.event_date
FROM Events E
JOIN Organisers O
ON E.organiser_id = O.organiser_id;

-- Participant Enrolments
SELECT
U.first_name + ' ' + U.last_name AS Participant,
E.event_name,
C.name AS Category,
EN.status
FROM Enrolments EN
JOIN ParticipantProfiles P
ON EN.profile_id = P.profile_id
JOIN Users U
ON P.user_id = U.user_id
JOIN Categories C
ON EN.category_id = C.category_id
JOIN Events E
ON C.event_id = E.event_id;

-- Results
SELECT
U.first_name + ' ' + U.last_name AS Participant,
EV.event_name,
CAT.name AS Category,
R.position,
R.time_seconds,
R.points
FROM Results R
JOIN Enrolments EN
ON R.enrolment_id = EN.enrolment_id
JOIN ParticipantProfiles PP
ON EN.profile_id = PP.profile_id
JOIN Users U
ON PP.user_id = U.user_id
JOIN Categories CAT
ON EN.category_id = CAT.category_id
JOIN Events EV
ON CAT.event_id = EV.event_id;

GO