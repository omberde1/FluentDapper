# FluentDapper

A clean, fluent abstraction over [Dapper](https://github.com/DapperLib/Dapper) for SQL Server. Write less boilerplate, stay in full control of your SQL.

[![NuGet](https://img.shields.io/nuget/v/FluentDapper.svg)](https://www.nuget.org/packages/FluentDapper)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

---

## What is FluentDapper?

FluentDapper is a lightweight wrapper around Dapper that gives you a simple, organized API for the most common database operations — **Insert, Update, Delete, and Query** — without writing repetitive SQL or boilerplate connection management code.

It's ideal for developers who:
- Want the speed and control of Dapper
- Don't want to write the same INSERT/UPDATE/DELETE patterns over and over
- Prefer clean, readable code over heavy ORM magic

---

## Requirements

- .NET Framework or .NET (compatible with `System.Data.SqlClient`)
- SQL Server database
- Dapper (installed automatically as a dependency)

---

## Installation

Install via NuGet Package Manager:

```
Install-Package FluentDapper
```

Or via .NET CLI:

```bash
dotnet add package FluentDapper
```

---

## Getting Started

Create a single instance of `FluentDapper` and reuse it throughout your application (e.g., register it as a singleton in your DI container).

```csharp
var db = new FluentDapper("your_connection_string_here");
```

That's it. You now have access to four services:

| Property | Description |
|---|---|
| `db.Insert` | Insert single records, bulk insert, insert with identity |
| `db.Update` | Update by model, by SET clause, or raw SQL |
| `db.Delete` | Hard delete, soft delete, or raw SQL |
| `db.Query` | Fetch lists, single records, paged results, stored procedures |

---

## Insert

### Insert a single record (returns generated ID)

```csharp
var user = new User { Name = "Alice", Email = "alice@example.com" };
int newId = await db.Insert.EntityAsync("Users", user);
```

The `Id` property is automatically excluded from the INSERT — the database generates it, and it's returned to you.

### Insert and return a specific key type

```csharp
long newId = await db.Insert.EntityAsync<User, long>("Users", user);
```

### Insert with identity (manually specify the ID)

Use this when you need to insert a record with a specific ID value (bypasses SQL Server's auto-increment temporarily).

```csharp
var user = new User { Id = 999, Name = "Bob" };
await db.Insert.EntityWithIdentityAsync("Users", user);
```

### Bulk insert

Insert many records in a single call:

```csharp
var users = new List<User>
{
    new User { Name = "Alice" },
    new User { Name = "Bob" },
    new User { Name = "Carol" }
};

await db.Insert.BulkAsync("Users", users);
```

### Insert with raw SQL

```csharp
int newId = await db.Insert.SqlAsync<User>(
    "INSERT INTO Users (Name, Email) VALUES (@Name, @Email)",
    new { Name = "Dave", Email = "dave@example.com" }
);
```

`SELECT SCOPE_IDENTITY()` is automatically appended — you just get the new ID back.

---

## Update

### Update by model (auto-generates SET clause)

Only non-null properties are included in the update. The `Id` property is always excluded from the SET clause.

```csharp
var updates = new User { Name = "Updated Name" };
await db.Update.EntityAsync("Users", updates, "Id = @Id", new { Id = 1 });
```

> **Tip:** Pass only the properties you want to change. Null properties are skipped automatically.

### Update with a custom SET clause

```csharp
await db.Update.SetAsync(
    tableName: "Users",
    setClause: "Name = @Name, IsActive = @IsActive",
    whereClause: "Id = @Id",
    param: new { Name = "New Name", IsActive = true, Id = 5 }
);
```

### Update with raw SQL

```csharp
await db.Update.SqlAsync(
    "UPDATE Users SET LastLogin = GETDATE() WHERE Id = @Id",
    new { Id = 1 }
);
```

---

## Delete

### Hard delete (permanently removes the record)

```csharp
await db.Delete.HardAsync("Users", "Id = @Id", new { Id = 1 });
```

> ⚠️ This physically removes the row. Use with caution.

### Soft delete (marks a record as deleted)

Useful when you want to keep the data but flag it as removed (e.g., an `IsDeleted` column):

```csharp
await db.Delete.SoftAsync(
    tableName: "Users",
    setClause: "IsDeleted = 1, DeletedAt = GETDATE()",
    whereClause: "Id = @Id",
    param: new { Id = 1 }
);
```

### Delete with raw SQL

```csharp
await db.Delete.SqlAsync(
    "DELETE FROM Logs WHERE CreatedAt < @CutoffDate",
    new { CutoffDate = DateTime.UtcNow.AddDays(-30) }
);
```

---

## Query

### Get a list of records

```csharp
var users = await db.Query.ListAsync<User>("Users");
```

With filters and ordering:

```csharp
var activeUsers = await db.Query.ListAsync<User>(
    tableName: "Users",
    whereClause: "IsActive = 1",
    orderBy: "Name ASC"
);
```

### Get a list using a filter model

Pass an object — only non-null properties are used as filters:

```csharp
var filter = new UserFilter { IsActive = true, RoleId = 2 };
var users = await db.Query.ListByWhereModelAsync<UserFilter, User>("Users", filter, orderBy: "Name");
```

### Get a list with raw SQL

```csharp
var users = await db.Query.ListSqlAsync<User>(
    "SELECT * FROM Users WHERE CreatedAt > @Since",
    new { Since = DateTime.UtcNow.AddDays(-7) }
);
```

### Get a single record

```csharp
var user = await db.Query.SingleAsync<User>("Users", "Id = @Id", param: new { Id = 1 });
```

Returns `null` (or default) if no match is found — no exceptions thrown.

### Get a single record using a filter model

```csharp
var filter = new UserFilter { Email = "alice@example.com" };
var user = await db.Query.SingleByWhereModelAsync<UserFilter, User>("Users", filter);
```

### Get a single record with raw SQL

```csharp
var user = await db.Query.SingleSqlAsync<User>(
    "SELECT TOP 1 * FROM Users WHERE Email = @Email",
    new { Email = "alice@example.com" }
);
```

### Get a single column as a list

Useful when you only need one column (e.g., a list of IDs or names):

```csharp
var names = await db.Query.ColumnSqlAsync<string>("SELECT Name FROM Users WHERE IsActive = 1");
```

### Get a single scalar value

```csharp
int totalUsers = await db.Query.ValueSqlAsync<int>("SELECT COUNT(*) FROM Users");
```

---

## Pagination

All paged methods use SQL Server's `OFFSET`/`FETCH NEXT` syntax. An `orderBy` clause is required.

### Paged list from a table

```csharp
var page1 = await db.Query.PagedListAsync<User>(
    tableName: "Users",
    pageNumber: 1,
    pageSize: 20,
    whereClause: "IsActive = 1",
    orderBy: "Name ASC"
);
```

### Paged list using a filter model

String properties automatically use `LIKE` for partial matching:

```csharp
var filter = new UserFilter { Name = "ali" }; // matches "Alice", "Alicia", etc.

var results = await db.Query.PageListByWhereModelAsync<UserFilter, User>(
    tableName: "Users",
    filterModel: filter,
    pageNumber: 1,
    pageSize: 10,
    orderBy: "Name ASC"
);
```

### Paged list with raw SQL

Your SQL must include an `ORDER BY` clause before calling this method:

```csharp
var results = await db.Query.PageListSqlAsync<User>(
    sql: "SELECT * FROM Users WHERE IsActive = 1 ORDER BY Name ASC",
    pageNumber: 2,
    pageSize: 10
);
```

---

## Stored Procedures

### Get a list from a stored procedure

```csharp
var users = await db.Query.SPListAsync<User>(
    "sp_GetActiveUsers",
    new { RoleId = 1 }
);
```

### Get a single result from a stored procedure

```csharp
var user = await db.Query.SPSingleAsync<User>(
    "sp_GetUserById",
    new { Id = 1 }
);
```

### Handle multiple result sets

Use `SPMultipleSetsAsync` when your stored procedure returns more than one result set:

```csharp
var result = await db.Query.SPMultipleSetsAsync<DashboardData>(
    storedProcedureName: "sp_GetDashboard",
    param: new { UserId = 1 },
    mapFunc: async grid =>
    {
        var stats = await grid.ReadAsync<UserStats>();
        var recentOrders = await grid.ReadAsync<Order>();
        return new DashboardData
        {
            Stats = stats.FirstOrDefault(),
            RecentOrders = recentOrders.ToList()
        };
    }
);
```

---

## Transactions

All methods accept optional `SqlConnection` and `SqlTransaction` parameters, allowing you to coordinate multiple operations in a single transaction.

```csharp
using var conn = new SqlConnection("your_connection_string");
await conn.OpenAsync();
using var transaction = conn.BeginTransaction();

try
{
    int orderId = await db.Insert.EntityAsync("Orders", order, conn, transaction);
    await db.Insert.BulkAsync("OrderItems", items, conn, transaction);

    transaction.Commit();
}
catch
{
    transaction.Rollback();
    throw;
}
```

---

## Tips & Best Practices

**Singleton usage** — Create one `FluentDapper` instance per connection string and reuse it. It does not hold open connections; it opens and closes them per operation.

**Null property skipping** — `EntityAsync` for inserts and updates only includes non-null properties. This means you can pass partial models and only the populated fields will be written.

**`Id` column convention** — Properties named `Id` (case-insensitive) are automatically excluded from INSERT and UPDATE statements. Your database handles identity generation.

**WHERE clause safety** — Always provide meaningful WHERE clauses for updates and deletes to avoid accidentally modifying all rows in a table.

**Raw SQL** — All `SqlAsync` variants give you full control when the model-based helpers don't fit your use case.

---

## License

MIT — free to use, modify, and distribute.

---

## Contributing

Issues and pull requests are welcome. If you find a bug or have a feature request, please open an issue on GitHub.
