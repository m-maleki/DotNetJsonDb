using DotNetJsonDb;
using Example;

// Create a context instance specifying the base path for data files
var context = new JsonContext<Product>("data");

// Add a new product
var product = new Product { Id = 1 ,Name = "T-Shirt", Price = 19.99m };
context.Add(product);

// Get a product by ID
var retrievedProduct = context.Get(product.Id);

// Update the product
retrievedProduct.Price = 24.99m;
context.Update(retrievedProduct.Id, retrievedProduct);

// Get all products
var allProducts = context.GetAll();

// Remove a product
context.Remove(product.Id);