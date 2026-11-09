# RaceDay – PROG6212 Part 3 (Final PoE)

ASP.NET Core MVC front-end for the RaceDay platform. Consumes the Part 2 RESTful API, supports role-based interfaces (Organiser / Participant), Azure Blob Storage for images (via API only), and Docker containerisation of the MVC application.
Solution structure
RaceDay/
├── RaceDay.Api/              # Part 2 Web API (JWT + EF Core + Azure Blob)
├── RaceDay.Contracts/        # Shared DTOs
├── RaceDay.Mvc/              # Part 3 MVC web application
│   ├── Controllers/
│   ├── Services/RaceDayApiClient.cs
│   ├── ViewModels/
│   ├── Views/
│   ├── wwwroot/css/site.css  # RaceDay sporting theme
│   ├── Dockerfile
│   └── Program.cs
├── RaceDay.Api.Tests/
├── RaceDay.sln
└── README.md
 Prerequisites
 - .NET 8 SDK
- SQL Server / LocalDB (or adjust connection string)
- Docker Desktop (WSL 2 backend recommended on Windows)
- Azure Storage account **or** Azurite for local Blob testing
## 1. Configure & run locally

### API

1. Open RaceDay.Api/appsettings.json
2. Set your SQL connection string under ConnectionStrings:DefaultConnection
3. Set Azure Storage connection string under AzureStorage:ConnectionString
   - Local Azurite: "UseDevelopmentStorage=true"
   - Real account: "DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...;EndpointSuffix=core.windows.net"
4. Run the API (note the HTTPS port from `launchSettings.json`, commonly `https://localhost:7xxx`)

MVC
1. Open RaceDay.Mvc/appsettings.json (or Development)
2. Set ApiSettings:BaseUrl to the exact API base URL including trailing slash, e.g. "https://localhost:7123/"
3. Run RaceDay.Mvc
4. Browse to the MVC URL, register an Organiser and a Participant, and test the full flow
Important architecture rule:** The MVC project never talks to the database. All data operations go through the Part 2 API. File uploads also go through the API (/api/events/{id}/banner and /api/users/me/profile-image).
1b. How the API and MVC connect


| Piece | Address |
|---|---|
| RaceDay.Api (local, HTTP) | http://localhost:5100 (Swagger at /swagger) |
| RaceDay.Mvc (local) | http://localhost:5280 |
| RaceDay.Mvc (Docker) | http://localhost:8080 |
| MVC -> API setting | ApiSettings:BaseUrl = http://localhost:5100/ (local) or http://host.docker.internal:5100/ (Docker, set with '-e`) |



Login flow: MVC posts to `api/auth/login`, the API returns a JWT, MVC stores it (plus UserId and Role) in the MVC session and sends it as a `Bearer` token on every later API call. If the API is not running, MVC shows "Cannot reach the RaceDay API" instead of crashing.

**Run both at once:** double-click start-local.bat, or in Visual Studio choose the **RaceDay API + MVC** launch profile (from RaceDay.slnLaunch) / set both projects as Startup Projects.

2. Database setup (SQL Server)





