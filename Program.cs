using NLog;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using NorthwindConsole.Model;
using System.ComponentModel.DataAnnotations;

string path = Directory.GetCurrentDirectory() + "//nlog.config";

// create instance of Logger
var logger = LogManager.Setup().LoadConfigurationFromFile(path).GetCurrentClassLogger();

logger.Info("Program started");

do
{
  Console.WriteLine("1) Display categories");
  Console.WriteLine("2) Add category");
  Console.WriteLine("3) Display Category and related products");
  Console.WriteLine("4) Display all Categories and their related products");
  Console.WriteLine("5) Add new product");
  Console.WriteLine("6) Edit product");
  Console.WriteLine("7) Display all products");
  Console.WriteLine("8) Display specific product");
  Console.WriteLine("9) Edit category");
  Console.WriteLine("10) Display all categories (Name and Description)");
  Console.WriteLine("11) Display all categories with all ACTIVE products");
  Console.WriteLine("12) Display a specific category with all ACTIVE products");
  Console.WriteLine("13) Delete a product");
  Console.WriteLine("14) Delete a category");
  Console.WriteLine("Enter to quit");
  string? choice = Console.ReadLine();
  Console.Clear();
  logger.Info("Option {choice} selected", choice);

  var configuration = new ConfigurationBuilder().AddJsonFile($"appsettings.json");
  var config = configuration.Build();
  var db = new DataContext();

  if (choice == "1")
  {
    // display categories
    var query = db.Categories.OrderBy(p => p.CategoryId);

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"{query.Count()} records returned");
    Console.ForegroundColor = ConsoleColor.Magenta;
    foreach (var item in query)
    {
      Console.WriteLine($"ID: {item.CategoryId} - {item.CategoryName} - {item.Description}");
    }
    Console.ForegroundColor = ConsoleColor.White;
  }
  else if (choice == "2")
  {
    // Add category
    Category category = new();
    Console.WriteLine("Enter Category Name:");
    category.CategoryName = Console.ReadLine()!;
    Console.WriteLine("Enter the Category Description:");
    category.Description = Console.ReadLine();
    ValidationContext context = new ValidationContext(category, null, null);
    List<ValidationResult> results = new List<ValidationResult>();

    var isValid = Validator.TryValidateObject(category, context, results, true);
    if (isValid)
    {
      // check for unique name
      if (db.Categories.Any(c => c.CategoryName == category.CategoryName))
      {
        // generate validation error
        isValid = false;
        results.Add(new ValidationResult("Name exists", new[] { "CategoryName" }));
      }
      else
      {
        logger.Info("Validation passed");
        db.Categories.Add(category);
        db.SaveChanges();
        logger.Info("Category added - {CategoryName}", category.CategoryName);
      }
    }
    if (!isValid)
    {
      foreach (var result in results)
      {
        logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
      }
    }
  }
  else if (choice == "3")
  {
    var query = db.Categories.OrderBy(p => p.CategoryId);
    Console.WriteLine("Select the category whose products you want to display:");
    Console.ForegroundColor = ConsoleColor.DarkRed;
    foreach (var item in query)
    {
      Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
    }
    Console.ForegroundColor = ConsoleColor.White;

    int id = int.Parse(Console.ReadLine()!);
    Console.Clear();
    logger.Info($"CategoryId {id} selected");
    Category category = db.Categories.Include("Products").FirstOrDefault(c => c.CategoryId == id)!;
    Console.WriteLine($"Category ID: {category.CategoryId} - {category.CategoryName} - {category.Description}");
    foreach (Product p in category.Products)
    {
      Console.WriteLine($"\tProduct ID: {p.ProductId} - {p.ProductName}");
    }
  }
  else if (choice == "4")
  {
    var query = db.Categories.Include("Products").OrderBy(p => p.CategoryId);
    foreach (var item in query)
    {
      Console.WriteLine($"Category ID: {item.CategoryId} - {item.CategoryName}");
      foreach (Product p in item.Products)
      {
        Console.WriteLine($"\tProduct ID: {p.ProductId} - {p.ProductName}");
      }
    }
  }
  else if (choice == "5")
{
    // Add new product
    Product product = new();

    Console.WriteLine("Enter Product Name:");
    product.ProductName = Console.ReadLine()!;

    // Fetch and display valid supplier IDs
    var suppliers = db.Suppliers.OrderBy(s => s.SupplierId).ToList();
    Console.WriteLine("Valid Supplier IDs:");
    foreach (var supplier in suppliers)
    {
        Console.WriteLine($"ID: {supplier.SupplierId} - Name: {supplier.CompanyName}");
    }

    // Validate Supplier ID
    int supplierId;
    do
    {
        Console.WriteLine("Enter Supplier ID:");
    } while (!int.TryParse(Console.ReadLine(), out supplierId) || !suppliers.Any(s => s.SupplierId == supplierId));
    product.SupplierId = supplierId;

    // Fetch and display valid category IDs
    var categories = db.Categories.OrderBy(c => c.CategoryId).ToList();
    Console.WriteLine("Valid Category IDs:");
    foreach (var category in categories)
    {
        Console.WriteLine($"ID: {category.CategoryId} - Name: {category.CategoryName}");
    }

    // Validate Category ID
    int categoryId;
    do
    {
        Console.WriteLine("Enter Category ID:");
    } while (!int.TryParse(Console.ReadLine(), out categoryId) || !categories.Any(c => c.CategoryId == categoryId));
    product.CategoryId = categoryId;

    Console.WriteLine("Enter Quantity Per Unit:");
    product.QuantityPerUnit = Console.ReadLine();

    Console.WriteLine("Enter Unit Price:");
    product.UnitPrice = decimal.Parse(Console.ReadLine()!);

    Console.WriteLine("Enter Units In Stock:");
    product.UnitsInStock = short.Parse(Console.ReadLine()!);

    Console.WriteLine("Enter Units On Order:");
    product.UnitsOnOrder = short.Parse(Console.ReadLine()!);

    Console.WriteLine("Enter Reorder Level:");
    product.ReorderLevel = short.Parse(Console.ReadLine()!);

    bool isDiscontinued;
    Console.WriteLine("Is Discontinued (true/false):");
    while (!bool.TryParse(Console.ReadLine(), out isDiscontinued))
    {
        Console.WriteLine("Invalid input. Please enter 'true' or 'false':");
    }
    product.Discontinued = isDiscontinued;

    db.Products.Add(product);
    db.SaveChanges();
    logger.Info("Product added - {ProductName}", product.ProductName);
}
  else if (choice == "6")
  {
    // Edit product
    var products = db.Products.OrderBy(p => p.ProductId).ToList();
    Console.WriteLine("Select the product to edit:");
    foreach (var product in products)
    {
      Console.WriteLine($"ID: {product.ProductId} - Name: {product.ProductName}");
    }

    Console.WriteLine("Enter Product ID to edit:");
    int productId = int.Parse(Console.ReadLine()!);
    Product productToEdit = db.Products.FirstOrDefault(p => p.ProductId == productId)!;

    if (productToEdit != null)
    {
      Console.WriteLine("Enter new Product Name (leave blank to keep current):");
      string productName = Console.ReadLine()!;
      if (!string.IsNullOrEmpty(productName)) productToEdit.ProductName = productName;

      Console.WriteLine("Enter new Supplier ID (leave blank to keep current):");
      string supplierId = Console.ReadLine()!;
      if (!string.IsNullOrEmpty(supplierId)) productToEdit.SupplierId = int.Parse(supplierId);

      Console.WriteLine("Enter new Category ID (leave blank to keep current):");
      string categoryId = Console.ReadLine()!;
      if (!string.IsNullOrEmpty(categoryId)) productToEdit.CategoryId = int.Parse(categoryId);

      Console.WriteLine("Enter new Quantity Per Unit (leave blank to keep current):");
      string quantityPerUnit = Console.ReadLine()!;
      if (!string.IsNullOrEmpty(quantityPerUnit)) productToEdit.QuantityPerUnit = quantityPerUnit;

      Console.WriteLine("Enter new Unit Price (leave blank to keep current):");
      string unitPrice = Console.ReadLine()!;
      if (!string.IsNullOrEmpty(unitPrice)) productToEdit.UnitPrice = decimal.Parse(unitPrice);

      Console.WriteLine("Enter new Units In Stock (leave blank to keep current):");
      string unitsInStock = Console.ReadLine()!;
      if (!string.IsNullOrEmpty(unitsInStock)) productToEdit.UnitsInStock = short.Parse(unitsInStock);

      Console.WriteLine("Enter new Units On Order (leave blank to keep current):");
      string unitsOnOrder = Console.ReadLine()!;
      if (!string.IsNullOrEmpty(unitsOnOrder)) productToEdit.UnitsOnOrder = short.Parse(unitsOnOrder);

      Console.WriteLine("Enter new Reorder Level (leave blank to keep current):");
      string reorderLevel = Console.ReadLine()!;
      if (!string.IsNullOrEmpty(reorderLevel)) productToEdit.ReorderLevel = short.Parse(reorderLevel);

      Console.WriteLine("Is Discontinued (true/false) (leave blank to keep current):");
      string discontinued = Console.ReadLine()!;
      if (!string.IsNullOrEmpty(discontinued)) productToEdit.Discontinued = bool.Parse(discontinued);

      db.SaveChanges();
      logger.Info("Product edited - {ProductName}", productToEdit.ProductName);
    }
    else
    {
      logger.Error("Product ID {ProductId} not found", productId);
    }
  }
  else if (choice == "7")
  {
    // Display all products
    Console.WriteLine("1) All products");
    Console.WriteLine("2) Discontinued products");
    Console.WriteLine("3) Active products");
    string? filterChoice = Console.ReadLine();
    IQueryable<Product> query;

    if (filterChoice == "1")
    {
      query = db.Products.OrderBy(p => p.ProductName);
    }
    else if (filterChoice == "2")
    {
      query = db.Products.Where(p => p.Discontinued).OrderBy(p => p.ProductName);
    }
    else
    {
      query = db.Products.Where(p => !p.Discontinued).OrderBy(p => p.ProductName);
    }

    foreach (var product in query)
    {
      Console.WriteLine($"Product ID: {product.ProductId} - {product.ProductName} - {(product.Discontinued ? "Discontinued" : "Active")}");
    }
    logger.Info("Displayed products with filter {FilterChoice}", filterChoice);
  }
  else if (choice == "8")
  {
    // Display specific product
    Console.WriteLine("Enter Product ID to display:");
    int productId = int.Parse(Console.ReadLine()!);
    Product product = db.Products.FirstOrDefault(p => p.ProductId == productId)!;

    if (product != null)
    {
      Console.WriteLine($"ProductID: {product.ProductId}");
      Console.WriteLine($"ProductName: {product.ProductName}");
      Console.WriteLine($"SupplierID: {product.SupplierId}");
      Console.WriteLine($"CategoryID: {product.CategoryId}");
      Console.WriteLine($"QuantityPerUnit: {product.QuantityPerUnit}");
      Console.WriteLine($"UnitPrice: {product.UnitPrice}");
      Console.WriteLine($"UnitsInStock: {product.UnitsInStock}");
      Console.WriteLine($"UnitsOnOrder: {product.UnitsOnOrder}");
      Console.WriteLine($"ReorderLevel: {product.ReorderLevel}");
      Console.WriteLine($"Discontinued: {product.Discontinued}");
      logger.Info("Displayed product - {ProductName}", product.ProductName);
    }
    else
    {
      logger.Error("Product ID {ProductId} not found", productId);
    }
  }
  else if (choice == "9")
  {
    // Edit category
    var categories = db.Categories.OrderBy(c => c.CategoryId).ToList();
    Console.WriteLine("Select the category to edit:");
    foreach (var category in categories)
    {
      Console.WriteLine($"ID: {category.CategoryId} - Name: {category.CategoryName}");
    }

    Console.WriteLine("Enter Category ID to edit:");
    int categoryId = int.Parse(Console.ReadLine()!);
    Category categoryToEdit = db.Categories.FirstOrDefault(c => c.CategoryId == categoryId)!;

    if (categoryToEdit != null)
    {
      Console.WriteLine("Enter new Category Name (leave blank to keep current):");
      string categoryName = Console.ReadLine()!;
      if (!string.IsNullOrEmpty(categoryName)) categoryToEdit.CategoryName = categoryName;

      Console.WriteLine("Enter new Description (leave blank to keep current):");
      string description = Console.ReadLine()!;
      if (!string.IsNullOrEmpty(description)) categoryToEdit.Description = description;

      db.SaveChanges();
      logger.Info("Category edited - {CategoryName}", categoryToEdit.CategoryName);
    }
    else
    {
      logger.Error("Category ID {CategoryId} not found", categoryId);
    }
  }
  else if (choice == "10")
  {
    // Display all categories (CategoryName and Description)
    var query = db.Categories.OrderBy(c => c.CategoryName);
    foreach (var category in query)
    {
      Console.WriteLine($"{category.CategoryName} - {category.Description}");
    }
    logger.Info("Displayed all categories");
  }
  else if (choice == "11")
  {
    // Display all categories and their related active products
    var query = db.Categories.Include(c => c.Products).OrderBy(c => c.CategoryName);
    foreach (var category in query)
    {
      Console.WriteLine($"{category.CategoryName}");
      foreach (var product in category.Products.Where(p => !p.Discontinued))
      {
        Console.WriteLine($"\t{product.ProductName}");
      }
    }
    logger.Info("Displayed all categories and their related active products");
  }
  else if (choice == "12")
  {
    // Display a specific category and its related active products
    Console.WriteLine("Enter Category ID to display:");
    int categoryId = int.Parse(Console.ReadLine()!);
    Category category = db.Categories.Include(c => c.Products).FirstOrDefault(c => c.CategoryId == categoryId)!;

    if (category != null)
    {
      Console.WriteLine($"ID: {category.CategoryId} - {category.CategoryName}");
      foreach (var product in category.Products.Where(p => !p.Discontinued))
      {
        Console.WriteLine($"\tID: {product.ProductId} - {product.ProductName}");
      }
      logger.Info("Displayed category and its related active products - {CategoryName}", category.CategoryName);
    }
    else
    {
      logger.Error("Category ID {CategoryId} not found", categoryId);
    }
  }
  else if (choice == "13")
  {
    // Delete a product
    var products = db.Products.OrderBy(p => p.ProductId).ToList();
    Console.WriteLine("Select the product to delete:");
    foreach (var product in products)
    {
      Console.WriteLine($"ID: {product.ProductId} - Name: {product.ProductName}");
    }

    Console.WriteLine("Enter Product ID to delete:");
    int productId = int.Parse(Console.ReadLine()!);
    Product productToDelete = db.Products.Include(p => p.OrderDetails).FirstOrDefault(p => p.ProductId == productId)!;

    if (productToDelete != null)
    {
      // Present a warning to the user
      Console.WriteLine("Warning: Deleting this product may leave orphaned records in related tables.");
      Console.WriteLine("Do you want to proceed? (yes/no)");
      string? confirm = Console.ReadLine();

      if (confirm?.ToLower() == "yes")
      {
        // Remove related order details
        db.OrderDetails.RemoveRange(productToDelete.OrderDetails);
        db.Products.Remove(productToDelete);
        db.SaveChanges();
        logger.Info("Product deleted - {ProductName}", productToDelete.ProductName);
      }
      else
      {
        logger.Info("Product deletion canceled by user.");
      }
    }
    else
    {
      logger.Error("Product ID {ProductId} not found", productId);
    }
  }
  else if (choice == "14")
  {
    // Delete a category
    var categories = db.Categories.OrderBy(c => c.CategoryId).ToList();
    Console.WriteLine("Select the category to delete:");
    foreach (var category in categories)
    {
      Console.WriteLine($"ID: {category.CategoryId} - Name: {category.CategoryName}");
    }

    Console.WriteLine("Enter Category ID to delete:");
    int categoryId = int.Parse(Console.ReadLine()!);
    Category categoryToDelete = db.Categories.Include(c => c.Products).FirstOrDefault(c => c.CategoryId == categoryId)!;

    if (categoryToDelete != null)
    {
      // Present a warning to the user
      Console.WriteLine("Warning: Deleting this category may leave orphaned records in related tables.");
      Console.WriteLine("Do you want to proceed? (yes/no)");
      string? confirm = Console.ReadLine();

      if (confirm?.ToLower() == "yes")
      {
        // Remove related products and their order details
        foreach (var product in categoryToDelete.Products)
        {
          db.OrderDetails.RemoveRange(product.OrderDetails);
          db.Products.Remove(product);
        }
        db.Categories.Remove(categoryToDelete);
        db.SaveChanges();
        logger.Info("Category deleted - {CategoryName}", categoryToDelete.CategoryName);
      }
      else
      {
        logger.Info("Category deletion canceled by user.");
      }
    }
    else
    {
      logger.Error("Category ID {CategoryId} not found", categoryId);
    }
  }
  else if (String.IsNullOrEmpty(choice))
  {
    break;
  }
  Console.WriteLine();
} while (true);

logger.Info("Program ended");
