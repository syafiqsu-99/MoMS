# MoMS — Step 1 scaffold (ASP.NET Core 8 + Vue 3)

This is the foundation of the MoMS port from Node/Express to ASP.NET Core 8:
solution skeleton, EF migration
tooling, DI + SpaProxy wiring, and one complete vertical slice (`full_list` +
`location`) proving the backend and frontend patterns end to end.

## Prerequisites

- .NET 8 SDK
- Node.js 18+
- SQL Server reachable from your machine

## 1. Set the connection string (env var, never appsettings.json)

The app and `dotnet ef` both read `ConnectionStrings__MoMsConnection` (falling
back to `ConnectionStrings__DefaultConnection`). Set it as a machine-level
system environment variable.

Windows (PowerShell, machine-level — run as admin):

```powershell
[Environment]::SetEnvironmentVariable(
  "ConnectionStrings__MoMsConnection",
  "Server=YOUR_SERVER\SQLEXPRESS;Database=MoMS;User Id=YOUR_USER;Password=YOUR_PASSWORD;Encrypt=False;TrustServerCertificate=True",
  "Machine")
```

Restart the terminal/VS Code after setting it.

## 2. Restore the EF CLI tool (pinned to the runtime version)

```bash
cd MoMS
dotnet tool restore
```

## 3. Create and apply the first migration

```bash
dotnet ef migrations add InitialCreate --project MoMS.Server
dotnet ef database update --project MoMS.Server
```

`SchemaInitializerService` also applies pending migrations at startup, so once
the migration exists you can skip `database update` and just run the app. The
initializer is additive only — it never drops columns or tables.

## 4. Run the backend (VS Code)

Press F5 (".NET Launch (MoMS.Server)") or:

```bash
dotnet watch run --project MoMS.Server
```

Backend listens on http://localhost:5000.

## 5. Run the frontend

```bash
cd MoMS.Client
npm install
npm run dev
```

Client on http://localhost:4000, proxying `/api` and `/static` to the backend.
Open http://localhost:4000 — the Full List table loads from `/api/full-list`.

## Keeping versions aligned

The EF Core runtime packages and the pinned `dotnet-ef` tool are both `8.0.8`.
If you upgrade one, upgrade the other, or migration commands will fail.

## Security note

Your original `.env` committed live SQL credentials (`sa` + password + server).
Rotate that password and remove `.env` from history. This scaffold's
`.gitignore` excludes `.env` and secrets going forward.
