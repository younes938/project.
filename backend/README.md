# ETicket backend — full-stack development

The backend targets .NET 10. Passenger CRUD and vehicle visits are configured to use SQL Server by default. Operating context, audit, and checkpoint catalog still use in-memory repositories in this prototype. The development profile enables a mock sign-in only for local development.

## Database

Execute `database/001_create_passengers.sql`, `database/002_create_operations.sql`, and `database/003_create_weight_history.sql` in order in SQL Server Management Studio. The default connection string targets `.;\SQLEXPRESS` and database `ETicketPassengers`; update `ConnectionStrings:Passengers` in `src/ETicket.Api/appsettings.json` if your SQL Server instance differs.

## Run

```powershell
dotnet restore ETicket.slnx
dotnet run --project src/ETicket.Api/ETicket.Api.csproj --launch-profile http
```

The API listens at `http://localhost:5009`; health endpoint: `/health`.

## Validation

```powershell
dotnet build ETicket.slnx
dotnet test ETicket.slnx
```
