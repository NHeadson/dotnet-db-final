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
  Console.WriteLine("Please make a Selection: ");
  Console.WriteLine("\nProducts:");
  Console.WriteLine("1) Display all EXISTING products");
  Console.WriteLine("2) Display a SPECIFIC product");
  Console.WriteLine("3) Add NEW product");
  Console.WriteLine("4) Edit EXISTING product");
  Console.WriteLine("\nCategories:");
  Console.WriteLine("5) Display all EXISTING categories (Name & Description)");
  Console.WriteLine("6) Display all EXISTING categories & all ACTIVE products");
  Console.WriteLine("7) Display a SPECIFIC category & all ACTIVE products");
  Console.WriteLine("8) Add NEW category");
  Console.WriteLine("9) Edit EXISTING category");
  Console.WriteLine("\nDELETE:");
  Console.WriteLine("10) Delete EXISTING category (May result in orphans)");
  Console.WriteLine("11) Delete EXISTING product (May result in orphans)");
  Console.WriteLine("Enter to quit");
  string? choice = Console.ReadLine();
  Console.Clear();
  logger.Info("Option {choice} selected", choice);

  var configuration = new ConfigurationBuilder().AddJsonFile($"appsettings.json");
  var config = configuration.Build();
  var db = new DataContext();

  if (choice == "1")
  {
    Console.Clear();
    // Display all products       1
    Console.WriteLine("1) All products");
    Console.WriteLine("2) Discontinued products");
    Console.WriteLine("3) Active products");
    string? filterChoice = Console.ReadLine();
    IQueryable<Product> query;
    Console.Clear();

    if (filterChoice == "1")
    {
      query = db.Products.OrderBy(p => p.ProductId);
    }
    else if (filterChoice == "2")
    {
      query = db.Products.Where(p => p.Discontinued).OrderBy(p => p.ProductId);
    }
    else
    {
      query = db.Products.Where(p => !p.Discontinued).OrderBy(p => p.ProductId);
    }

    Console.Clear();
    foreach (var product in query)
    {
      Console.WriteLine($"Product ID: {product.ProductId} - {product.ProductName} - {(product.Discontinued ? "Discontinued" : "Active")}");
    }
    logger.Info("Displayed products with filter {FilterChoice}", filterChoice);
  }
  else if (choice == "2")
  {
    Console.Clear();
    // Display specific product
    // Display all products with IDs and names
    var products = db.Products.OrderBy(p => p.ProductId).ToList();
    Console.WriteLine("Select the product to display:");
    foreach (var product in products)
    {
      Console.WriteLine($"ID: {product.ProductId} - Name: {product.ProductName}");
    }

    Console.WriteLine("Enter Product ID to display:");
    int productId = int.Parse(Console.ReadLine()!);
    Product productToDisplay = db.Products.FirstOrDefault(p => p.ProductId == productId)!;
    Console.Clear();

    if (productToDisplay != null)
    {
      Console.WriteLine($"ProductID: {productToDisplay.ProductId}");
      Console.WriteLine($"ProductName: {productToDisplay.ProductName}");
      Console.WriteLine($"SupplierID: {productToDisplay.SupplierId}");
      Console.WriteLine($"CategoryID: {productToDisplay.CategoryId}");
      Console.WriteLine($"QuantityPerUnit: {productToDisplay.QuantityPerUnit}");
      Console.WriteLine($"UnitPrice: {productToDisplay.UnitPrice}");
      Console.WriteLine($"UnitsInStock: {productToDisplay.UnitsInStock}");
      Console.WriteLine($"UnitsOnOrder: {productToDisplay.UnitsOnOrder}");
      Console.WriteLine($"ReorderLevel: {productToDisplay.ReorderLevel}");
      Console.WriteLine($"Discontinued: {productToDisplay.Discontinued}");
      logger.Info("Displayed product - {ProductName}", productToDisplay.ProductName);
    }
    else
    {
      logger.Error("Product ID {ProductId} not found", productId);
    }
  }
  else if (choice == "3")
  {
    Console.Clear();
    // Add new product        3
    Product product = new();

    Console.WriteLine("Enter Product Name:");
    product.ProductName = Console.ReadLine()!;
    Console.Clear();

    // Fetch and display valid supplier IDs
    var suppliers = db.Suppliers.OrderBy(s => s.SupplierId).ToList();
    Console.WriteLine("Valid Supplier IDs:");
    foreach (var supplier in suppliers)
    {
      Console.WriteLine($"ID: {supplier.SupplierId} - Name: {supplier.CompanyName}");
    }

    // Validate Supplier ID
    int supplierId = 0;
    do
    {
      Console.WriteLine("Enter Supplier ID:");
    } while (!int.TryParse(Console.ReadLine(), out supplierId) || !suppliers.Any(s => s.SupplierId == supplierId));
    product.SupplierId = supplierId;
    Console.Clear();

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
    Console.Clear();

    Console.WriteLine("Enter Quantity Per Unit:");
    product.QuantityPerUnit = Console.ReadLine();
    Console.Clear();

    Console.WriteLine("Enter Unit Price:");
    product.UnitPrice = Math.Round(decimal.Parse(Console.ReadLine()!), 2);
    Console.Clear();

    Console.WriteLine("Enter Units In Stock:");
    product.UnitsInStock = short.Parse(Console.ReadLine()!);
    Console.Clear();

    Console.WriteLine("Enter Units On Order:");
    product.UnitsOnOrder = short.Parse(Console.ReadLine()!);
    Console.Clear();

    Console.WriteLine("Enter Reorder Level:");
    product.ReorderLevel = short.Parse(Console.ReadLine()!);
    Console.Clear();

    bool isDiscontinued;
    Console.WriteLine("Is Discontinued (true/false):");
    while (!bool.TryParse(Console.ReadLine(), out isDiscontinued))
    {
      Console.WriteLine("Invalid input. Please enter 'true' or 'false':");
    }
    product.Discontinued = isDiscontinued;
    Console.Clear();

    db.Products.Add(product);
    db.SaveChanges();
    logger.Info("Product added - {ProductName}", product.ProductName);
  }
  else if (choice == "4")
  {
    Console.Clear();
    // Edit product
    // Display all products with IDs and names
    var products = db.Products.OrderBy(p => p.ProductId).ToList();
    Console.WriteLine("Select the product to edit:");
    foreach (var product in products)
    {
      Console.WriteLine($"ID: {product.ProductId} - Name: {product.ProductName}");
    }

    Console.WriteLine("Enter Product ID to edit:");
    int productId = int.Parse(Console.ReadLine()!);
    Console.Clear();
    Product productToEdit = db.Products.FirstOrDefault(p => p.ProductId == productId)!;

    if (productToEdit != null)
    {
      Console.WriteLine("Enter new Product Name (leave blank to keep current):");
      string productName = Console.ReadLine()!;
      if (!string.IsNullOrEmpty(productName)) productToEdit.ProductName = productName;
      Console.Clear();

      // Fetch and display valid supplier IDs
      var suppliers = db.Suppliers.OrderBy(s => s.SupplierId).ToList();
      Console.WriteLine("Valid Supplier IDs:");
      foreach (var supplier in suppliers)
      {
        Console.WriteLine($"ID: {supplier.SupplierId} - Name: {supplier.CompanyName}");
      }

      Console.WriteLine("Enter new Supplier ID (leave blank to keep current):");
      string supplierIdInput = Console.ReadLine()!;
      if (!string.IsNullOrEmpty(supplierIdInput))
      {
        int supplierId;
        while (!int.TryParse(supplierIdInput, out supplierId) || !suppliers.Any(s => s.SupplierId == supplierId))
        {
          Console.WriteLine("Invalid Supplier ID. Please enter a valid Supplier ID:");
          supplierIdInput = Console.ReadLine()!;
        }
        productToEdit.SupplierId = supplierId;
      }
      Console.Clear();

      // Fetch and display valid category IDs
      var categories = db.Categories.OrderBy(c => c.CategoryId).ToList();
      Console.WriteLine("Valid Category IDs:");
      foreach (var category in categories)
      {
        Console.WriteLine($"ID: {category.CategoryId} - Name: {category.CategoryName}");
      }

      Console.WriteLine("Enter new Category ID (leave blank to keep current):");
      string categoryIdInput = Console.ReadLine()!;
      if (!string.IsNullOrEmpty(categoryIdInput))
      {
        int categoryId;
        while (!int.TryParse(categoryIdInput, out categoryId) || !categories.Any(c => c.CategoryId == categoryId))
        {
          Console.WriteLine("Invalid Category ID. Please enter a valid Category ID:");
          categoryIdInput = Console.ReadLine()!;
        }
        productToEdit.CategoryId = categoryId;
      }
      Console.Clear();

      Console.WriteLine("Enter new Quantity Per Unit (leave blank to keep current):");
      string quantityPerUnit = Console.ReadLine()!;
      if (!string.IsNullOrEmpty(quantityPerUnit)) productToEdit.QuantityPerUnit = quantityPerUnit;
      Console.Clear();

      Console.WriteLine("Enter new Unit Price (leave blank to keep current):");
      string unitPrice = Console.ReadLine()!;
      if (!string.IsNullOrEmpty(unitPrice)) productToEdit.UnitPrice = decimal.Parse(unitPrice);
      Console.Clear();

      Console.WriteLine("Enter new Units In Stock (leave blank to keep current):");
      string unitsInStock = Console.ReadLine()!;
      if (!string.IsNullOrEmpty(unitsInStock)) productToEdit.UnitsInStock = short.Parse(unitsInStock);
      Console.Clear();

      Console.WriteLine("Enter new Units On Order (leave blank to keep current):");
      string unitsOnOrder = Console.ReadLine()!;
      if (!string.IsNullOrEmpty(unitsOnOrder)) productToEdit.UnitsOnOrder = short.Parse(unitsOnOrder);
      Console.Clear();

      Console.WriteLine("Enter new Reorder Level (leave blank to keep current):");
      string reorderLevel = Console.ReadLine()!;
      if (!string.IsNullOrEmpty(reorderLevel)) productToEdit.ReorderLevel = short.Parse(reorderLevel);
      Console.Clear();

      Console.WriteLine("Is Discontinued (true/false) (leave blank to keep current):");
      string discontinuedInput = Console.ReadLine()!;
      if (!string.IsNullOrEmpty(discontinuedInput))
      {
        bool isDiscontinued;
        while (!bool.TryParse(discontinuedInput, out isDiscontinued))
        {
          Console.WriteLine("Invalid input. Please enter 'true' or 'false':");
          discontinuedInput = Console.ReadLine()!;
        }
        productToEdit.Discontinued = isDiscontinued;
      }
      Console.Clear();

      db.SaveChanges();
      logger.Info("Product edited - {ProductName}", productToEdit.ProductName);
    }
    else
    {
      logger.Error("Product ID {ProductId} not found", productId);
    }
  }
  else if (choice == "5")
  {
    Console.Clear();
    // Display all categories (CategoryName and Description)    5
    var query = db.Categories.OrderBy(c => c.CategoryName);
    foreach (var category in query)
    {
      Console.WriteLine($"{category.CategoryName} - {category.Description}");
    }
    logger.Info("Displayed all categories");
  }
  else if (choice == "6")
  {
    Console.Clear();
    // Display all categories and their related active products     6
    var query = db.Categories.Include(c => c.Products).OrderBy(c => c.CategoryId);
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
  else if (choice == "7")
  {
    Console.Clear();
    // Display a specific category and its related active products
    // Display all categories with IDs and names
    var categories = db.Categories.OrderBy(c => c.CategoryId).ToList();
    Console.WriteLine("Select the category to display:");
    foreach (var category in categories)
    {
      Console.WriteLine($"ID: {category.CategoryId} - {category.CategoryName}");
    }

    Console.WriteLine("Enter Category ID to display:");
    int categoryId = int.Parse(Console.ReadLine()!);
    Category categoryToDisplay = db.Categories.Include(c => c.Products).FirstOrDefault(c => c.CategoryId == categoryId)!;

    if (categoryToDisplay != null)
    {
      Console.WriteLine($"ID: {categoryToDisplay.CategoryId} - {categoryToDisplay.CategoryName}");
      foreach (var product in categoryToDisplay.Products.Where(p => !p.Discontinued))
      {
        Console.WriteLine($"\tID: {product.ProductId} - {product.ProductName}");
      }
      logger.Info("Displayed category and its related active products - {CategoryName}", categoryToDisplay.CategoryName);
    }
    else
    {
      logger.Error("Category ID {CategoryId} not found", categoryId);
    }
  }
  else if (choice == "8")
  {
    Console.Clear();
    // Add category       8
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
  else if (choice == "9")
  {
    Console.Clear();
    // Edit category        9
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
    Console.Clear();
    // Delete a category      10
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
  else if (choice == "11")
  {
    Console.Clear();
    // Delete a product     11
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
  else if (String.IsNullOrEmpty(choice))
  {
    break;
  }
  Console.WriteLine();
} while (true);

logger.Info("Program ended");
