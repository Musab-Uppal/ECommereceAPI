using ECommerce.Models;
using ECommerce.Repositories.Interfaces;
using ECommerce.Services.Implementation;
using ECommerce.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ECommerce.Tests.Unit.Services
{
    public class OrderServiceTests
    {
        private readonly Mock<IOrderRepository> _mockOrderRepository;
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<ILogger<OrderService>> _mockLogger;
        private readonly OrderService _orderService;

        public OrderServiceTests()
        {
            _mockOrderRepository = new Mock<IOrderRepository>();
            _mockProductRepository = new Mock<IProductRepository>();
            _mockLogger = new Mock<ILogger<OrderService>>();
            _orderService = new OrderService(
                _mockOrderRepository.Object,
                _mockProductRepository.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task CreateOrderAsync_WithValidData_CreatesOrder()
        {
            // Arrange
            var userId = 1;
            var createOrderDto = new CreateOrderDto
            {
                Items = new List<OrderItemInputDto>
                {
                    new OrderItemInputDto { ProductId = 1, Quantity = 2, Discount = 0 }
                }
            };

            var product = new Product
            {
                ProductId = 1,
                Name = "Test Product",
                Price = 1000,
                Stock = 10,
                Category = new Category { CategoryId = 1, Name = "Test" }
            };

            _mockOrderRepository
                .Setup(x => x.UserExistsAsync(userId))
                .ReturnsAsync(true);

            _mockProductRepository
                .Setup(x => x.GetProductByIdAsync(1))
                .ReturnsAsync(product);

            var createdOrder = new Order
            {
                OrderId = 1,
                UserId = userId,
                Status = "Pending",
                TotalAmount = 2000,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem { ProductId = 1, Quantity = 2, UnitPrice = 1000 }
                }
            };

            _mockOrderRepository
                .Setup(x => x.CreateOrderAsync(It.IsAny<Order>()))
                .ReturnsAsync(createdOrder);

            _mockOrderRepository
                .Setup(x => x.GetOrderByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(createdOrder);

            // Act
            var result = await _orderService.CreateOrderAsync(userId, createOrderDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Pending", result.Status);
            Assert.Equal(2000, result.TotalAmount);

            _mockOrderRepository.Verify(
                x => x.CreateOrderAsync(It.IsAny<Order>()),
                Times.Once
            );
        }

        [Fact]
        public async Task CreateOrderAsync_WithNonexistentUser_ThrowsKeyNotFoundException()
        {
            // Arrange
            var userId = 999;
            var createOrderDto = new CreateOrderDto { Items = new List<OrderItemInputDto>() };

            _mockOrderRepository
                .Setup(x => x.UserExistsAsync(userId))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => _orderService.CreateOrderAsync(userId, createOrderDto)
            );
        }

        [Fact]
        public async Task CreateOrderAsync_WithEmptyItems_ThrowsArgumentException()
        {
            // Arrange
            var userId = 1;
            var createOrderDto = new CreateOrderDto { Items = new List<OrderItemInputDto>() };

            _mockOrderRepository
                .Setup(x => x.UserExistsAsync(userId))
                .ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _orderService.CreateOrderAsync(userId, createOrderDto)
            );
        }

        [Fact]
        public async Task CancelOrderAsync_WithPendingOrder_CancelsAndRestoresStock()
        {
            // Arrange
            var orderId = 1;
            var product = new Product
            {
                ProductId = 1,
                Name = "Test",
                Stock = 8 // 10 - 2 from order
            };

            var order = new Order
            {
                OrderId = orderId,
                Status = "Pending",
                OrderItems = new List<OrderItem>
                {
                    new OrderItem { ProductId = 1, Quantity = 2 }
                }
            };

            _mockOrderRepository
                .Setup(x => x.GetOrderByIdAsync(orderId))
                .ReturnsAsync(order);

            _mockProductRepository
                .Setup(x => x.GetProductByIdAsync(1))
                .ReturnsAsync(product);

            _mockOrderRepository
                .Setup(x => x.DeleteOrderAsync(orderId))
                .ReturnsAsync(true);

            // Act
            var result = await _orderService.CancelOrderAsync(orderId);

            // Assert
            Assert.True(result);
            Assert.Equal(10, product.Stock); // Stock restored

            _mockProductRepository.Verify(
                x => x.UpdateProductAsync(product),
                Times.Once
            );
        }
    }
}