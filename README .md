# VendorHub

A multi-vendor e-commerce **Web API** built with **ASP.NET Core (.NET 10)**, following a **5-layer architecture** (Domain, Contracts, AppService, Infrastructure, API). Vendors apply and get approved by an admin, list products, and customers browse and place orders across multiple vendors in a single checkout. No Razor Views — this is a JSON API, tested via Swagger.

No CQRS, no external NuGet packages beyond what's explicitly required: **Entity Framework Core** (Code-First), **JWT Bearer Authentication**, and **Swashbuckle** (Swagger UI).

---

## Architecture

```
VendorHub.Domain          → Entities, enums, core domain rules (no dependencies)
VendorHub.Contracts       → DTOs only (depends on Domain, for shared enums)
VendorHub.AppService      → Services, business logic (depends on Domain, Contracts)
VendorHub.Infrastructure  → EF Core DbContext, Repositories, Security (depends on Domain, AppService)
VendorHub.API             → Controllers, DI wiring (depends on AppService, Contracts, Infrastructure)
```

**Dependency rule:** `API → AppService → Infrastructure → Domain`, with `Contracts` sitting alongside as a shared DTO layer referenced by both `API` and `AppService`.

All projects live under `src/`.

### Core Modules

| Module | Description | Included? |
|---|---|---|
| Auth | Register/login (JWT) for Admin, Vendor, and Customer roles | ✅ Yes |
| Vendor Management | Vendor application, admin approval workflow | ✅ Yes |
| Catalog | Categories and products, owned per-vendor | ✅ Yes |
| Orders | Checkout, order history, per-vendor order splitting | ✅ Yes |
| Payments | Simulated payment covering one or more orders | ✅ Yes |
| Cart | Add/update/remove items before checkout | ⬜ Not included yet — checkout currently takes items directly in the request |
| Reviews | Customers review products after purchase | ⬜ Not included yet |

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
Open `VendorHub.slnx` in Visual Studio, or work from the terminal with the `dotnet` CLI. Project folders live under `src/`.

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
This pulls in `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Tools`, `Microsoft.EntityFrameworkCore.Design` (in `VendorHub.API`, required for migrations), `Microsoft.AspNetCore.Authentication.JwtBearer`, and `Swashbuckle.AspNetCore` — the only external packages used, added because JWT auth and Swagger were explicitly required.

### 4. Configure the Connection String and JWT Key
Open `src/VendorHub.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=VendorHubDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Jwt": {
    "Key": "CHANGE_THIS_TO_A_LONG_RANDOM_SECRET_KEY_AT_LEAST_32_CHARS",
    "Issuer": "VendorHub",
    "Audience": "VendorHubUsers",
    "ExpiryMinutes": "60"
  }
}
```

- Using **LocalDB** → the value above works as-is.
- Using **SQL Server Express/full** → replace `Server=` with your instance name, e.g. `Server=.\SQLEXPRESS`, and add `User Id=` / `Password=` if not using Windows auth.

### 5. Create the Database (Code-First Migrations)

Run these from the solution root, with the `src/` prefix on both project paths:

```bash
dotnet ef migrations add InitialCreate --project src/VendorHub.Infrastructure --startup-project src/VendorHub.API
```

This generates a `Migrations` folder inside `VendorHub.Infrastructure` describing the schema based on your entity classes.

Apply the migration to create the actual database:

```bash
dotnet ef database update --project src/VendorHub.Infrastructure --startup-project src/VendorHub.API
```

This creates `VendorHubDb` in your SQL Server instance with tables for the modules included above (Users, Roles, Vendors, Categories, Products, Orders, OrderItems, Payments), seeded with the three roles and the default **Others** category.

> **Note:** Any time you change an entity, repeat both commands with a new migration name, e.g. `AddProductDiscountField`, to keep the schema in sync.

### 6. Run the Application

```bash
dotnet run --project src/VendorHub.API
```

Or press **F5** in Visual Studio with `VendorHub.API` set as the startup project.

### 7. Open Swagger

Once running, open:
```
https://localhost:5001/swagger/index.html
```

Test endpoints directly from here. For authenticated ones, log in via `POST /api/auth/login`, copy `data.token` from the response, click **Authorize** (top right) and paste the token on its own — **without** the `Bearer ` prefix, which Swagger adds for you. Every endpoint then sends the header until you log out.

### 8. First-Time Use

1. `POST /api/auth/register` with `roleId: 1` → creates an Admin account.
2. `POST /api/auth/register` with `roleId: 3` → creates a Customer account.
3. As Customer, `POST /api/vendor/apply` → submits a vendor application (Pending).
4. As Admin, approve it via `PUT /api/vendor/{id}/review` with body `{ "status": "Approved" }` — send `"Rejected"` to turn it down. Only a Pending application can be reviewed.
5. The approved Vendor adds products. `categoryId` is optional: omit it and the product is filed under the seeded **Others** category (id `1000`). Admins can create richer categories at any time.
6. A Customer browses products, checks out, and pays — orders split automatically per vendor.

---

## Resetting the Database

```bash
dotnet ef database drop --project src/VendorHub.Infrastructure --startup-project src/VendorHub.API
dotnet ef database update --project src/VendorHub.Infrastructure --startup-project src/VendorHub.API
```

---

## Tech Stack

- ASP.NET Core **Web API** (.NET 10) — JSON endpoints, no Razor Views
- Entity Framework Core (Code-First, SQL Server)
- JWT Bearer Authentication with role-based `[Authorize(Roles = "...")]`
- Swagger / Swashbuckle for interactive API docs and testing
- Built-in `ILogger<T>` for logging, built-in `System.Security.Cryptography` for password hashing (no external libraries for either)

---

## Project Status

This is a learning/portfolio project built to practice layered architecture in ASP.NET Core. Features are implemented incrementally in this order: **Auth → Vendor Approval → Product Catalog → Orders/Checkout → Payments**. Cart and Reviews are not built yet.