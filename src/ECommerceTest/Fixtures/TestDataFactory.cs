using ECommerce.Models;

namespace ECommerce.Tests.Fixtures
{
    public static class TestDataFactory
    {
        public static Product CreateTestProduct(
            int id = 1,
            string name = "Test Product",
            decimal price = 1000,
            int stock = 10,
            int categoryId = 1)
        {
            return new Product
            {
                ProductId = id,
                Name = name,
                Description = "Test Description",
                Price = price,
                Stock = stock,
                CategoryId = categoryId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static Category CreateTestCategory(
            int id = 1,
            string name = "Test Category")
        {
            return new Category
            {
                CategoryId = id,
                Name = name,
                Description = "Test Category Description",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static User CreateTestUser(
            int id = 1,
            string email = "test@example.com",
            string role = "Customer")
        {
            return new User
            {
                UserId = id,
                Email = email,
                PasswordHash = "$2a$12$test",
                FirstName = "Test",
                LastName = "User",
                Phone = "03001234567",
                Address = "Test Address",
                Role = role,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static Order CreateTestOrder(
            int id = 1,
            int userId = 1,
            string status = "Pending")
        {
            return new Order
            {
                OrderId = id,
                UserId = userId,
                Status = status,
                TotalAmount = 2000,
                OrderDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                OrderItems = new List<OrderItem>()
            };
        }

        public static Review CreateTestReview(
            int productId = 1,
            int userId = 1,
            int rating = 5)
        {
            return new Review
            {
                ProductId = productId,
                UserId = userId,
                Rating = rating,
                ReviewText = "Great product!",
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}