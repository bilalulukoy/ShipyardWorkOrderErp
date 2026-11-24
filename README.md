# Shipyard Work Order & ERP Integration API

An enterprise-grade backend integration service built with **ASP.NET Core (.NET 10)**, **Entity Framework Core**, and **PostgreSQL**, designed to bridge shipyard production operations (dry dock scheduling, block/tank work packages, and field material requisitions) with corporate ERP systems using the **Transactional Outbox & Retry Pattern**.

## Architecture & Design Patterns

The solution follows **Clean Architecture** principles to ensure separation of concerns, testability, and maintainability:

- **`ShipyardErp.Domain`**: Core shipyard entities (`ShipProject`, `WorkOrder`, `MaterialRequest`, `ErpSyncOutbox`) and domain enums (`DockLocation`, `WorkOrderStatus`, `ErpSyncStatus`).
- **`ShipyardErp.Application`**: Business logic, DTOs, service interfaces, and atomic transaction management ensuring domain writes and ERP outbox entries succeed or fail together.
- **`ShipyardErp.Infrastructure`**: EF Core `AppDbContext` (PostgreSQL & SQLite support) and `ErpSyncBackgroundWorker` (`BackgroundService`) implementing exponential backoff retry queues.
- **`ShipyardErp.API`**: RESTful controllers, global exception handling middleware, and interactive Swagger/OpenAPI documentation.

## Key Features

- **Vessel & Dock Scheduling:** Manage new-build and ship-repair projects across dry docks, floating docks, panel shops, and outfitting quays.
- **Work Order & Material Requisition Management:** Track block/zone-level work packages (`BL-204`, cargo/ballast tanks) and link field material requests directly to ERP stock codes.
- **Fault-Tolerant ERP Synchronization (Transactional Outbox):** Prevents data loss during ERP downtime or network partitions via database-backed outbox queues, exponential backoff retries, and manual retry triggers.
- **Containerized Deployment:** Ready-to-run `Dockerfile` and `docker-compose.yml` orchestrating the .NET API and PostgreSQL 16.

## Tech Stack

- **Framework:** C# / ASP.NET Core Web API (.NET 10)
- **Database & ORM:** PostgreSQL, SQLite, Entity Framework Core (Code-First)
- **Background Processing:** .NET `BackgroundService` (`IHostedService`)
- **Containerization:** Docker & Docker Compose
- **API Documentation:** Swagger / OpenAPI

## Quick Start

### Run Locally
```bash
dotnet run --project ShipyardErp.API/ShipyardErp.API.csproj