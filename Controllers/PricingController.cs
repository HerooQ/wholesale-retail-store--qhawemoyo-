using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WholesaleRetailStore.Data;
using WholesaleRetailStore.Models;
using WholesaleRetailStore.Models.DTOs;
using WholesaleRetailStore.Services;

namespace WholesaleRetailStore.Controllers
{
    /// <summary>
    /// Controller for handling pricing calculations and quote generation.
    /// Provides pricing information based on customer type and current pricing rules.
    /// </summary>
    public class PricingController : Controller
    {
        private readonly AppDbContext _context;
        private readonly PricingService _pricingService;
        private readonly ILogger<PricingController> _logger;

        public PricingController(AppDbContext context, PricingService pricingService, ILogger<PricingController> logger)
        {
            _context = context;
            _pricingService = pricingService;
            _logger = logger;
        }

        // MVC Views

        /// <summary>
        /// Displays the quote generation form
        /// </summary>
        public async Task<IActionResult> Quote(int? customerId)
        {
            var customers = await _context.Customers.ToListAsync();
            var products = await _context.Products.ToListAsync();

            ViewBag.Customers = customers;
            ViewBag.Products = products;
            ViewBag.SelectedCustomerId = customerId;

            return View();
        }

        /// <summary>
        /// Processes the quote generation form and displays the quote
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateQuote(int customerId, Dictionary<int, int> quantities)
        {
            // Filter out zero quantities
            var validQuantities = quantities.Where(q => q.Value > 0)
                                           .ToDictionary(q => q.Key, q => q.Value);

            if (!validQuantities.Any())
            {
                TempData["Error"] = "Please select at least one product with quantity greater than 0.";
                return RedirectToAction(nameof(Quote), new { customerId });
            }

            try
            {
                var customer = await _context.Customers.FindAsync(customerId);
                if (customer == null)
                {
                    TempData["Error"] = "Customer not found.";
                    return RedirectToAction(nameof(Quote));
                }

                var quote = _pricingService.GenerateQuote(customer, validQuantities);

                ViewBag.Customer = customer;
                return View("QuoteResult", quote);
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Quote), new { customerId });
            }
        }

        /// <summary>
        /// Displays pricing rules management
        /// </summary>
        public async Task<IActionResult> Rules()
        {
            var pricingRules = await _context.PricingRules
                .OrderBy(r => r.CustomerType)
                .ThenBy(r => r.DiscountPercentage)
                .ToListAsync();

            return View(pricingRules);
        }

        // API Endpoints

        /// <summary>
        /// GET: api/pricing/quote/{customerId}?productId=1&quantity=5&productId=2&quantity=3
        /// Generates a quote for a customer with specified products and quantities
        /// </summary>
        [HttpGet("api/pricing/quote/{customerId}")]
        public async Task<IActionResult> GetQuote(int customerId, [FromQuery] List<int> productId, [FromQuery] List<int> quantity)
        {
            if (productId == null || quantity == null || productId.Count != quantity.Count)
            {
                return BadRequest(new { message = "Product IDs and quantities must be provided and have the same count" });
            }

            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null)
            {
                return NotFound(new { message = "Customer not found" });
            }

            var productQuantities = new Dictionary<int, int>();
            for (int i = 0; i < productId.Count; i++)
            {
                if (quantity[i] > 0)
                {
                    productQuantities[productId[i]] = quantity[i];
                }
            }

            if (!productQuantities.Any())
            {
                return BadRequest(new { message = "At least one product with quantity greater than 0 must be specified" });
            }

            try
            {
                var quote = _pricingService.GenerateQuote(customer, productQuantities);
                return Ok(quote);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// POST: api/pricing/quote
        /// Generates a quote using a structured request body
        /// </summary>
        [HttpPost("api/pricing/quote")]
        public async Task<IActionResult> GenerateQuoteApi([FromBody] QuoteRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var customer = await _context.Customers.FindAsync(request.CustomerId);
            if (customer == null)
            {
                return NotFound(new { message = "Customer not found" });
            }

            var productQuantities = request.Items.ToDictionary(i => i.ProductId, i => i.Quantity);

            try
            {
                var quote = _pricingService.GenerateQuote(customer, productQuantities);
                return Ok(quote);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// GET: api/pricing/rules
        /// Returns all active pricing rules
        /// </summary>
        [HttpGet("api/pricing/rules")]
        public async Task<IActionResult> GetPricingRules()
        {
            var rules = await _context.PricingRules
                .Where(r => r.IsActive)
                .OrderBy(r => r.CustomerType)
                .ThenBy(r => r.DiscountPercentage)
                .Select(r => new
                {
                    r.Id,
                    r.CustomerType,
                    r.DiscountPercentage,
                    r.MinimumOrderAmount,
                    r.Description,
                    DiscountFactor = r.DiscountFactor
                })
                .ToListAsync();

            return Ok(rules);
        }

        /// <summary>
        /// GET: api/pricing/products/{customerId}
        /// Returns all products with calculated prices for a specific customer
        /// </summary>
        [HttpGet("api/pricing/products/{customerId}")]
        public async Task<IActionResult> GetProductPrices(int customerId)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null)
            {
                return NotFound(new { message = "Customer not found" });
            }

            var products = await _context.Products
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Description,
                    p.Stock,
                    BasePrice = p.BasePrice,
                    CalculatedPrice = _pricingService.CalculatePrice(p, customer.CustomerType, 1),
                    CustomerType = customer.CustomerType
                })
                .ToListAsync();

            return Ok(new
            {
                CustomerId = customerId,
                CustomerName = customer.Name,
                CustomerType = customer.CustomerType,
                Products = products
            });
        }

        /// <summary>
        /// POST: api/pricing/compare
        /// Compares pricing between retail and wholesale for the same products
        /// </summary>
        [HttpPost("api/pricing/compare")]
        public async Task<IActionResult> ComparePricing([FromBody] ComparePricingDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var products = await _context.Products
                .Where(p => request.ProductIds.Contains(p.Id))
                .ToListAsync();

            var comparison = new List<PricingComparisonDto>();

            foreach (var product in products)
            {
                var retailPrice = _pricingService.CalculatePrice(product, CustomerType.Retail);
                var wholesalePrice = _pricingService.CalculatePrice(product, CustomerType.Wholesale);

                comparison.Add(new PricingComparisonDto
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    BasePrice = product.BasePrice,
                    RetailPrice = retailPrice,
                    WholesalePrice = wholesalePrice,
                    WholesaleDiscount = product.BasePrice - wholesalePrice,
                    WholesaleDiscountPercentage = ((product.BasePrice - wholesalePrice) / product.BasePrice) * 100
                });
            }

            return Ok(new
            {
                ProductIds = request.ProductIds,
                Comparison = comparison
            });
        }
    }

    /// <summary>
    /// DTO for quote generation requests
    /// </summary>
    public class QuoteRequestDto
    {
        /// <summary>
        /// Customer ID for the quote
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// List of items to include in the quote
        /// </summary>
        public List<QuoteItemRequestDto> Items { get; set; } = new List<QuoteItemRequestDto>();
    }

    /// <summary>
    /// DTO for individual quote items
    /// </summary>
    public class QuoteItemRequestDto
    {
        /// <summary>
        /// Product ID
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Quantity requested
        /// </summary>
        public int Quantity { get; set; }
    }

    /// <summary>
    /// DTO for pricing comparison requests
    /// </summary>
    public class ComparePricingDto
    {
        /// <summary>
        /// List of product IDs to compare pricing for
        /// </summary>
        public List<int> ProductIds { get; set; } = new List<int>();
    }

    /// <summary>
    /// DTO for pricing comparison results
    /// </summary>
    public class PricingComparisonDto
    {
        /// <summary>
        /// Product ID
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Product name
        /// </summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>
        /// Base retail price
        /// </summary>
        public decimal BasePrice { get; set; }

        /// <summary>
        /// Calculated retail price (same as base price)
        /// </summary>
        public decimal RetailPrice { get; set; }

        /// <summary>
        /// Calculated wholesale price after discounts
        /// </summary>
        public decimal WholesalePrice { get; set; }

        /// <summary>
        /// Wholesale discount amount
        /// </summary>
        public decimal WholesaleDiscount { get; set; }

        /// <summary>
        /// Wholesale discount percentage
        /// </summary>
        public decimal WholesaleDiscountPercentage { get; set; }
    }
}
