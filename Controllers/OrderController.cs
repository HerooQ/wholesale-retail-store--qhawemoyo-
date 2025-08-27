using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WholesaleRetailStore.Data;
using WholesaleRetailStore.Models;
using WholesaleRetailStore.Models.DTOs;
using WholesaleRetailStore.Services;

namespace WholesaleRetailStore.Controllers
{
    /// <summary>
    /// Controller for managing orders in the wholesale/retail store system.
    /// Handles order creation, processing, stock management, and order history.
    /// </summary>
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;
        private readonly PricingService _pricingService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(AppDbContext context, PricingService pricingService, ILogger<OrderController> logger)
        {
            _context = context;
            _pricingService = pricingService;
            _logger = logger;
        }

        // MVC Views

        /// <summary>
        /// Displays a list of all orders
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View(orders);
        }

        /// <summary>
        /// Displays order details
        /// </summary>
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        /// <summary>
        /// Creates an order from a quote
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFromQuote(int customerId, Dictionary<int, int> productQuantities)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(customerId);
                if (customer == null)
                {
                    TempData["Error"] = "Customer not found.";
                    return RedirectToAction("Quote", "Pricing");
                }

                // Validate stock availability
                if (!_pricingService.ValidateStockAvailability(productQuantities))
                {
                    TempData["Error"] = "Insufficient stock for one or more products.";
                    return RedirectToAction("Quote", "Pricing", new { customerId });
                }

                // Create the order
                var order = await CreateOrderAsync(customerId, productQuantities);

                // Reduce stock
                _pricingService.ReduceStock(productQuantities);

                TempData["Success"] = $"Order #{order.Id} created successfully!";
                return RedirectToAction(nameof(Details), new { id = order.Id });
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Quote", "Pricing", new { customerId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order from quote");
                TempData["Error"] = "An error occurred while creating the order.";
                return RedirectToAction("Quote", "Pricing", new { customerId });
            }
        }

        /// <summary>
        /// Displays the order creation form
        /// </summary>
        public async Task<IActionResult> Create()
        {
            ViewBag.Customers = await _context.Customers.ToListAsync();
            ViewBag.Products = await _context.Products.ToListAsync();
            return View();
        }

        /// <summary>
        /// Processes the order creation form
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateOrderDto orderDto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Customers = await _context.Customers.ToListAsync();
                ViewBag.Products = await _context.Products.ToListAsync();
                return View(orderDto);
            }

            try
            {
                var productQuantities = orderDto.Items.ToDictionary(i => i.ProductId, i => i.Quantity);

                // Validate stock availability
                if (!_pricingService.ValidateStockAvailability(productQuantities))
                {
                    ModelState.AddModelError("", "Insufficient stock for one or more products.");
                    ViewBag.Customers = await _context.Customers.ToListAsync();
                    ViewBag.Products = await _context.Products.ToListAsync();
                    return View(orderDto);
                }

                // Create the order
                var order = await CreateOrderAsync(orderDto.CustomerId, productQuantities);

                // Reduce stock
                _pricingService.ReduceStock(productQuantities);

                TempData["Success"] = $"Order #{order.Id} created successfully!";
                return RedirectToAction(nameof(Details), new { id = order.Id });
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Customers = await _context.Customers.ToListAsync();
                ViewBag.Products = await _context.Products.ToListAsync();
                return View(orderDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                ModelState.AddModelError("", "An error occurred while creating the order.");
                ViewBag.Customers = await _context.Customers.ToListAsync();
                ViewBag.Products = await _context.Products.ToListAsync();
                return View(orderDto);
            }
        }

        // API Endpoints

        /// <summary>
        /// GET: api/orders
        /// Returns a list of all orders
        /// </summary>
        [HttpGet("api/orders")]
        public async Task<IActionResult> GetOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    CustomerId = o.CustomerId,
                    CustomerName = o.Customer!.Name,
                    CustomerType = o.Customer.CustomerType,
                    CreatedAt = o.CreatedAt,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    Items = o.OrderItems.Select(oi => new OrderItemDetailDto
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.Product!.Name,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.Quantity * oi.UnitPrice
                    }).ToList()
                })
                .ToListAsync();

            return Ok(orders);
        }

        /// <summary>
        /// GET: api/orders/{id}
        /// Returns a specific order by ID
        /// </summary>
        [HttpGet("api/orders/{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound(new { message = "Order not found" });
            }

            var orderDto = new OrderDto
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                CustomerName = order.Customer.Name!,
                CustomerType = order.Customer.CustomerType,
                CreatedAt = order.CreatedAt,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                Items = order.OrderItems.Select(oi => new OrderItemDetailDto
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product!.Name,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.Quantity * oi.UnitPrice
                }).ToList()
            };

            return Ok(orderDto);
        }

        /// <summary>
        /// POST: api/orders
        /// Creates a new order
        /// </summary>
        [HttpPost("api/orders")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto orderDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var productQuantities = orderDto.Items.ToDictionary(i => i.ProductId, i => i.Quantity);

                // Validate stock availability
                if (!_pricingService.ValidateStockAvailability(productQuantities))
                {
                    return BadRequest(new { message = "Insufficient stock for one or more products" });
                }

                // Create the order
                var order = await CreateOrderAsync(orderDto.CustomerId, productQuantities);

                // Reduce stock
                _pricingService.ReduceStock(productQuantities);

                var responseDto = new OrderDto
                {
                    Id = order.Id,
                    CustomerId = order.CustomerId,
                    CustomerName = order.Customer!.Name,
                    CustomerType = order.Customer.CustomerType,
                    CreatedAt = order.CreatedAt,
                    TotalAmount = order.TotalAmount,
                    Status = order.Status,
                    Items = order.OrderItems.Select(oi => new OrderItemDetailDto
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.Product!.Name,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.Quantity * oi.UnitPrice
                    }).ToList()
                };

                _logger.LogInformation("Order created via API: {OrderId} for customer {CustomerName}", order.Id, order.Customer!.Name);

                return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, responseDto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order via API");
                return StatusCode(500, new { message = "An error occurred while creating the order" });
            }
        }

        /// <summary>
        /// PUT: api/orders/{id}/status
        /// Updates the status of an order
        /// </summary>
        [HttpPut("api/orders/{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto statusDto)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound(new { message = "Order not found" });
            }

            if (!Enum.TryParse<OrderStatus>(statusDto.Status, out var newStatus))
            {
                return BadRequest(new { message = "Invalid order status" });
            }

            order.Status = newStatus;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Order status updated successfully" });
        }

        /// <summary>
        /// Helper method to create an order with proper pricing calculations
        /// </summary>
        private async Task<Order> CreateOrderAsync(int customerId, Dictionary<int, int> productQuantities)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null)
            {
                throw new InvalidOperationException("Customer not found");
            }

            // Generate quote to get pricing information
            var quote = _pricingService.GenerateQuote(customer, productQuantities);

            // Create the order
            var order = new Order
            {
                CustomerId = customerId,
                TotalAmount = quote.Total,
                Status = OrderStatus.Confirmed,
                CreatedAt = DateTime.UtcNow,
                OrderItems = new List<OrderItem>()
            };

            // Add order items
            foreach (var quoteItem in quote.Items)
            {
                var orderItem = new OrderItem
                {
                    ProductId = quoteItem.ProductId,
                    Quantity = quoteItem.Quantity,
                    UnitPrice = quoteItem.DiscountedPrice
                };
                order.OrderItems.Add(orderItem);
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return order;
        }
    }

    /// <summary>
    /// DTO for updating order status
    /// </summary>
    public class UpdateOrderStatusDto
    {
        /// <summary>
        /// New status for the order
        /// </summary>
        public string Status { get; set; } = string.Empty;
    }
}
