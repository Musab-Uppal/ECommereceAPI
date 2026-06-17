using ECommerce.Controllers;
using ECommerce.Services.Interfaces;
using ECommerce.Tests.Fixtures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace ECommerce.Tests.Unit.Controllers
{
    public class OrderControllerTests
    {
        private readonly Mock<IOrderService> _mockOrderService;
        private readonly Mock<ILogger<OrderController>> _mockLogger;
        private readonly OrderController _controller;

        public OrderControllerTests()
        {
            _mockOrderService = new Mock<IOrderService>();
            _mockLogger = new Mock<ILogger<OrderController>>();
            _controller = new OrderController(_mockOrderService.Object, _mockLogger.Object);

            // Setup user claims
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("sub", "1"),
                new Claim(ClaimTypes.Email, "test@example.com")
            }));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        #region CreateOrder

        [Fact]
        public async Task CreateOrder_WithValidData_ReturnsCreatedAtAction()
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

            var createdOrder = new OrderServiceDto
            {
                OrderId = 1,
                UserId = userId,
                UserEmail = "test@example.com",
                OrderDate = DateTime.UtcNow,
                Status = "Pending",
                TotalAmount = 2000,
                OrderItems = new List<OrderItemServiceDto>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _mockOrderService
                .Setup(x => x.CreateOrderAsync(userId, createOrderDto))
                .ReturnsAsync(createdOrder);

            // Act
            var result = await _controller.CreateOrder(userId, createOrderDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.NotNull(createdResult);
        }

        [Fact]
        public async Task CreateOrder_WithInvalidData_ReturnsBadRequest()
        {
            // Arrange
            var userId = 1;
            var createOrderDto = new CreateOrderDto { Items = new List<OrderItemInputDto>() };

            _mockOrderService
                .Setup(x => x.CreateOrderAsync(userId, createOrderDto))
                .ThrowsAsync(new ArgumentException("No items in order"));

            // Act
            var result = await _controller.CreateOrder(userId, createOrderDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.NotNull(badRequestResult);
        }

        #endregion

        #region GetMyOrders

        [Fact]
        public async Task GetMyOrders_WithAuthentication_ReturnsOkWithOrders()
        {
            // Arrange
            var userId = 1;
            var orders = new List<OrderServiceDto>
            {
                new OrderServiceDto
                {
                    OrderId = 1,
                    UserId = userId,
                    UserEmail = "test@example.com",
                    OrderDate = DateTime.UtcNow,
                    Status = "Pending",
                    TotalAmount = 2000,
                    OrderItems = new List<OrderItemServiceDto>(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            _mockOrderService
                .Setup(x => x.GetUserOrdersAsync(userId))
                .ReturnsAsync(orders);

            // Act
            var result = await _controller.GetMyOrders(userId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);
        }

        #endregion

        #region GetOrderById

        [Fact]
        public async Task GetOrderById_WithValidId_ReturnsOkWithOrder()
        {
            // Arrange
            var orderId = 1;
            var order = new OrderServiceDto
            {
                OrderId = orderId,
                UserId = 1,
                UserEmail = "test@example.com",
                OrderDate = DateTime.UtcNow,
                Status = "Pending",
                TotalAmount = 2000,
                OrderItems = new List<OrderItemServiceDto>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _mockOrderService
                .Setup(x => x.GetOrderByIdAsync(orderId))
                .ReturnsAsync(order);

            // Act
            var result = await _controller.GetOrderById(orderId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult);
        }

        [Fact]
        public async Task GetOrderById_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var orderId = 999;
            _mockOrderService
                .Setup(x => x.GetOrderByIdAsync(orderId))
                .ReturnsAsync((OrderServiceDto)null);

            // Act
            var result = await _controller.GetOrderById(orderId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFoundResult);
        }

        #endregion

        #region CancelOrder

        [Fact]
        public async Task CancelOrder_WithValidId_ReturnsOk()
        {
            // Arrange
            var orderId = 1;
            _mockOrderService
                .Setup(x => x.CancelOrderAsync(orderId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.CancelOrder(orderId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult);
        }

        [Fact]
        public async Task CancelOrder_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var orderId = 999;
            _mockOrderService
                .Setup(x => x.CancelOrderAsync(orderId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.CancelOrder(orderId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFoundResult);
        }

        #endregion

        #region UpdateOrderStatus

        [Fact]
        public async Task UpdateOrderStatus_WithValidData_ReturnsOkWithUpdatedOrder()
        {
            // Arrange
            var orderId = 1;
            var updateDto = new UpdateOrderStatusDto { NewStatus = "Shipped" };

            var updatedOrder = new OrderServiceDto
            {
                OrderId = orderId,
                UserId = 1,
                UserEmail = "test@example.com",
                OrderDate = DateTime.UtcNow,
                Status = "Shipped",
                TotalAmount = 2000,
                OrderItems = new List<OrderItemServiceDto>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _mockOrderService
                .Setup(x => x.UpdateOrderStatusAsync(orderId, "Shipped"))
                .ReturnsAsync(updatedOrder);

            // Act
            var result = await _controller.UpdateOrderStatus(orderId, updateDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult);
        }

        #endregion
    }
}