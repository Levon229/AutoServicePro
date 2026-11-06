# AutoServicePro

AutoServicePro is a small ASP.NET Core Web API for an auto repair shop. It manages vehicles, spare parts, and work orders.

## Technology

- .NET 10
- ASP.NET Core Web API
- Entity Framework Core with SQLite
- xUnit, Moq, and FluentAssertions

## API endpoints

| Method | Route | Purpose |
| --- | --- | --- |
| POST | /api/Vehicles | Create a vehicle |
| GET | /api/Vehicles | List vehicles |
| GET | /api/Vehicles/{id} | Get a vehicle |
| POST | /api/SpareParts | Create a spare part |
| GET | /api/SpareParts | List spare parts |
| GET | /api/SpareParts/{id} | Get a spare part |
| POST | /api/WorkOrders | Create a work order and deduct its parts from stock |
| GET | /api/WorkOrders | List work orders with vehicle details |
| GET | /api/WorkOrders/{id} | Get a work order |
| PATCH | /api/WorkOrders/{id}/complete | Complete a work order |

## Run locally

    dotnet restore
    dotnet run --project AutoServicePro.API/AutoServicePro.API.csproj

In Development, Swagger UI is available at /swagger. The API uses AutoServicePro.API/appsettings.json for the SQLite connection string.

## Tests

    dotnet test AutoServicePro.slnx

## Current limitations

The application currently uses EnsureCreated for a local SQLite database and does not include database migrations, authentication, or role-based authorization. Before deploying it or storing real customer information, add a migration strategy and authentication/authorization. The API is intended for local development and learning at this stage.