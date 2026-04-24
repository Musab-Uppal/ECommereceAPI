using ECommereceAPI.Models;
using ECommereceAPI.Data;
using ECommereceAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommereceAPI.Data
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                // If data already exists, don't seed again
                if (context.Users.Any() || context.Categories.Any())
                {
                    Console.WriteLine("Database already seeded. Skipping seed data.");
                    return;
                }

                Console.WriteLine("Seeding database with initial data...");

                try
                {
                    // Seed Categories
                    var categories = new Category[]
                    {
                        new Category
                        {
                            Name = "Electronics",
                            Description = "Electronic devices and gadgets",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new Category
                        {
                            Name = "Clothing",
                            Description = "Men and women clothing",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new Category
                        {
                            Name = "Books",
                            Description = "Physical and digital books",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new Category
                        {
                            Name = "Home & Kitchen",
                            Description = "Home appliances and kitchen items",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new Category
                        {
                            Name = "Sports",
                            Description = "Sports equipment and accessories",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        }
                    };

                    foreach (var category in categories)
                    {
                        context.Categories.Add(category);
                    }
                    context.SaveChanges();
                    Console.WriteLine("✓ Categories seeded");

                    // Seed Users (with plain passwords for demo - DON'T DO THIS IN PRODUCTION)
                    // In real app, use BCrypt to hash passwords
                    var users = new User[]
                    {
                        new User
                        {
                            Email = "admin@ecommerce.pk",
                            Phone = "03001234567",
                            PasswordHash = "admin123", // Temporary - will be hashed later
                            FirstName = "Musab",
                            LastName = "Uppal",
                            Address = "Lahore, Punjab, Pakistan",
                            Role = "Admin",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new User
                        {
                            Email = "customer1@example.com",
                            Phone = "03009876543",
                            PasswordHash = "customer123",
                            FirstName = "Ali",
                            LastName = "Khan",
                            Address = "Islamabad, Pakistan",
                            Role = "Customer",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new User
                        {
                            Email = "customer2@example.com",
                            Phone = "03115551234",
                            PasswordHash = "customer123",
                            FirstName = "Fatima",
                            LastName = "Ahmed",
                            Address = "Karachi, Sindh, Pakistan",
                            Role = "Customer",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new User
                        {
                            Email = "customer3@example.com",
                            Phone = "03125558888",
                            PasswordHash = "customer123",
                            FirstName = "Hassan",
                            LastName = "Malik",
                            Address = "Multan, Punjab, Pakistan",
                            Role = "Customer",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        }
                    };

                    foreach (var user in users)
                    {
                        context.Users.Add(user);
                    }
                    context.SaveChanges();
                    Console.WriteLine("✓ Users seeded (1 Admin, 3 Customers)");

                    // Seed Products
                    var products = new Product[]
                    {
                        new Product
                        {
                            Name = "Dell Laptop",
                            Description = "High-performance laptop with 16GB RAM and 512GB SSD",
                            Price = 150000,
                            Stock = 10,
                            CategoryId = 1,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new Product
                        {
                            Name = "Wireless Mouse",
                            Description = "Ergonomic wireless mouse with USB receiver",
                            Price = 2500,
                            Stock = 50,
                            CategoryId = 1,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new Product
                        {
                            Name = "Mechanical Keyboard",
                            Description = "RGB mechanical keyboard with Cherry MX switches",
                            Price = 8500,
                            Stock = 25,
                            CategoryId = 1,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new Product
                        {
                            Name = "Cotton T-Shirt",
                            Description = "Premium quality cotton t-shirt, available in multiple colors",
                            Price = 1500,
                            Stock = 100,
                            CategoryId = 2,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new Product
                        {
                            Name = "Jeans",
                            Description = "Classic blue denim jeans with comfortable fit",
                            Price = 3500,
                            Stock = 60,
                            CategoryId = 2,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new Product
                        {
                            Name = "C# Programming Guide",
                            Description = "Comprehensive guide to C# programming for beginners to advanced",
                            Price = 2500,
                            Stock = 40,
                            CategoryId = 3,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new Product
                        {
                            Name = ".NET Core Guide",
                            Description = "Learn ASP.NET Core web development from basics",
                            Price = 3000,
                            Stock = 30,
                            CategoryId = 3,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new Product
                        {
                            Name = "Electric Kettle",
                            Description = "1.5L stainless steel electric kettle with auto shut-off",
                            Price = 2800,
                            Stock = 35,
                            CategoryId = 4,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new Product
                        {
                            Name = "Yoga Mat",
                            Description = "Non-slip yoga mat, 6mm thick, eco-friendly",
                            Price = 4500,
                            Stock = 20,
                            CategoryId = 5,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new Product
                        {
                            Name = "Dumbbells Set",
                            Description = "Adjustable dumbbells set (5kg - 25kg)",
                            Price = 12000,
                            Stock = 15,
                            CategoryId = 5,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        }
                    };

                    foreach (var product in products)
                    {
                        context.Products.Add(product);
                    }
                    context.SaveChanges();
                    Console.WriteLine("✓ Products seeded (10 products)");

                    // Seed Orders with OrderItems
                    var order1 = new Order
                    {
                        UserId = 2, // customer1
                        OrderDate = DateTime.UtcNow.AddDays(-5),
                        Status = "Delivered",
                        TotalAmount = 158500,
                        CreatedAt = DateTime.UtcNow.AddDays(-5),
                        UpdatedAt = DateTime.UtcNow
                    };

                    context.Orders.Add(order1);
                    context.SaveChanges();

                    // Add items to order1
                    var orderItem1 = new OrderItem
                    {
                        OrderId = order1.OrderId,
                        ProductId = 1, // Dell Laptop
                        Quantity = 1,
                        UnitPrice = 150000,
                        Discount = 0,
                        CreatedAt = DateTime.UtcNow.AddDays(-5)
                    };

                    var orderItem2 = new OrderItem
                    {
                        OrderId = order1.OrderId,
                        ProductId = 2, // Wireless Mouse
                        Quantity = 2,
                        UnitPrice = 2500,
                        Discount = 500, // 500 discount on this item
                        CreatedAt = DateTime.UtcNow.AddDays(-5)
                    };

                    context.OrderItems.Add(orderItem1);
                    context.OrderItems.Add(orderItem2);

                    // Order 2
                    var order2 = new Order
                    {
                        UserId = 3, // customer2
                        OrderDate = DateTime.UtcNow.AddDays(-2),
                        Status = "Shipped",
                        TotalAmount = 9000,
                        CreatedAt = DateTime.UtcNow.AddDays(-2),
                        UpdatedAt = DateTime.UtcNow
                    };

                    context.Orders.Add(order2);
                    context.SaveChanges();

                    var orderItem3 = new OrderItem
                    {
                        OrderId = order2.OrderId,
                        ProductId = 4, // Cotton T-Shirt
                        Quantity = 6,
                        UnitPrice = 1500,
                        Discount = 0,
                        CreatedAt = DateTime.UtcNow.AddDays(-2)
                    };

                    context.OrderItems.Add(orderItem3);

                    // Order 3
                    var order3 = new Order
                    {
                        UserId = 4, // customer3
                        OrderDate = DateTime.UtcNow.AddDays(-1),
                        Status = "Pending",
                        TotalAmount = 4500,
                        CreatedAt = DateTime.UtcNow.AddDays(-1),
                        UpdatedAt = DateTime.UtcNow
                    };

                    context.Orders.Add(order3);
                    context.SaveChanges();

                    var orderItem4 = new OrderItem
                    {
                        OrderId = order3.OrderId,
                        ProductId = 9, // Yoga Mat
                        Quantity = 1,
                        UnitPrice = 4500,
                        Discount = 0,
                        CreatedAt = DateTime.UtcNow.AddDays(-1)
                    };

                    context.OrderItems.Add(orderItem4);
                    context.SaveChanges();
                    Console.WriteLine("✓ Orders seeded (3 orders with items)");

                    // Seed Reviews
                    var reviews = new Review[]
                    {
                        new Review
                        {
                            ProductId = 1, // Dell Laptop
                            UserId = 2, // customer1
                            Rating = 5,
                            ReviewText = "Excellent laptop, very fast and reliable!",
                            CreatedAt = DateTime.UtcNow.AddDays(-3)
                        },
                        new Review
                        {
                            ProductId = 2, // Wireless Mouse
                            UserId = 2,
                            Rating = 4,
                            ReviewText = "Good quality mouse, comfortable to use",
                            CreatedAt = DateTime.UtcNow.AddDays(-3)
                        },
                        new Review
                        {
                            ProductId = 4, // Cotton T-Shirt
                            UserId = 3,
                            Rating = 4,
                            ReviewText = "Nice quality fabric, fits perfectly",
                            CreatedAt = DateTime.UtcNow.AddDays(-1)
                        },
                        new Review
                        {
                            ProductId = 9, // Yoga Mat
                            UserId = 4,
                            Rating = 5,
                            ReviewText = "Perfect for yoga, very comfortable and non-slip",
                            CreatedAt = DateTime.UtcNow
                        }
                    };

                    foreach (var review in reviews)
                    {
                        context.Reviews.Add(review);
                    }
                    context.SaveChanges();
                    Console.WriteLine("✓ Reviews seeded");

                    Console.WriteLine("\n✅ Database seeded successfully!");
                    Console.WriteLine("Admin User: admin@ecommerce.pk / admin123");
                    Console.WriteLine("Customer Users: customer1@example.com, customer2@example.com, customer3@example.com\n");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error seeding database: {ex.Message}");
                    throw;
                }
            }
        }
    }
}