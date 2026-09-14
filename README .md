# VendorHub

A multi-vendor e-commerce web application built with **ASP.NET Core MVC (.NET 10)**, following a **5-layer architecture** (Domain, Contracts, Application, Infrastructure, Api). Vendors can register and list products, customers can browse, add to cart, and place orders, and admins can approve vendors and manage categories.

No CQRS, no external NuGet packages beyond **Entity Framework Core** (used for data access, Code-First approach).

---

## Architecture

```
VendorHub.Domain          → Entities, enums, core domain rules (no dependencies)
VendorHub.Contracts       → DTOs only (depends on Domain, for shared enums)
VendorHub.Application     → Services, business logic, mapping (depends on Domain, Contracts)
VendorHub.Infrastructure  → EF Core DbContext, Repositories, Migrations (depends on Domain)
VendorHub.Api             → Controllers, Views, DI wiring (depends on Application, Contracts, Infrastructure)
```

**Dependency rule:** `Api → Application → Infrastructure → Domain`, with `Contracts` sitting alongside as a shared DTO layer referenced by both `Api` and `Application`.

### Core Modules

| Module | Description |
|---|---|
| Auth | Register/login for Admin, Vendor, and Customer roles |
| Vendor Management | Vendor registration, admin approval workflow |
| Catalog | Categories and products, owned per-vendor |
| Cart | Add/update/remove items before checkout |
| Orders | Checkout, order history, per-vendor order splitting |
| Payments | Basic payment record per order |
| Reviews | Customers review products after purchase |

---

## Prerequisites

Before running this project, install:

- **.NET 10 SDK** — [https://dotnet.microsoft.com/download](https://dotnet.microsoft.com/download)
- **SQL Server** (LocalDB, Express, or full SQL Server) — LocalDB ships with Visual Studio
- **Visual Studio 2022** or **VS Code** (with C# extension)
- **EF Core CLI tools** (installed globally, see Step 2 below)

Verify .NET is installed:
```bash
dotnet --version
```

---

## Project Setup (Code-First Approach)

### 1. Clone / Open the Solution
```bash
cd VendorHub
```
Open `VendorHub.sln` in Visual Studio, or work from the terminal with the `dotnet` CLI.

### 2. Install EF Core CLI Tools (one-time, machine-wide)
```bash
dotnet tool install --global dotnet-ef
```
Verify it installed correctly:
```bash
dotnet ef --version
```

### 3. Restore NuGet Packages
```bash
dotnet restore
```
This pulls in `Microsoft.EntityFrameworkCore.SqlServer` and `Microsoft.EntityFrameworkCore.Tools` (the only two external packages used in this project, both inside `VendorHub.Infrastructure`).

### 4. Configure the Connection String
Open `VendorHub.Api/appsettings.json` and set your SQL Server connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=VendorHubDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

- Using **LocalDB** → the value above works as-is.
- Using **SQL Server Express/full** → replace `Server=` with your instance name, e.g. `Server=.\SQLEXPRESS`, and add `User Id=` / `Password=` if not using Windows auth.

### 5. Create the Database (Code-First Migrations)

Run these from the solution root, targeting the `Infrastructure` project (where `AppDbContext` lives) and the `Api` project (as startup project):

```bash
dotnet ef migrations add InitialCreate --project VendorHub.Infrastructure --startup-project VendorHub.Api
```

This generates a `Migrations` folder inside `VendorHub.Infrastructure` describing the schema based on your entity classes.

Apply the migration to create the actual database:

```bash
dotnet ef database update --project VendorHub.Infrastructure --startup-project VendorHub.Api
```

This creates `VendorHubDb` in your SQL Server instance with all tables (Users, Vendors, Categories, Products, Carts, Orders, Payments, Reviews).

> **Note:** Any time you change an entity in `VendorHub.Domain` (add a field, new entity, etc.), repeat both commands with a new migration name, e.g. `AddProductDiscountField`, to keep the database schema in sync.

### 6. Run the Application

From the solution root:
```bash
dotnet run --project VendorHub.Api
```

Or press **F5** in Visual Studio with `VendorHub.Api` set as the startup project.

The app will start on something like:
```
https://localhost:5001
http://localhost:5000
```

### 7. First-Time Use

1. Navigate to `/Account/Register` and create an **Admin** account first (or seed one — see below).
2. Register a **Vendor** account — it will sit in `Pending` status.
3. Log in as Admin and approve the vendor from the Admin dashboard.
4. Log in as the approved Vendor and add products under a category.
5. Register a **Customer** account, browse products, add to cart, and check out.

---

## Optional: Seeding Initial Data

To avoid manually creating an Admin account every time the database is recreated, add a seed method in `AppDbContext.OnModelCreating` (Fluent API `HasData`) or call a seeding method at startup in `Program.cs`. Suggested seed data:

- One `Admin` user
- A couple of `Category` records (e.g., Electronics, Clothing)

This step is optional and can be added once the base schema is working.

---

## Resetting the Database

If you need to start fresh during development:

```bash
dotnet ef database drop --project VendorHub.Infrastructure --startup-project VendorHub.Api
dotnet ef database update --project VendorHub.Infrastructure --startup-project VendorHub.Api
```

---

## Tech Stack

- ASP.NET Core MVC (.NET 10)
- Entity Framework Core (Code-First, SQL Server)
- Razor Views + Bootstrap (default MVC template, no extra CSS/JS packages)
- Cookie-based Authentication with Role-based Authorization
- Built-in `ILogger<T>` for logging (no external logging library)

---

## Project Status

This is a learning/portfolio project built to practice layered architecture in ASP.NET Core. Features are implemented incrementally in this order: Auth → Vendor Approval → Product Catalog → Cart → Checkout/Orders → Reviews.
