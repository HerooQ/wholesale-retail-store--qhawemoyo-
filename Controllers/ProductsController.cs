using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WholesaleRetailStore.Data;
using WholesaleRetailStore.Models;
using WholesaleRetailStore.Services;

namespace WholesaleRetailStore.Controllers
{
    /// <summary>
    /// Controller for managing products in the wholesale/retail store system.
    /// Provides both MVC views and REST API endpoints for product catalog.
    /// </summary>
    public class ProductsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly PricingService _pricingService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(AppDbContext context, PricingService pricingService, ILogger<ProductsController> logger)
        {
            _context = context;
            _pricingService = pricingService;
            _logger = logger;
        }

        /// <summary>
        /// Displays the product catalog with pricing information
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products.ToListAsync();
            ViewBag.PricingService = _pricingService;
            return View(products);
        }

        /// <summary>
        /// Displays product details with pricing breakdown
        /// </summary>
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            ViewBag.PricingService = _pricingService;
            return View(product);
        }

        // API Endpoints

        /// <summary>
        /// GET: api/products
        /// Returns a list of all products
        /// </summary>
        [HttpGet("api/products")]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _context.Products
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Description,
                    p.Stock,
                    p.BasePrice
                })
                .ToListAsync();

            return Ok(products);
        }

        /// <summary>
        /// GET: api/products/{id}
        /// Returns a specific product by ID
        /// </summary>
        [HttpGet("api/products/{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound(new { message = "Product not found" });
            }

            return Ok(new
            {
                product.Id,
                product.Name,
                product.Description,
                product.Stock,
                product.BasePrice
            });
        }

        /// <summary>
        /// GET: api/products/stock/{id}
        /// Returns stock information for a specific product
        /// </summary>
        [HttpGet("api/products/stock/{id}")]
        public async Task<IActionResult> GetProductStock(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound(new { message = "Product not found" });
            }

            return Ok(new
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Stock = product.Stock,
                IsAvailable = product.Stock > 0,
                StockStatus = product.Stock > 10 ? "In Stock" :
                             product.Stock > 0 ? "Low Stock" : "Out of Stock"
            });
        }
    }
}
