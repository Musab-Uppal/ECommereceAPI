using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using ECommerce.Repositories.Interfaces;

namespace ECommerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private const string SessionKey = "cart";
        private readonly IProductRepository _productRepository;

        public CartController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        private CartDto GetCartFromSession()
        {
            var json = HttpContext.Session.GetString(SessionKey);
            if (string.IsNullOrEmpty(json))
            {
                return new CartDto { Items = new List<CartItemDto>() };
            }
            return JsonSerializer.Deserialize<CartDto>(json) ?? new CartDto { Items = new List<CartItemDto>() };
        }

        private void SaveCartToSession(CartDto cart)
        {
            var json = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString(SessionKey, json);
        }

        [HttpGet]
        public ActionResult<CartDto> GetCart()
        {
            var cart = GetCartFromSession();
            cart.Recalculate();
            return Ok(cart);
        }

        [HttpPost]
        public async Task<ActionResult<CartDto>> AddToCart([FromBody] AddCartItemRequest request)
        {
            if (request == null || request.ProductId <= 0 || request.Quantity <= 0)
                return BadRequest("Invalid request");

            var product = await _productRepository.GetProductByIdAsync(request.ProductId);
            if (product == null)
                return NotFound("Product not found");

            var cart = GetCartFromSession();
            var existing = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
            if (existing != null)
            {
                existing.Quantity += request.Quantity;
            }
            else
            {
                cart.Items.Add(new CartItemDto
                {
                    ProductId = product.ProductId,
                    Name = product.Name,
                    Price = product.Price,
                    Quantity = request.Quantity
                });
            }

            cart.Recalculate();
            SaveCartToSession(cart);
            return Ok(cart);
        }

        [HttpPut("{productId}")]
        public ActionResult<CartDto> UpdateItem(int productId, [FromBody] UpdateCartItemRequest request)
        {
            if (request == null || request.Quantity < 0)
                return BadRequest("Invalid request");

            var cart = GetCartFromSession();
            var existing = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (existing == null)
                return NotFound("Item not in cart");

            if (request.Quantity == 0)
            {
                cart.Items.Remove(existing);
            }
            else
            {
                existing.Quantity = request.Quantity;
            }

            cart.Recalculate();
            SaveCartToSession(cart);
            return Ok(cart);
        }

        [HttpDelete("{productId}")]
        public ActionResult<CartDto> RemoveItem(int productId)
        {
            var cart = GetCartFromSession();
            var existing = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (existing != null)
            {
                cart.Items.Remove(existing);
                cart.Recalculate();
                SaveCartToSession(cart);
            }
            return Ok(cart);
        }

        [HttpDelete]
        [Route("clear")]
        public ActionResult ClearCart()
        {
            HttpContext.Session.Remove(SessionKey);
            return NoContent();
        }

        // DTOs
        public class AddCartItemRequest
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; } = 1;
        }

        public class UpdateCartItemRequest
        {
            public int Quantity { get; set; }
        }

        public class CartDto
        {
            public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
            public decimal Total { get; set; }
            public int TotalItems { get; set; }
            public void Recalculate()
            {
                Total = Items.Sum(i => i.Price * i.Quantity);
                TotalItems = Items.Sum(i => i.Quantity);
            }
        }

        public class CartItemDto
        {
            public int ProductId { get; set; }
            public string? Name { get; set; }
            public decimal Price { get; set; }
            public int Quantity { get; set; }
        }
    }
}
