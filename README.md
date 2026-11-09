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
