#  RaceDay Event Management System
## Part 1 – Planning | Part 2 – RESTful API | Part 3 – MVC Application

---
## 1. Project Overview

The RaceDay Event Management System is a web-based application designed to manage running events, participants, event categories, enrolments and competition results.

The project consists of three development parts:

- **Part 1 – Planning and Database:** Entity Relationship Diagram (ERD), API endpoint planning and SQL Server database design and implementation.
- **Part 2 – RESTful API:** ASP.NET Core Web API, SQL Server persistence, authentication, role-based access and event management.
- **Part 3 – MVC Application:** ASP.NET Core MVC frontend that consumes the Part 2 API through HTTP requests.

The MVC application does not connect directly to SQL Server and does not duplicate the API's business logic.

### POE Sections

- Section A: Entity Relationship Diagram.
- Section B: API Endpoint Planning.
- Section C: SQL Server Database Script and RESTful Web API Implementation.
- Part 3: MVC frontend, Azure Blob Storage integration and Docker deployment.

## 2. Table of Contents

- Technology Stack
- Solution Structure
- Features by Role
- UI Design and Accessibility
- Prerequisites
- Database Setup
- Configuration and Secrets
- Running the API and MVC Application
- Azure Blob Storage
- Docker
- API Endpoints
- Testing and Verification
- GitHub and CI/CD
- YouTube Demonstration and Submission
- Final Submission Checklist

## 3. Technology Stack

| Technology | Purpose |
|---|---|
| C# / .NET 10 | Programming language and runtime |
| ASP.NET Core Web API | Part 2 backend |
| ASP.NET Core MVC | Part 3 frontend |
| Microsoft SQL Server | Relational database |
| Entity Framework Core 10 | Database access |
| BCrypt.Net-Next | Password hashing |
| ASP.NET Core Session | Session-based authentication |
| Bootstrap 5.3 | Responsive user interface |
| HTML5 and CSS3 | Frontend design |
| JavaScript | Client-side interactions |
| Azure.Storage.Blobs | Cloud image storage |
| Swagger / OpenAPI | API documentation |
| xUnit | Automated testing |
| Docker | Application containerisation |
| Git and GitHub | Version control |
| GitHub Actions | Continuous integration |

## 4. Solution Structure

The combined repository follows this structure:

```text
RaceDay_Combined/
│
├── .github/
│   └── workflows/
│       └── build-and-test.yml
│
├── docs/
│   ├── Section_A_RaceDay_ERD.pdf
│   ├── Section_B_API_Endpoint_Plan.pdf
│   ├── RaceDayDB.sql
│   └── RaceDayDBChanges.sql
│
├── RaceDayAPI/
│   ├── Attributes/
│   ├── Controllers/
│   ├── Data/
│   ├── DTOs/
│   ├── Migrations/
│   ├── Models/
│   ├── Scripts/
│   ├── Services/
│   ├── wwwroot/
│   ├── Program.cs
│   └── RaceDayAPI.csproj
│
├── RaceDayMVC/
│   ├── Controllers/
│   ├── Filters/
│   ├── Models/
│   ├── Services/
│   ├── Views/
│   ├── wwwroot/
│   │   ├── css/
│   │   ├── js/
│   │   └── images/
│   ├── Program.cs
│   ├── Dockerfile
│   └── RaceDayMVC.csproj
│
├── tests/
│   └── RaceDayAPI.Tests/
│
├── docker-compose.yml
├── .dockerignore
├── NuGet.Config
├── RaceDay.slnx
├── .gitignore
└── README.md
```

## 5. Features by Role

### 5.1 Authentication and Security

- User registration and login.
- BCrypt password hashing.
- Session-based authentication.
- Role-based authorisation.
- Login and logout.
- Protection of restricted pages.
- Server-side validation.

### 5.2 Organiser Features

Organisers can:

- Access the organiser dashboard.
- Create, view, update and delete events.
- Create and manage event categories.
- View participant enrolments.
- Manage enrolments where authorised.
- Record competition results.
- Update and delete results.
- Upload event images through Azure Blob Storage.

### 5.3 Participant Features

Participants can:

- Register and log in.
- Access the participant dashboard.
- Browse available events.
- View event categories.
- Enrol in event categories.
- View personal enrolments.
- Cancel personal enrolments.
- Update profile information.
- View competition results.

## 6. UI Design and Accessibility

### 6.1 RaceDay Branding

The interface follows a sporting and energetic visual theme.

The design uses consistent colours, typography, spacing and reusable components across all pages.

| Interface Element | Colour |
|---|---|
| Primary | Dark Navy Blue |
| Secondary | Racing Blue |
| Accent | Bright Orange |
| Background | Light Grey / White |
| Confirmed | Green |
| Pending | Amber |
| Cancelled | Red |

### 6.2 Responsive Design

Bootstrap 5.3 provides responsive layouts for:

- Desktop computers.
- Laptops.
- Tablets.
- Mobile devices.

The interface uses Bootstrap navigation bars, cards, tables, buttons, forms and responsive grids.

### 6.3 Role-Based Navigation

**Organiser navigation**

- Dashboard
- Manage Events
- Manage Categories
- Enrolments
- Results
- Profile
- Logout

**Participant navigation**

- Dashboard
- Browse Events
- My Enrolments
- My Results
- Profile
- Logout

Public users can access the Home, Events, Register and Login pages.

### 6.4 Enrolment Status Colour Coding

| Status | Bootstrap Class | Meaning |
|---|---|---|
| Confirmed | `bg-success` | Successfully confirmed |
| Pending | `bg-warning text-dark` | Awaiting confirmation |
| Cancelled | `bg-danger` | Cancelled enrolment |

Example:

```html
<span class="badge bg-success">Confirmed</span>
<span class="badge bg-warning text-dark">Pending</span>
<span class="badge bg-danger">Cancelled</span>
```

### 6.5 Validation and Error Handling

The application must handle invalid input without crashing or exposing raw technical errors.

Validation includes:

- Required fields.
- Valid email addresses.
- Password requirements.
- Invalid event selections.
- Duplicate enrolments.
- Missing records.
- API communication failures.
- Expired sessions.

Example:

```csharp
if (!ModelState.IsValid)
{
    return View(model);
}
```

The application must display understandable validation messages to users.

### 6.6 Accessibility

The application should include:

- Meaningful labels for form controls.
- Keyboard-accessible navigation.
- Appropriate colour contrast.
- Alternative text for images.
- Clear validation messages.
- Consistent fonts and component styles.

## 7. Prerequisites

Install the following software:

- .NET 10 SDK.
- Visual Studio with ASP.NET and web development workload.
- Microsoft SQL Server or SQL Server LocalDB.
- SQL Server Management Studio.
- Git.
- Docker Desktop.
- GitHub account.
- Azure Storage account.

## 8. Database Setup

1. Open SQL Server Management Studio.
2. Connect to your SQL Server instance.
3. Open `docs/RaceDayDB.sql`.
4. Execute the SQL script.
5. Verify that the database and tables were created.
6. Confirm that the database matches the Part 1 ERD.
7. Configure the API database connection string.

Example LocalDB connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=RaceDayDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

Replace the server name according to your SQL Server installation.

## 9. Configuration and Secrets

The API and MVC applications require configuration before running.

Configure:

- SQL Server connection string in the API.
- API base URL in the MVC application.
- Azure Blob Storage connection settings.
- Appropriate session configuration.

Do not commit database passwords, Azure credentials or production secrets to GitHub.

Use .NET User Secrets during development and environment variables for deployment.

## 10. Running Part 2 API and Part 3 MVC Locally

### Step 1: Clone the repository

```bash
git clone YOUR_GITHUB_REPOSITORY_URL
cd RaceDay_Combined
```

### Step 2: Restore dependencies

```bash
dotnet restore RaceDay.slnx
```

### Step 3: Build the solution

```bash
dotnet build RaceDay.slnx
```

### Step 4: Run the RESTful API

```bash
dotnet run --project RaceDayAPI/RaceDayAPI.csproj
```

Open Swagger using the API address displayed in the terminal, followed by:

```text
/swagger
```

### Step 5: Run the MVC application

Open a second terminal:

```bash
dotnet run --project RaceDayMVC/RaceDayMVC.csproj
```

Open the MVC address displayed in the terminal.

The API must be running and accessible to the MVC application.

## 11. Azure Blob Storage

Azure Blob Storage provides cloud-based storage for event banners and profile pictures.

### Upload Process

1. The user selects an image in the MVC application.
2. MVC sends the image to the API.
3. The API validates the uploaded file.
4. The API uploads the image to Azure Blob Storage.
5. Azure stores the image.
6. The API returns the image URL or reference.
7. MVC displays the stored image.

The Azure connection string must be stored securely.

The application may use local storage as a development fallback if configured, but Azure Blob Storage must be tested for the required cloud integration.

## 12. Docker

Docker is used to package the MVC application into a container.

### Build the Docker image

Run this command from the repository root:

```bash
docker build -f RaceDayMVC/Dockerfile -t raceday-mvc .
```

### Run the container

```bash
docker run -d --name raceday-mvc -p 8080:8080 raceday-mvc
```

Open:

```text
http://localhost:8080
```

The container must be configured with the correct API URL and required environment variables.

### Useful Docker Commands

View running containers:

```bash
docker ps
```

View application logs:

```bash
docker logs raceday-mvc
```

Stop the container:

```bash
docker stop raceday-mvc
```

Remove the container:

```bash
docker rm raceday-mvc
```

**Note:** SQL Server and Azure Storage must be accessible to the container. Configure the API URL correctly for container networking rather than assuming that `localhost` refers to the host computer.

## 13. API Endpoint Reference

The following table describes the Part 2 API contract.

| Method | Route | Access |
|---|---|---|
| POST | `/api/auth/register` | Public |
| POST | `/api/auth/login` | Public |
| POST | `/api/auth/logout` | Authenticated |
| GET | `/api/profile` | Authenticated |
| PUT | `/api/profile` | Authenticated |
| GET | `/api/profile/enrolments` | Participant |
| GET | `/api/events` | Public |
| GET | `/api/events/{id}` | Public |
| POST | `/api/events` | Organiser |
| PUT | `/api/events/{id}` | Organiser |
| DELETE | `/api/events/{id}` | Organiser |
| GET | `/api/events/{eventId}/categories` | Public |
| POST | `/api/categories` | Organiser |
| PUT | `/api/categories/{id}` | Organiser |
| DELETE | `/api/categories/{id}` | Organiser |
| POST | `/api/enrolments` | Participant |
| DELETE | `/api/enrolments/{id}` | Participant |
| GET | `/api/events/{eventId}/results` | Public |
| POST | `/api/results` | Organiser |
| PUT | `/api/results/{id}` | Organiser |
| DELETE | `/api/results/{id}` | Organiser |

Image upload endpoints must also be documented according to the actual API controller implementation.

The routes and permissions must be verified against the implemented controllers before submission.

## 14. Testing and Verification

Run automated tests from the repository root:

```bash
dotnet test RaceDay.slnx
```

Testing should cover:

- Registration and login.
- Password hashing.
- Session authentication.
- Role-based access.
- Event management.
- Category management.
- Participant enrolments.
- Result management.
- Invalid requests.
- Missing records.
- API error responses.

Manually test the MVC application to verify that:

- Both user roles can log in.
- Role-specific pages are protected.
- Navigation works correctly.
- Forms validate input.
- Enrolment statuses display correctly.
- The MVC application communicates with the API.
- Azure image uploads work.
- Docker runs the application successfully.

## 15. GitHub and CI/CD

GitHub is used for source-code management and version control.

Part 3 development should be maintained in a clearly named branch:

```text
part-3-mvc
```

## 16. GitHub Actions Workflow

GitHub Actions automates the build and testing process for the API and MVC projects.

The workflow should:

1. Check out the repository.
2. Install .NET 10 SDK.
3. Restore NuGet dependencies.
4. Build the complete solution.
5. Run automated tests.
6. Validate the MVC Docker build where configured.

### Example build-and-test.yml

```yaml
name: RaceDay Build and Test

on:
  push:
    branches:
      - main
      - part-3-mvc
  pull_request:
    branches:
      - main
      - part-3-mvc

jobs:
  build:
    runs-on: ubuntu-latest

    steps:
      - name: Checkout Repository
        uses: actions/checkout@v4

      - name: Install .NET 10 SDK
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 10.0.x

      - name: Restore Dependencies
        run: dotnet restore RaceDay.slnx

      - name: Build Solution
        run: dotnet build RaceDay.slnx --configuration Release --no-restore

      - name: Run Tests
        run: dotnet test RaceDay.slnx --configuration Release --no-build
```

The workflow must be tested in GitHub Actions to confirm that the solution builds successfully.

## 17. YouTube Demonstration and Submission

An unlisted YouTube demonstration video must be submitted on ARC.

### Recommended Video Structure

**1. Introduction**

- Introduce the RaceDay Event Management System.
- Explain the purpose of the application.
- Explain the technologies used.

**2. Application Demonstration**

- Demonstrate registration and login.
- Show the Participant dashboard.
- Browse events and categories.
- Demonstrate enrolments.
- Show colour-coded statuses.
- Show the Organiser dashboard.
- Demonstrate event and category management.
- Demonstrate results management.

**3. MVC Structure**

Explain:

- Controllers.
- Models.
- Views.
- Services.
- API communication.

**4. Azure Blob Storage**

- Explain the purpose of Blob Storage.
- Demonstrate an image upload.
- Show the image stored in Azure.
- Explain how the API handles the upload.

**5. Docker**

- Explain the Dockerfile.
- Build the Docker image.
- Run the container.
- Demonstrate the application running through Docker.

**6. GitHub and CI/CD**

- Show the repository.
- Show the Part 3 branch.
- Demonstrate meaningful commits.
- Show the GitHub Actions workflow.
- Demonstrate a successful build.

### GitHub Actions Build Status

<img width="1902" height="636" alt="image" src="https://github.com/user-attachments/assets/d1100086-5340-479d-9c13-b7ac4139e083" />


### YouTube Video Link

**Unlisted Demonstration:** [INSERT YOUTUBE VIDEO LINK]


## Conclusion

The RaceDay Event Management System combines planning, database design, RESTful API development and an ASP.NET Core MVC frontend to provide a complete event management solution.

Part 1 establishes the system requirements, database structure and API design.

Part 2 implements the backend functionality, including database persistence, authentication, authorisation, event management, enrolments and competition results.

Part 3 provides a responsive user interface that allows Organisers and Participants to interact with the system. Azure Blob Storage provides cloud-based image storage, while Docker supports application containerisation.

GitHub and GitHub Actions support source-code management, automated builds and continuous integration.

Together, Parts 1, 2 and 3 demonstrate the development of a full-stack event management application using modern software development practices.

---
