using Microsoft.EntityFrameworkCore;
using ECommerce.Data;

namespace ECommerce.Tests.Fixtures
{
    public class DatabaseFixture : IDisposable
    {
        public ApplicationDbContext Context { get; }

        public DatabaseFixture()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            Context = new ApplicationDbContext(options);
            Context.Database.EnsureCreated();
            SeedTestData();
        }

        private void SeedTestData()
        {
            // Add test categories
            Context.Categories.AddRange(
                new ECommerce.Models.Category
                {
                    CategoryId = 1,
                    Name = "Electronics",
                    Description = "Electronic devices",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new ECommerce.Models.Category
                {
                    CategoryId = 2,
                    Name = "Books",
                    Description = "Books and reading materials",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            );

            // Add test products
            Context.Products.AddRange(
                new ECommerce.Models.Product
                {
                    ProductId = 1,
                    Name = "Test Product 1",
                    Description = "Test Description 1",
                    Price = 1000,
                    Stock = 10,
                    CategoryId = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new ECommerce.Models.Product
                {
                    ProductId = 2,
                    Name = "Test Product 2",
                    Description = "Test Description 2",
                    Price = 2000,
                    Stock = 5,
                    CategoryId = 1,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            );

            // Add test users
            Context.Users.AddRange(
                new ECommerce.Models.User
                {
                    UserId = 1,
                    Email = "admin@test.com",
                    PasswordHash = "$2a$12$test",
                    FirstName = "Admin",
                    LastName = "User",
                    Phone = "03001234567",
                    Address = "Test Address",
                    Role = "Admin",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new ECommerce.Models.User
                {
                    UserId = 2,
                    Email = "customer@test.com",
                    PasswordHash = "$2a$12$test",
                    FirstName = "Customer",
                    LastName = "User",
                    Phone = "03009876543",
                    Address = "Customer Address",
                    Role = "Customer",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            );

            Context.SaveChanges();
        }

        public void Dispose()
        {
            Context?.Dispose();
        }
    }
}