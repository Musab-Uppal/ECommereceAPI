using Xunit;
using ECommerce.Repositories.Implementation;
using ECommerce.Tests.Fixtures;
using Microsoft.Extensions.Logging;
using Moq;

namespace ECommerce.Tests.Unit.Repositories
{
    public class OrderRepositoryTests : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _fixture;
        private readonly Mock<ILogger<OrderRepository>> _mockLogger;
        private readonly OrderRepository _repository;

        public OrderRepositoryTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
            _mockLogger = new Mock<ILogger<OrderRepository>>();
            _repository = new OrderRepository(_fixture.Context, _mockLogger.Object);
        }

        #region CreateOrderAsync

        [Fact]
        public async Task CreateOrderAsync_WithValidOrder_CreatesOrder()
        {
            // Arrange
            var order = TestDataFactory.CreateTestOrder(id: 100, userId: 1);

            // Act
            var result = await _repository.CreateOrderAsync(order);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(100, result.OrderId);
            Assert.Equal("Pending", result.Status);
        }

        #endregion

        #region GetOrderByIdAsync

        [Fact]
        public async Task GetOrderByIdAsync_AfterCreation_ReturnsOrder()
        {
            // Arrange
            var order = TestDataFactory.CreateTestOrder(id: 101, userId: 1);
            await _repository.CreateOrderAsync(order);

            // Act
            var result = await _repository.GetOrderByIdAsync(101);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(101, result.OrderId);
        }

        [Fact]
        public async Task GetOrderByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Act
            var result = await _repository.GetOrderByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region UserExistsAsync

        [Fact]
        public async Task UserExistsAsync_WithValidId_ReturnsTrue()
        {
            // Act
            var result = await _repository.UserExistsAsync(1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task UserExistsAsync_WithInvalidId_ReturnsFalse()
        {
            // Act
            var result = await _repository.UserExistsAsync(999);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region UpdateOrderAsync

        [Fact]
        public async Task UpdateOrderAsync_WithValidOrder_UpdatesOrder()
        {
            // Arrange
            var order = TestDataFactory.CreateTestOrder(id: 102, userId: 1);
            await _repository.CreateOrderAsync(order);

            order.Status = "Shipped";

            // Act
            var result = await _repository.UpdateOrderAsync(order);

            // Assert
            Assert.Equal("Shipped", result.Status);
        }

        #endregion

        #region DeleteOrderAsync

        [Fact]
        public async Task DeleteOrderAsync_WithValidId_DeletesOrder()
        {
            // Arrange
            var order = TestDataFactory.CreateTestOrder(id: 103, userId: 1);
            await _repository.CreateOrderAsync(order);

            // Act
            var result = await _repository.DeleteOrderAsync(103);

            // Assert
            Assert.True(result);

            // Verify deletion
            var deletedOrder = await _repository.GetOrderByIdAsync(103);
            Assert.Null(deletedOrder);
        }

        #endregion
    }
}