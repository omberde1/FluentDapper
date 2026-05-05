# 🚀 FluentDapper

**The fluent bridge between your POCOs and Dapper. Write less SQL, do more.**

FluentDapper is a lightweight middleman for [Dapper](https://github.com) designed to eliminate query boilerplate and provide a clean, readable API for data access in .NET.

## ✨ Features
* **Zero Boilerplate**: Perform CRUD operations without writing raw strings.
* **Fluent API**: Highly readable method chaining for building queries.
* **Performance**: Maintains Dapper's famous high-speed execution.

## 📦 Installation
```bash
dotnet add package FluentDapper
```

## 🛠️ Quick Start
```csharp
// FluentDapper makes it simple
var user = connection.Fluent<User>()
    .Where(u => u.Id == 1)
    .ExecuteSingle();
```

## 🤝 Contributing
Suggestions and bug reports are welcome! Please open an **Issue** to discuss changes before submitting a **Pull Request**.

## 📄 License
This project is licensed under the **MIT License**.
