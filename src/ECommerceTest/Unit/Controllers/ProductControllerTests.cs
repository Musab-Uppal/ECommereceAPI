using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using ECommerce.Controllers;
using ECommerce.Services.Interfaces;
using ECommerce.Tests.Fixtures;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace ECommerce.Tests.Unit.Controllers
{
    public class ProductControllerTests
    {
        private readonly Mock<IProductService> _mockProductService;
        private readonly Mock<ILogger<ProductController>> _mockLogger;
        private readonly Mock<IWebHostEnvironment> _mockEnvironment;
        private readonly ProductController _controller;

        public ProductControllerTests()
        {
            _mockProductService = new Mock<IProductService>();
            _mockLogger = new Mock<ILogger<ProductController>>();
            _mockEnvironment = new Mock<IWebHostEnvironment>();
            _controller = new ProductController(_mockProductService.Object, _mockLogger.Object, _mockEnvironment.Object);
        }

        #region GetAllProducts

        [Fact]
        public async Task GetAllProducts_WithValidPagination_ReturnsOkWithProducts()
        {
            // Arrange
            var products = new List<ProductServiceDto>
            {
                new ProductServiceDto
                {
                    ProductId = 1,
                    Name = "Product 1",
                    Description = "Test Description",
                    Price = 1000,
                    Stock = 10,
                    CategoryId = 1,
                    CategoryName = "Electronics",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            _mockProductService
                .Setup(x => x.GetAllProductsAsync(1, 10))
                .ReturnsAsync(products);

            // Act
            var result = await _controller.GetAllProducts(1, 10);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);
        }

        #endregion

        #region GetProductById

        [Fact]
        public async Task GetProductById_WithValidId_ReturnsOkWithProduct()
        {
            // Arrange
            var productId = 1;
            var product = new ProductServiceDto
            {
                ProductId = productId,
                Name = "Test Product",
                Description = "Test Description",
                Price = 1000,
                Stock = 10,
                CategoryId = 1,
                CategoryName = "Electronics",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _mockProductService
                .Setup(x => x.GetProductByIdAsync(productId))
                .ReturnsAsync(product);

            // Act
            var result = await _controller.GetProductById(productId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedProduct = Assert.IsType<ProductServiceDto>(okResult.Value);
            Assert.Equal("Test Product", returnedProduct.Name);
        }

        [Fact]
        public async Task GetProductById_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var productId = 999;
            _mockProductService
                .Setup(x => x.GetProductByIdAsync(productId))
                .ReturnsAsync((ProductServiceDto)null);

            // Act
            var result = await _controller.GetProductById(productId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.NotNull(notFoundResult);
        }

        #endregion

        #region CreateProduct

        [Fact]
        public async Task CreateProduct_WithValidData_ReturnsCreatedAtAction()
        {
            // Arrange
            var createDto = new CreateProductDto
            {
                Name = "New Product",
                Description = "New Product Description",
                ImageUrl = "https://example.com/product.png",
                Price = 5000,
                Stock = 20,
                CategoryId = 1
            };

            var createdProduct = new ProductServiceDto
            {
                ProductId = 1,
                Name = "New Product",
                Description = "New Product Description",
                ImageUrl = "https://example.com/product.png",
                Price = 5000,
                Stock = 20,
                CategoryId = 1,
                CategoryName = "Electronics",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _mockProductService
                .Setup(x => x.CreateProductAsync(createDto))
                .ReturnsAsync(createdProduct);

            // Act
            var result = await _controller.CreateProduct(createDto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.NotNull(createdResult);
            Assert.Equal(nameof(ProductController.GetProductById), createdResult.ActionName);
        }

        [Fact]
        public async Task CreateProduct_WithInvalidData_ReturnsBadRequest()
        {
            // Arrange
            var createDto = new CreateProductDto 
            { 
                Name = "", 
                Description = "Test",
                ImageUrl = "https://example.com/product.png",
                Price = -100, 
                Stock = 10, 
                CategoryId = 1 
            };

            _mockProductService
                .Setup(x => x.CreateProductAsync(createDto))
                .ThrowsAsync(new ArgumentException("Invalid price"));

            // Act
            var result = await _controller.CreateProduct(createDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.NotNull(badRequestResult);
        }

        #endregion

        #region UpdateProduct

        [Fact]
        public async Task UpdateProduct_WithValidData_ReturnsOkWithUpdatedProduct()
        {
            // Arrange
            var productId = 1;
            var updateDto = new UpdateProductDto
            {
                Name = "Updated Name",
                Description = "Updated Description",
                Price = 1500
            };

            var updatedProduct = new ProductServiceDto
            {
                ProductId = productId,
                Name = "Updated Name",
                Description = "Updated Description",
                Price = 1500,
                Stock = 10,
                CategoryId = 1,
                CategoryName = "Electronics",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _mockProductService
                .Setup(x => x.UpdateProductAsync(productId, updateDto))
                .ReturnsAsync(updatedProduct);

            // Act
            var result = await _controller.UpdateProduct(productId, updateDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedProduct = Assert.IsType<ProductServiceDto>(okResult.Value);
            Assert.Equal("Updated Name", returnedProduct.Name);
        }

        #endregion

        #region DeleteProduct

        [Fact]
        public async Task DeleteProduct_WithValidId_ReturnsOk()
        {
            // Arrange
            var productId = 1;
            _mockProductService
                .Setup(x => x.DeleteProductAsync(productId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteProduct(productId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult);
        }

        [Fact]
        public async Task DeleteProduct_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var productId = 999;
            _mockProductService
                .Setup(x => x.DeleteProductAsync(productId))
                .ThrowsAsync(new KeyNotFoundException());

            // Act
            var result = await _controller.DeleteProduct(productId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFoundResult);
        }

        #endregion
    }
}