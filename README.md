\# Auto Service \& Spare Parts Management API



Stack: C#, .NET 8/9, ASP.NET Core Web API, Entity Framework Core, SQLite, xUnit



Project Structure:

\- AutoServicePro.Domain/ — Core domain entities, enums, and base classes

\- AutoServicePro.Application/ — Business logic, DTOs, services, and repository interfaces

\- AutoServicePro.Infrastructure/ — Data access layer, Entity Framework Core DbContext, and repository implementations

\- AutoServicePro.API/ — Presentation layer, controllers, global middleware, and application entry point

\- AutoServicePro.UnitTests/ — Unit testing suite for core business logic



Important Files:

\- AutoServicePro.sln — Solution file uniting all projects

\- AutoServicePro.API/Program.cs — Application entry point, DI container configuration, and middleware setup

\- AutoServicePro.Application/Services/WorkOrderService.service.cs — Core business logic and inventory validation

\- AutoServicePro.Infrastructure/Data/AppDbContext.cs — EF Core database context and Fluent API configuration



\## Application Overview

Auto Service \& Spare Parts Management API is a professional backend system simulating a real-world auto repair shop ERP module. The application allows management to register vehicles, track spare parts inventory, validate stock levels, and process work orders with automated cost calculations and status tracking.



\## Architecture and Design Patterns



The project follows \*\*Clean Architecture\*\* principles to ensure strict separation of concerns across layers:



\- \*\*Domain:\*\* Contains enterprise logic, business entities, and enums, remaining completely independent of external frameworks and databases.

\- \*\*Application:\*\* Contains business rules, application services, immutable data transfer objects (`record`), and generic repository interfaces implementing the \*\*Dependency Inversion Principle\*\*.

\- \*\*Infrastructure:\*\* Implements data persistence using Entity Framework Core and SQLite, providing concrete repository implementations.

\- \*\*API:\*\* Exposes HTTP endpoints via thin controllers and handles cross-cutting concerns.



\### Key Architectural Patterns

\- \*\*Generic Repository Pattern:\*\* Encapsulates data access logic and promotes code reusability across entities.

\- \*\*Dependency Injection (DI):\*\* Native .NET DI container used for decoupling services and repositories.

\- \*\*Observer / Middleware Pattern:\*\* Centralized error handling via custom middleware.



\---



\## Data Models and Entities



\### Domain Entities

\- \*\*`BaseEntity`\*\*: Abstract base class containing the primary key identifier (`Id`).

\- \*\*`Vehicle`\*\*: Represents a client vehicle with properties: `Make`, `Model`, `Year`, `LicensePlate`, `VIN`, `OwnerName`, and `OwnerPhone`.

\- \*\*`SparePart`\*\*: Represents warehouse inventory items with properties: `Name`, `PartNumber`, `UnitPrice`, and `StockQuantity`.

\- \*\*`WorkOrder`\*\*: Represents a repair service order linking a vehicle, issue description, labor cost, total cost, status (`Pending`, `In Progress`, `Completed`), and timestamps.

\- \*\*`WorkOrderPart`\*\*: Junction entity for the many-to-many relationship between work orders and spare parts, tracking requested quantities and historical unit prices (`UnitPriceAtTime`).



\---



\## Business Logic and Services



\### WorkOrderService

The core service implementing business workflows:

1\. \*\*Vehicle Validation:\*\* Verifies if the target vehicle exists in the database.

2\. \*\*Stock Validation \& Deduction:\*\* Iterates through requested spare parts, checks warehouse stock availability, throws an exception if stock is insufficient, and automatically deducts the used quantities.

3\. \*\*Dynamic Cost Calculation:\*\* Computes total work order costs by combining labor costs and historical part prices.



\---



\## Error Handling



The application features a \*\*Global Exception Handling Middleware\*\* (`ExceptionHandlingMiddleware`) that intercepts unhandled exceptions across the request pipeline, transforming domain and validation errors into standardized, clean JSON responses with appropriate HTTP status codes (404, 400, 500).



\---



\## Installation and Setup



To install and run the project locally, execute the following commands:



```bash

dotnet restore

dotnet run --project AutoServicePro.API/AutoServicePro.API.csproj

