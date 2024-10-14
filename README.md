# DotNetJsonDb : A Simple JSON Persistence Library

This is a lightweight C# library that provides basic CRUD (Create, Read, Update, Delete) operations for storing and retrieving data in JSON format.

# Features:

Simple API: The library offers a concise and easy-to-use interface for adding, getting, updating, and removing data objects.
File-Based Persistence: Data is persisted in JSON files, making it portable and easy to manage.
Error Handling: Exceptions are thrown for common errors like file access issues and missing data during retrieval or update.

# Getting Started

Install the Package: NuGet\Install-Package DotNetJsonDb

# Usage Example:

```csharp
using DotNetJsonDb;

// Create a context instance specifying the base path for data files
var context = new JsonContext<Product>("data");

// Add a new product
var product = new Product { Name = "T-Shirt", Price = 19.99m };
context.Add(product);

// Get a product by ID
var retrievedProduct = context.GetById(product.Id);

// Update the product
retrievedProduct.Price = 24.99m;
context.Update(retrievedProduct.Id, retrievedProduct);

// Get all products
var allProducts = context.GetAll();

// Remove a product
context.Remove(product.Id);

public class Product
{
  public int Id { get; set; }
  public string Name { get; set; }
  public decimal Price { get; set; }
}

