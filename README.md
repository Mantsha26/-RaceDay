# RaceDay Event Management System – Part 2 RESTful API

## 1. Project Overview

The RaceDay Event Management System is a RESTful Web API designed to manage running events, event categories, participant enrolments, and competition results.

The system enables organisers to create and manage races, while participants can register, log in, browse events, enrol in race categories, manage their profiles, and view their results.

This project forms part of the Programming 2B (PROG6212) Portfolio of Evidence (POE) and builds on the database design and API planning completed in Part 1.

### POE Sections

- **Section A:** Entity Relationship Diagram (ERD).
- **Section B:** API Endpoint Planning.
- **Section C:** SQL Server Database Script and RESTful Web API Implementation.

## 2. Key Features

### Authentication and Security

- User registration and login.
- Secure password hashing using BCrypt.
- Server-side session management.
- Role-based authorization for Organisers and Participants.
- Login and logout functionality.
- Validation of requests to protected endpoints.

### Organiser Features

- Register and log in.
- Create, view, update, and delete race events.
- Create and manage event categories.
- View participant enrolments for events.
- Record participant competition results.
- Update and delete results.

### Participant Features

- Register and log in.
- View available race events and categories.
- Enrol in event categories.
- Cancel personal enrolments.
- View and update personal profile information.
- View personal enrolments and results.

### API Documentation and Testing

- Interactive Swagger UI documentation.
- Entity Framework Core database integration.
- Automated unit tests using xUnit.
- GitHub Actions continuous integration.
- SQL Server database persistence.

## 3. Technologies Used

| Technology | Purpose |
|---|---|
| C# | Programming language |
| ASP.NET Core Web API | REST API development |
| .NET 8 | Application framework |
| Microsoft SQL Server | Relational database |
| SQL Server Management Studio (SSMS) | Database management |
| Entity Framework Core 8 | Database access and persistence |
| BCrypt.Net-Next | Password hashing |
| ASP.NET Core Session | Server-side session management |
| Swagger / OpenAPI | API documentation and testing |
| xUnit | Automated testing |
| EF Core In-Memory Provider | Isolated database testing |
| Git and GitHub | Version control and source-code hosting |
| GitHub Actions | Automated build and testing |

## 4. Project Folder Structure

```text
RaceDay/
├── .github/
│   └── workflows/
│       ├── validate-docs.yml
│       └── build-and-test.yml
├── docs/
│   ├── Section_A_RaceDay_ERD.pdf
│   ├── Section_B_API_Endpoint_Plan.pdf
│   ├── RaceDayDB.sql
│   
├── src/
│   └── RaceDayAPI/
│       ├── Attributes/
│       │   └── SessionAuthorizeAttribute.cs
│       ├── Controllers/
│       │   ├── AuthController.cs
│       │   ├── CategoriesController.cs
│       │   ├── EnrolmentsController.cs
│       │   ├── EventsController.cs
│       │   ├── ProfileController.cs
│       │   └── ResultsController.cs
│       ├── Data/
│       │   └── RaceDayDbContext.cs
│       ├── DTOs/
│       ├── Models/
│       ├── Program.cs
│       ├── appsettings.json
│       └── RaceDayAPI.csproj
├── tests/
│   └── RaceDayAPI.Tests/
│       ├── ApiIntegrationAndUnitTests.cs
│       └── RaceDayAPI.Tests.csproj
├── RaceDayAPI.sln
├── .gitattributes
├── .gitignore
├── LICENSE
└── README.md
```

## 5. Prerequisites

Install the following software before running the project:

- .NET 8 SDK.
- Visual Studio with the ASP.NET and web development workload, or Visual Studio Code.
- Microsoft SQL Server or SQL Server LocalDB.
- SQL Server Management Studio (SSMS).
- Git.
- A GitHub account.

## 6. Database Setup

1. Open SQL Server Management Studio.
2. Connect to your local SQL Server instance.
3. Open `docs/RaceDayDB.sql`.
4. Execute the script to create the RaceDay database, tables, relationships, constraints, and sample data.
5. Verify that the database and sample records were created successfully.
6. If required, execute `docs/RaceDayDBChanges.sql` after confirming that it is compatible with the main database script.

The database must match the ERD and the Entity Framework Core models used by the API.

## 7. Configuration

Open `src/RaceDayAPI/appsettings.json` and configure the database connection string for your SQL Server instance.

Example for SQL Server LocalDB:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=RaceDayDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

If you use a regular SQL Server instance, replace the server name with your actual instance name.

Do not commit database passwords, production credentials, or other secrets to GitHub.

## 8. Running the REST API

Open a terminal in the repository's root directory.

Restore the NuGet packages:

```bash
dotnet restore
```

Build the solution:

```bash
dotnet build
```

Run the API:

```bash
dotnet run --project src/RaceDayAPI/RaceDayAPI.csproj
```

Open the Swagger UI using the HTTPS address printed in the terminal, followed by `/swagger`.

For example:

```text
https://localhost:7123/swagger
```

The port may differ depending on your launch settings.

## 9. API Endpoint Reference

| Method | Route | Access | Description |
|---|---|---|---|
| POST | `/api/auth/register` | Public | Register an Organiser or Participant |
| POST | `/api/auth/login` | Public | Authenticate and start a session |
| POST | `/api/auth/logout` | Authenticated | End the current session |
| GET | `/api/profile` | Authenticated | Retrieve the current user's profile |
| PUT | `/api/profile` | Authenticated | Update profile details |
| GET | `/api/profile/enrolments` | Participant | View personal enrolments |
| GET | `/api/events` | Public | Retrieve all available events |
| GET | `/api/events/{id}` | Public | Retrieve an event by ID |
| POST | `/api/events` | Organiser | Create an event |
| PUT | `/api/events/{id}` | Organiser | Update an event |
| DELETE | `/api/events/{id}` | Organiser | Delete an event |
| GET | `/api/events/{eventId}/categories` | Public | View categories for an event |
| POST | `/api/categories` | Organiser | Create an event category |
| PUT | `/api/categories/{id}` | Organiser | Update a category |
| DELETE | `/api/categories/{id}` | Organiser | Delete a category |
| POST | `/api/enrolments` | Participant | Enrol in an event category |
| DELETE | `/api/enrolments/{id}` | Participant | Cancel an enrolment |
| GET | `/api/events/{eventId}/results` | Public | View event results |
| POST | `/api/results` | Organiser | Record a participant's result |
| PUT | `/api/results/{id}` | Organiser | Update a result |
| DELETE | `/api/results/{id}` | Organiser | Delete a result |

**Note:** The routes and access requirements above describe the planned API contract. Confirm that the implemented controllers expose these routes and enforce the specified permissions before submission.

## 10. Authentication and Role-Based Authorization

The API uses BCrypt to hash passwords before storing them in the database.

After successful login, the application stores the authenticated user's identity and role in server-side session state.

Protected endpoints verify the session before allowing access.

- **Organiser:** Can manage race events, event categories, and competition results.
- **Participant:** Can manage personal profile information and enrolments and view personal results.
- **Public user:** Can register, log in, and access public event information.

The API should return appropriate HTTP status codes for unauthorized requests, forbidden operations, invalid input, and missing records.

Session cookies should be configured appropriately for secure local and production deployments.

## 11. Running Automated Tests

The test project uses xUnit to test the application's behaviour.

Run all tests from the repository root:

```bash
dotnet test
```

The test suite should cover:

- Registration and login validation.
- Password hashing and authentication.
- Role-based access restrictions.
- Event creation and management.
- Category management.
- Participant enrolments.
- Result recording and retrieval.
- Invalid requests and missing resources.

The EF Core In-Memory Provider can isolate database tests from the development SQL Server database. It does not replace testing against SQL Server for database-specific behaviour.

## 12. Continuous Integration with GitHub Actions

The project uses GitHub Actions to automate build and test verification.

The `build-and-test.yml` workflow should:

1. Check out the repository.
2. Install the .NET 8 SDK.
3. Restore dependencies.
4. Build the solution.
5. Run automated tests.
6. Report the workflow result.

The `validate-docs.yml` workflow checks that the required Part 1 documentation and SQL script are present.

### GitHub Actions Build Status

<img width="1886" height="902" alt="image" src="https://github.com/user-attachments/assets/adcd0b46-7340-48bf-aae9-0da543fc80f1" />


## 16. Conclusion

The RaceDay Event Management System provides a foundation for managing running events through a RESTful Web API. It combines relational database persistence, authentication, role-based access control, event and category management, participant enrolments, and competition results.

Automated testing and GitHub Actions support code quality and build verification. The project builds on the database design, ERD, and API endpoint planning completed in Part 1 and provides a foundation for further development in the subsequent POE section.


