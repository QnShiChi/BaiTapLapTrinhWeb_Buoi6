# Buoi 6 Ecommerce Rebuild Design

## Goal

Rebuild the current ASP.NET Core MVC demo project into a working ecommerce training application for Buoi 6, using EF Core, SQL Server, ASP.NET Core Identity, repository pattern, async database access, an `Admin` area, and UI styling aligned with `DESIGN.md`.

## Current Project Assessment

- The current project is a `Book` demo from a previous exercise.
- Existing business code is centered around `Book`, `Category`, and direct `DbContext` usage in controllers.
- The project already runs on ASP.NET Core MVC with EF Core SQL Server packages installed.
- `docker-compose.yml` already contains a SQL Server service shell but currently depends on an external `SA_PASSWORD` environment variable.
- `DESIGN.md` defines a clear Airbnb-inspired visual system:
  - white canvas
  - near-black text
  - Rausch accent `#ff385c`
  - soft rounded corners
  - photo-first product cards
  - clean admin-friendly forms and buttons

## Scope

This rebuild includes:

- removing old `Book` demo business code, views, view models, and migrations
- keeping the existing MVC project structure and `.csproj`
- introducing ecommerce domain models:
  - `Category`
  - `Product`
  - `ProductImage`
  - `ApplicationUser`
- converting `ApplicationDbContext` to `IdentityDbContext<ApplicationUser>`
- adding repository abstractions and implementations
- adding public product browsing
- adding `Area/Admin` for restricted management
- adding Identity registration/login with custom user fields
- seeding roles and sample users
- configuring SQL Server in Docker for database `QLBanHang`
- creating a fresh migration baseline for ecommerce + Identity

This rebuild does not include:

- shopping cart
- checkout
- order management
- payment integration
- advanced media processing

## Architecture

### Project Strategy

The implementation will rebuild the app inside the existing project instead of creating a new project. This keeps launch settings, static asset conventions, and the current MVC host intact while allowing a full cleanup of previous demo code.

### Application Layers

- `Models/`
  - entity classes and `ApplicationDbContext`
- `Repositories/`
  - repository interfaces and EF Core implementations
- `Controllers/`
  - public browsing controllers
- `Areas/Admin/Controllers/`
  - restricted CRUD management for products and categories
- `Views/`
  - public UI
- `Areas/Admin/Views/`
  - admin UI

### Data Access Pattern

- Controllers will depend on repositories, not directly on `ApplicationDbContext`.
- Repository methods will use async EF Core APIs.
- Query composition that needs includes or delete orchestration will live in repositories.
- File upload handling will stay in controllers or a small helper boundary, while persistence remains in repositories.

## Domain Model Design

### Category

Responsibilities:

- store product grouping metadata
- support admin CRUD
- support public product filtering

Planned fields:

- `Id`
- `Name`
- `Description`
- navigation collection `Products`

Validation:

- required `Name`
- length limits on `Name` and `Description`

### Product

Responsibilities:

- represent a sellable catalog item
- belong to one category
- support multiple product images

Planned fields:

- `Id`
- `Name`
- `Price`
- `Description`
- `CategoryId`
- navigation `Category`
- navigation collection `ProductImages`

Validation:

- required `Name`
- positive `Price`
- required `Description`
- required valid `CategoryId`

### ProductImage

Responsibilities:

- store one uploaded image path for a product
- support multiple images per product

Planned fields:

- `Id`
- `ProductId`
- `ImageUrl`
- `IsPrimary`
- navigation `Product`

Rules:

- every created product should have at least one image
- one image can be marked primary for card and detail display

### ApplicationUser

Responsibilities:

- extend Identity user storage for course requirements

Planned fields:

- inherited `IdentityUser`
- `FullName`
- `Address`

Rules:

- register form must collect `UserName`, `FullName`, `Address`, password
- new users are assigned the `Member` role by default

## Database Design

### DbContext

`ApplicationDbContext` will inherit from `IdentityDbContext<ApplicationUser>`.

DbSets:

- `Products`
- `Categories`
- `ProductImages`

Model configuration:

- `Category` one-to-many `Product`
- `Product` one-to-many `ProductImage`
- decimal precision for `Product.Price`
- delete behavior configured to support safe admin deletion flows

### Category Delete Strategy

This is an explicit business requirement and will not rely on accidental cascade settings.

Delete flow when admin deletes a category:

1. load the category with all products and product images
2. delete `ProductImage` rows for all child products
3. delete child `Product` rows
4. delete the `Category`
5. save in one transaction

This avoids foreign key issues and matches the required confirmation behavior.

### SQL Server

Target database:

- `QLBanHang`

Connection approach:

- SQL Server container in Docker
- SQL authentication with:
  - user: `sa`
  - password: `123456`

Expected connection string target:

- local SQL Server port `1433`
- database `QLBanHang`
- `TrustServerCertificate=True`
- `MultipleActiveResultSets=True`

## Identity and Authorization Design

### Identity Setup

Identity will be fully integrated into the same database as the business tables.

Configuration includes:

- `AddDefaultIdentity<ApplicationUser>()`
- roles enabled with `AddRoles<IdentityRole>()`
- EF stores using `ApplicationDbContext`

### Roles

Required seeded roles:

- `Admin`
- `Member`

### Seeded Accounts

At least:

- one admin account
- one member account

Seed process:

- runs at startup
- ensures database exists or migrations are applied first
- ensures roles exist
- ensures users exist
- ensures role assignments are correct

### Authorization Rules

Public users and non-admin users:

- can view product list
- can filter by category
- can view product details
- cannot access admin routes

Admin users:

- can access `/Admin/...`
- can CRUD products
- can CRUD categories

Protection mechanisms:

- `[Authorize(Roles = "Admin")]` on admin controllers
- admin navigation rendered conditionally based on signed-in role
- unauthorized access goes through Identity access denied behavior

## Routing Design

### Public Routes

- `/Products`
- `/Products/Details/{id}`
- `/Products/Category/{categoryId}`

### Admin Routes

- `/Admin/Product`
- `/Admin/Product/Details/{id}`
- `/Admin/Product/Create`
- `/Admin/Product/Edit/{id}`
- `/Admin/Product/Delete/{id}`
- `/Admin/Category`
- `/Admin/Category/Create`
- `/Admin/Category/Edit/{id}`
- `/Admin/Category/Delete/{id}`

### Home Route

The home page will redirect to or present an entry point for the public product listing rather than the old book demo landing page.

## Repository Design

### IProductRepository

Planned responsibilities:

- get all products with category and images
- get products by category
- get product by id with full detail
- add product
- update product
- delete product
- support image persistence metadata updates

Representative async method set:

- `Task<IEnumerable<Product>> GetAllAsync()`
- `Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId)`
- `Task<Product?> GetByIdAsync(int id)`
- `Task AddAsync(Product product)`
- `Task UpdateAsync(Product product)`
- `Task DeleteAsync(int id)`

### ICategoryRepository

Planned responsibilities:

- get categories
- get category by id
- add category
- update category
- delete category with child product cleanup

Representative async method set:

- `Task<IEnumerable<Category>> GetAllAsync()`
- `Task<Category?> GetByIdAsync(int id)`
- `Task AddAsync(Category category)`
- `Task UpdateAsync(Category category)`
- `Task DeleteAsync(int id)`

## Controller Design

### Public Controllers

#### HomeController

- minimal landing behavior
- likely redirect to `Products/Index`

#### ProductsController

Public-facing responsibilities:

- list all products
- list products by category
- show details

No create, edit, or delete actions will exist in the public controller.

### Admin Controllers

#### Area/Admin/ProductController

Responsibilities:

- list all products
- create product with multiple image upload
- edit product and manage images
- view details
- delete product

#### Area/Admin/CategoryController

Responsibilities:

- list categories
- create category
- edit category
- delete category with explicit warning that child products will also be deleted

## View and UI Design

### Design System Translation

The implementation will replace the current neon/gradient visual style with a UI based on `DESIGN.md`.

Core UI decisions:

- background: white canvas
- text: `#222222`
- primary accent: `#ff385c`
- border tones: light gray hairlines
- rounded corners: soft and consistent
- restrained shadow usage
- typography: clean sans-serif fallback stack approximating the design guidance

### Public UI

#### Layout

- top navigation with brand, category/product browsing access, auth links, and conditional admin link
- wide centered content container
- footer with simple informational links and legal text

#### Product List

- card-based grid
- primary product image displayed prominently
- product name, category, short summary, price
- filter entry points for categories

#### Product Detail

- main primary image plus supporting gallery if present
- product name, price, description, category
- clean spacing and readable typography

### Admin UI

- separate admin layout or clearly distinct admin navigation within area views
- category and product management screens optimized for CRUD clarity
- mix of tables and cards where appropriate
- validation messages visible near fields
- destructive actions clearly marked

### Forms

- use server-side validation attributes
- show `asp-validation-for` messages
- category/product forms must be clear and compact
- product forms must support multiple image uploads

## File Cleanup Plan

Items to remove or replace:

- old `BookController`
- old `Book` model
- old `Book` views
- old `Book` view models
- old migration files for the book demo
- outdated navbar links that expose create actions publicly
- CSS that conflicts with `DESIGN.md`

Items to keep and adapt:

- existing ASP.NET Core MVC host project
- static library assets already installed
- `HomeController` file path, though behavior may change
- `Views/Shared` structure
- `docker-compose.yml`, rewritten as needed

## Infrastructure Design

### Docker Compose

`docker-compose.yml` will be updated to make the SQL Server setup self-contained for the exercise.

Expected behavior:

- SQL Server 2022 container
- fixed container name
- exposed port `1433`
- environment configured directly for `sa` / `123456`
- named volume for persistence

### App Configuration

`appsettings.json` will be updated with a working SQL Server connection string for local Docker use.

If needed for stronger local reliability, the connection target will use `localhost,1433`.

## Migration Strategy

- remove existing book migrations
- generate a fresh migration for Identity + ecommerce schema
- apply migration to `QLBanHang`

Expected resulting tables include:

- `Products`
- `Categories`
- `ProductImages`
- Identity tables including:
  - `AspNetUsers`
  - `AspNetRoles`
  - `AspNetUserRoles`
  - default Identity support tables

## Error Handling and Validation

- missing entities return `NotFound()`
- invalid model state returns the same view with validation messages
- category deletion confirmation page explicitly warns about child product deletion
- unauthorized admin access flows to access denied or login redirect
- repository delete operations guard against null entities

## Verification Plan

The implementation will be considered complete only if these checks pass:

- project builds successfully
- migration is created successfully
- database updates successfully against Docker SQL Server
- roles and seeded users exist
- public product list, detail, and category filter work
- admin login works
- non-admin users cannot access admin routes
- admin can CRUD products
- admin can CRUD categories
- deleting a category removes its products safely

## Open Operational Assumptions

These assumptions are now fixed for implementation unless you change them:

- SQL Server runs in Docker
- SQL auth uses `sa` / `123456`
- database name is `QLBanHang`
- old book demo code and migrations are intentionally removed
- products support multiple images via `ProductImage`
- the rebuild stays inside the existing MVC project instead of creating a new one

## Spec Review Notes

Self-review completed against the approved scope:

- no placeholder sections remain
- no contradictory architecture decisions found
- scope is intentionally limited to catalog management and Identity
- the category delete requirement is explicitly covered
- the UI direction is mapped to `DESIGN.md` rather than left generic

## Limitation

There is no Git repository initialized in the current project directory, so this spec cannot be committed at this time. The file will still be written locally and used as the source of truth for the implementation plan.
