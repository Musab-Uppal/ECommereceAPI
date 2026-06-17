# Database Schema — ECommereceAPI

This document describes the canonical database schema, relationships, constraints, indexing and operational guidance for the ECommereceAPI backend. It is written for engineers and DBAs who will maintain or evolve the schema.

Contents

- Overview & goals
- Entity Relationship (textual)
- Table definitions (DDL examples for SQL Server)
- Index recommendations
- Common queries & examples
- Migration, seeding and operational guidance
- Performance & scaling notes

---

## Overview & goals

Schema goals:

- Normalized model for consistency and clarity (3NF for core entities)
- Auditability (createdAt, updatedAt, createdBy optional)
- Referential integrity via foreign keys
- Reasonable default indexes to support common OLTP queries
- Soft delete support where appropriate

Primary entities

- Users
- Products
- Categories
- Orders
- OrderItems
- Reviews
- (optional) Cart/CartItems or a transient cart implementation

---

## Entity Relationship (textual)

- A `User` can place many `Order`s (1:N)
- An `Order` contains many `OrderItem`s (1:N). Each `OrderItem` references a single `Product`.
- A `Product` belongs to a single `Category` (or N-to-M via `ProductCategory` if needed).
- A `User` can create many `Review`s; a `Product` can have many `Review`s (1:N)

ER sketch:

User 1---_ Order _---1 OrderItem _---1 Product _---1 Category
User 1---_ Review _---1 Product

---

## Table definitions (SQL Server DDL examples)

Note: tune types to your RDBMS (e.g. `timestamp`/`timestamptz` for Postgres). We use SQL Server types below.

-- Users

```sql
CREATE TABLE [dbo].[Users] (
  [Id] INT IDENTITY(1,1) PRIMARY KEY,
  [Email] NVARCHAR(256) NOT NULL UNIQUE,
  [PasswordHash] NVARCHAR(512) NOT NULL,
  [FirstName] NVARCHAR(120) NULL,
  [LastName] NVARCHAR(120) NULL,
  [Phone] NVARCHAR(32) NULL,
  [Address] NVARCHAR(1024) NULL,
  [Role] NVARCHAR(32) NOT NULL DEFAULT('customer'),
  [IsDeleted] BIT NOT NULL DEFAULT(0),
  [RowVersion] ROWVERSION NOT NULL,
  [CreatedAt] DATETIMEOFFSET(7) NOT NULL DEFAULT SYSUTCDATETIME(),
  [UpdatedAt] DATETIMEOFFSET(7) NULL
);
```

-- Categories

```sql
CREATE TABLE [dbo].[Categories] (
  [Id] INT IDENTITY(1,1) PRIMARY KEY,
  [Name] NVARCHAR(200) NOT NULL UNIQUE,
  [Description] NVARCHAR(1024) NULL,
  [CreatedAt] DATETIMEOFFSET(7) NOT NULL DEFAULT SYSUTCDATETIME(),
  [UpdatedAt] DATETIMEOFFSET(7) NULL
);
```

-- Products

```sql
CREATE TABLE [dbo].[Products] (
  [Id] INT IDENTITY(1,1) PRIMARY KEY,
  [Name] NVARCHAR(300) NOT NULL,
  [Description] NVARCHAR(MAX) NULL,
  [Price] DECIMAL(12,2) NOT NULL,
  [Stock] INT NOT NULL DEFAULT 0,
  [CategoryId] INT NOT NULL REFERENCES [dbo].[Categories](Id) ON DELETE RESTRICT,
  [Rating] DECIMAL(3,2) NULL,
  [ReviewsCount] INT NOT NULL DEFAULT 0,
  [InStock] BIT NOT NULL DEFAULT 1,
  [CreatedAt] DATETIMEOFFSET(7) NOT NULL DEFAULT SYSUTCDATETIME(),
  [UpdatedAt] DATETIMEOFFSET(7) NULL
);
```

-- Orders

```sql
CREATE TABLE [dbo].[Orders] (
  [Id] INT IDENTITY(1,1) PRIMARY KEY,
  [UserId] INT NOT NULL REFERENCES [dbo].[Users](Id) ON DELETE CASCADE,
  [Subtotal] DECIMAL(12,2) NOT NULL,
  [Tax] DECIMAL(12,2) NOT NULL,
  [Shipping] DECIMAL(12,2) NOT NULL,
  [Total] DECIMAL(12,2) NOT NULL,
  [Status] NVARCHAR(32) NOT NULL DEFAULT('pending'),
  [AddressSnapshot] NVARCHAR(1024) NULL, -- store shipping address at time of order
  [CreatedAt] DATETIMEOFFSET(7) NOT NULL DEFAULT SYSUTCDATETIME(),
  [UpdatedAt] DATETIMEOFFSET(7) NULL
);
```

-- OrderItems

```sql
CREATE TABLE [dbo].[OrderItems] (
  [Id] INT IDENTITY(1,1) PRIMARY KEY,
  [OrderId] INT NOT NULL REFERENCES [dbo].[Orders](Id) ON DELETE CASCADE,
  [ProductId] INT NOT NULL REFERENCES [dbo].[Products](Id),
  [ProductName] NVARCHAR(300) NOT NULL,
  [Quantity] INT NOT NULL,
  [UnitPrice] DECIMAL(12,2) NOT NULL,
  [Discount] DECIMAL(12,2) NOT NULL DEFAULT 0
);
```

-- Reviews

```sql
CREATE TABLE [dbo].[Reviews] (
  [Id] INT IDENTITY(1,1) PRIMARY KEY,
  [ProductId] INT NOT NULL REFERENCES [dbo].[Products](Id) ON DELETE CASCADE,
  [UserId] INT NOT NULL REFERENCES [dbo].[Users](Id) ON DELETE SET NULL,
  [Rating] TINYINT NOT NULL CHECK (Rating BETWEEN 1 AND 5),
  [Comment] NVARCHAR(2000) NULL,
  [CreatedAt] DATETIMEOFFSET(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
```

-- Optional Cart (if persisted server-side)

```sql
CREATE TABLE [dbo].[Carts] (
  [Id] INT IDENTITY(1,1) PRIMARY KEY,
  [UserId] INT NOT NULL REFERENCES [dbo].[Users](Id) ON DELETE CASCADE,
  [CreatedAt] DATETIMEOFFSET(7) NOT NULL DEFAULT SYSUTCDATETIME()
);

CREATE TABLE [dbo].[CartItems] (
  [Id] INT IDENTITY(1,1) PRIMARY KEY,
  [CartId] INT NOT NULL REFERENCES [dbo].[Carts](Id) ON DELETE CASCADE,
  [ProductId] INT NOT NULL REFERENCES [dbo].[Products](Id),
  [Quantity] INT NOT NULL
);
```

---

## Index recommendations

- Users: unique index on `Email` (already declared). Add a nonclustered index on `Role` if admin queries filter by role.
- Products: index on `CategoryId`, full-text index on `Name`/`Description` if search required.
- Orders: clustered PK on `Id` (identity). Add nonclustered index on `UserId` and `CreatedAt` to support user order history and recent-orders queries.`
- OrderItems: index on `OrderId` for quick join; index on `ProductId` if reverse lookups are common.
- Reviews: index on `ProductId` for aggregation (avg rating), and `UserId` for user's reviews.

Example:

```sql
CREATE INDEX IX_Orders_UserId_CreatedAt ON dbo.Orders(UserId, CreatedAt DESC);
CREATE INDEX IX_Products_CategoryId ON dbo.Products(CategoryId);
```

---

## Common queries

- Get recent orders for a user (paginated):

```sql
SELECT * FROM Orders
WHERE UserId = @userId
ORDER BY CreatedAt DESC
OFFSET @skip ROWS FETCH NEXT @pageSize ROWS ONLY;
```

- Get order with items:

```sql
SELECT o.*, oi.*
FROM Orders o
JOIN OrderItems oi ON oi.OrderId = o.Id
WHERE o.Id = @orderId;
```

- Recalculate product rating (periodic job or after review insert):

```sql
UPDATE Products
SET Rating = r.AvgRating, ReviewsCount = r.Count
FROM (
  SELECT ProductId, AVG(CAST(Rating AS DECIMAL(3,2))) AS AvgRating, COUNT(*) AS Count
  FROM Reviews
  WHERE ProductId = @productId
  GROUP BY ProductId
) r
WHERE Products.Id = r.ProductId;
```

---

## Migration, seeding and operational guidance

- Use EF Core migrations for schema changes. Keep incremental migrations small and reversible where possible.
- Seed minimal data (admin user, base categories) in `SeedData` with idempotent checks.
- Backups: take regular full backups and enable point-in-time restore if using managed DB.
- Apply schema changes first in a staging environment; test data migrations (e.g. column splits/merges) with scripts and backups.

Migration tips

- Add columns as nullable first, backfill data, then make NOT NULL with a migration to avoid downtime.
- Avoid long-running transactions during migration; break large operations into smaller batches.

---

## Performance & scaling notes

- For large catalogs, move heavy text (product descriptions) to separate table or use column compression.
- Consider read replicas for reporting and feeds.
- Use cache (Redis) for frequently-read data like category lists, product details and user sessions.
- Use optimistic concurrency for user edits (RowVersion) and reconcile conflicts in the API layer.

---

If you want, I can:

- generate EF Core migration-ready C# model classes based on this DDL,
- produce a PostgreSQL variant of the DDL,
- or add a SQL script that seeds the database with example users, categories and products for local development.
