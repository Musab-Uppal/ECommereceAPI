using ECommerce.Models;
using ECommerce.Repositories.Interfaces;
using ECommerce.Services.Implementation;
using ECommerce.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ECommerce.Tests.Unit.Services
{
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<ILogger<ProductService>> _mockLogger;
        private readonly ProductService _productService;

        public ProductServiceTests()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _mockLogger = new Mock<ILogger<ProductService>>();
            _productService = new ProductService(_mockProductRepository.Object, _mockLogger.Object);
        }

        #region GetProductById

        [Fact]
        public async Task GetProductByIdAsync_WithValidId_ReturnsProduct()
        {
            // Arrange
            var productId = 1;
            var product = new Product
            {
                ProductId = productId,
                Name = "Test Product",
                Price = 1000,
                Stock = 10,
                CategoryId = 1,
                Category = new Category { CategoryId = 1, Name = "Electronics" }
            };

            _mockProductRepository
                .Setup(x => x.GetProductByIdAsync(productId))
                .ReturnsAsync(product);

            // Act
            var result = await _productService.GetProductByIdAsync(productId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(productId, result.ProductId);
            Assert.Equal("Test Product", result.Name);
            Assert.Equal(1000, result.Price);

            _mockProductRepository.Verify(x => x.GetProductByIdAsync(productId), Times.Once);
        }

        [Fact]
        public async Task GetProductByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var productId = 999;
            _mockProductRepository
                .Setup(x => x.GetProductByIdAsync(productId))
                .ReturnsAsync((Product)null);

            // Act
            var result = await _productService.GetProductByIdAsync(productId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetProductByIdAsync_WithZeroId_ThrowsArgumentException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _productService.GetProductByIdAsync(0)
            );
        }

        #endregion

        #region CreateProduct

        [Fact]
        public async Task CreateProductAsync_WithValidData_CreatesProduct()
        {
            // Arrange
            var createDto = new CreateProductDto
            {
                Name = "New Product",
                Description = "Test Description",
                ImageUrl = "https://example.com/product.png",
                Price = 5000,
                Stock = 20,
                CategoryId = 1
            };

            _mockProductRepository
                .Setup(x => x.CategoryExistsAsync(createDto.CategoryId))
                .ReturnsAsync(true);

            var createdProduct = new Product
            {
                ProductId = 1,
                Name = createDto.Name,
                Description = createDto.Description,
                ImageUrl = createDto.ImageUrl,
                Price = createDto.Price,
                Stock = createDto.Stock,
                CategoryId = createDto.CategoryId
            };

            _mockProductRepository
                .Setup(x => x.CreateProductAsync(It.IsAny<Product>()))
                .ReturnsAsync(createdProduct);

            // Act
            var result = await _productService.CreateProductAsync(createDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("New Product", result.Name);
            Assert.Equal(5000, result.Price);

            _mockProductRepository.Verify(
                x => x.CreateProductAsync(It.IsAny<Product>()),
                Times.Once
            );
        }

        [Fact]
        public async Task CreateProductAsync_WithNegativePrice_ThrowsArgumentException()
        {
            // Arrange
            var createDto = new CreateProductDto
            {
                Name = "Test",
                ImageUrl = "https://example.com/product.png",
                Price = -100,
                Stock = 10,
                CategoryId = 1
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _productService.CreateProductAsync(createDto)
            );
        }

        [Fact]
        public async Task CreateProductAsync_WithNonexistentCategory_ThrowsInvalidOperationException()
        {
            // Arrange
            var createDto = new CreateProductDto
            {
                Name = "Test",
                ImageUrl = "https://example.com/product.png",
                Price = 1000,
                Stock = 10,
                CategoryId = 999
            };

            _mockProductRepository
                .Setup(x => x.CategoryExistsAsync(999))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _productService.CreateProductAsync(createDto)
            );
        }

        #endregion

        #region GetAllProducts

        [Fact]
        public async Task GetAllProductsAsync_WithValidPagination_ReturnsProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { ProductId = 1, Name = "Product 1", Price = 1000 },
                new Product { ProductId = 2, Name = "Product 2", Price = 2000 }
            };

            _mockProductRepository
                .Setup(x => x.GetAllProductsAsync(1, 10))
                .ReturnsAsync(products);

            // Act
            var result = await _productService.GetAllProductsAsync(1, 10);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        #endregion
    }
}