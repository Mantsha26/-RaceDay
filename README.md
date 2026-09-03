# RaceDay Event Management System

## Project Overview

The RaceDay Event Management System is a REST API and database solution designed to manage running events. The system allows organisers to create race events and categories while participants register, enrol in races, and view their results.

The project is divided into three POE sections:

* **Section A:** Entity Relationship Diagram (ERD)
* **Section B:** API Endpoint Planning
* **Section C:** SQL Server Database Script

---

## User Roles

### Organiser

* Register and log in.
* Create race events.
* Create event categories.
* Update and delete events.
* Record participant results.

### Participant

* Register and log in.
* Update personal profile.
* View available events.
* Enrol into race categories.
* View personal enrolments and results.

---

## Technologies Used

* SQL Server
* SQL Server Management Studio (SSMS)
* GitHub
* GitHub Actions
* REST API Design Principles

---

## Project Folder Structure


RaceDay/
├── .github/
│   └── workflows/
│       └── validate-docs.yml
├── docs/
│   ├── Section_A_RaceDay_ERD.pdf
│   ├── Section_B_API_Endpoint_Plan.pdf
│   └──  RaceDayDB.sql
|
├── .gitattributes
├── .gitignore
├── LICENSE
└── README.md



---

## Setup Instructions

1. Clone the repository.
2. Open SQL Server Management Studio.
3. Execute **RaceDayDB.sql**.
4. Verify that the database and sample data are created successfully.

---

## Continuous Integration (GitHub Actions)

A GitHub Actions workflow validates that:

* The `/docs` folder exists.
* The ERD document exists.
* The API Endpoint Plan exists.
* The SQL database script exists.

A successful workflow run produces a green build.

GitHub Actions Build


<img width="1906" height="800" alt="image" src="https://github.com/user-attachments/assets/ffe74d25-63e4-47ee-959f-c63478e30d0f" />



YouTube Link

(Add your GitHub repository URL here.)

Example:

https://github.com/YourUsername/RaceDay-Event-Management-System


