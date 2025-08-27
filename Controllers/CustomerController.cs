using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WholesaleRetailStore.Data;
using WholesaleRetailStore.Models;
using WholesaleRetailStore.Models.DTOs;

namespace WholesaleRetailStore.Controllers
{
    /// <summary>
    /// Controller for managing customers in the wholesale/retail store system.
    /// Provides both MVC views and REST API endpoints.
    /// </summary>
    public class CustomerController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(AppDbContext context, ILogger<CustomerController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // MVC Views

        /// <summary>
        /// Displays a list of all customers
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var customers = await _context.Customers.ToListAsync();
            return View(customers);
        }

        /// <summary>
        /// Displays the create customer form
        /// </summary>
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Processes the create customer form submission
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCustomerDto customerDto)
        {
            if (!ModelState.IsValid)
            {
                return View(customerDto);
            }

            // Check if email already exists
            if (await _context.Customers.AnyAsync(c => c.Email == customerDto.Email))
            {
                ModelState.AddModelError("Email", "A customer with this email already exists.");
                return View(customerDto);
            }

            var customer = new Customer
            {
                Name = customerDto.Name,
                Email = customerDto.Email,
                CustomerType = customerDto.CustomerType
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created new customer: {CustomerName} ({CustomerType})", customer.Name, customer.CustomerType);

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Displays customer details
        /// </summary>
        public async Task<IActionResult> Details(int id)
        {
            var customer = await _context.Customers
                .Include(c => c.Orders)
                .ThenInclude(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        // API Endpoints

        /// <summary>
        /// GET: api/customers
        /// Returns a list of all customers
        /// </summary>
        [HttpGet("api/customers")]
        public async Task<IActionResult> GetCustomers()
        {
            var customers = await _context.Customers
                .Select(c => new CustomerDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Email = c.Email,
                    CustomerType = c.CustomerType
                })
                .ToListAsync();

            return Ok(customers);
        }

        /// <summary>
        /// GET: api/customers/{id}
        /// Returns a specific customer by ID
        /// </summary>
        [HttpGet("api/customers/{id}")]
        public async Task<IActionResult> GetCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);

            if (customer == null)
            {
                return NotFound(new { message = "Customer not found" });
            }

            var customerDto = new CustomerDto
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email,
                CustomerType = customer.CustomerType
            };

            return Ok(customerDto);
        }

        /// <summary>
        /// POST: api/customers
        /// Creates a new customer
        /// </summary>
        [HttpPost("api/customers")]
        public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerDto customerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if email already exists
            if (await _context.Customers.AnyAsync(c => c.Email == customerDto.Email))
            {
                return BadRequest(new { message = "A customer with this email already exists" });
            }

            var customer = new Customer
            {
                Name = customerDto.Name,
                Email = customerDto.Email,
                CustomerType = customerDto.CustomerType
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            var responseDto = new CustomerDto
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email,
                CustomerType = customer.CustomerType
            };

            _logger.LogInformation("Created new customer via API: {CustomerName} ({CustomerType})", customer.Name, customer.CustomerType);

            return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, responseDto);
        }

        /// <summary>
        /// PUT: api/customers/{id}
        /// Updates an existing customer
        /// </summary>
        [HttpPut("api/customers/{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] CreateCustomerDto customerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound(new { message = "Customer not found" });
            }

            // Check if email is being changed and if it already exists
            if (customer.Email != customerDto.Email &&
                await _context.Customers.AnyAsync(c => c.Email == customerDto.Email && c.Id != id))
            {
                return BadRequest(new { message = "A customer with this email already exists" });
            }

            customer.Name = customerDto.Name;
            customer.Email = customerDto.Email;
            customer.CustomerType = customerDto.CustomerType;

            await _context.SaveChangesAsync();

            var responseDto = new CustomerDto
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email,
                CustomerType = customer.CustomerType
            };

            return Ok(responseDto);
        }

        /// <summary>
        /// DELETE: api/customers/{id}
        /// Deletes a customer (only if they have no orders)
        /// </summary>
        [HttpDelete("api/customers/{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var customer = await _context.Customers
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null)
            {
                return NotFound(new { message = "Customer not found" });
            }

            // Check if customer has any orders
            if (customer.Orders.Any())
            {
                return BadRequest(new { message = "Cannot delete customer with existing orders" });
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
