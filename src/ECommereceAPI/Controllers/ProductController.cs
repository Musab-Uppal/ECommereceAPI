using Microsoft.AspNetCore.Mvc;
using ECommereceAPI.Models;
using ECommereceAPI.Data;
namespace ECommereceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly ILogger<ProductController> _logger;
        private readonly ApplicationDbContext _context;
        public ProductController(ILogger<ProductController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;

        }

        [HttpGet("products")]
        public List<Product> GetProducts()
        {
            var products = _context.Products;
            return products.ToList();
        }
    }
}
