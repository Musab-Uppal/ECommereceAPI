using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ECommerce.Services.Interfaces;
using Stripe;
using System.IO;

namespace ECommerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IOrderService orderService, IConfiguration configuration, ILogger<PaymentController> logger)
        {
            _orderService = orderService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("create-intent")]
        [Authorize]
        public async Task<ActionResult> CreatePaymentIntent([FromQuery] int orderId)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(orderId);
                if (order == null) return NotFound("Order not found");

                if (order.Status != "Pending") return BadRequest($"Order is in {order.Status} status and cannot be paid.");

                var amountInCents = (long)(order.TotalAmount * 100);

                var options = new PaymentIntentCreateOptions
                {
                    Amount = amountInCents,
                    Currency = "usd",
                    Metadata = new Dictionary<string, string>
                    {
                        { "OrderId", orderId.ToString() }
                    },
                    AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                    {
                        Enabled = true,
                    },
                };

                var service = new PaymentIntentService();
                var intent = await service.CreateAsync(options);

                return Ok(new { clientSecret = intent.ClientSecret });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment intent");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var endpointSecret = _configuration["Stripe:WebhookSecret"];

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], endpointSecret);

                if (stripeEvent.Type == "payment_intent.succeeded")
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    if (paymentIntent != null && paymentIntent.Metadata.TryGetValue("OrderId", out string orderIdStr))
                    {
                        if (int.TryParse(orderIdStr, out int orderId))
                        {
                            await _orderService.UpdateOrderStatusAsync(orderId, "Paid");
                            _logger.LogInformation($"Order {orderId} marked as Paid.");
                        }
                    }
                }

                return Ok();
            }
            catch (StripeException e)
            {
                _logger.LogError(e, "Stripe webhook failed");
                return BadRequest();
            }
        }
    }
}
