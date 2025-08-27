using WholesaleRetailStore.Data;
using WholesaleRetailStore.Models;

namespace WholesaleRetailStore.Services
{
    /// <summary>
    /// Service for handling pricing calculations and business rules.
    /// Automatically applies discounts based on customer type and pricing rules.
    /// </summary>
    public class PricingService
    {
        private readonly AppDbContext _context;

        public PricingService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Calculates the discounted price for a product based on customer type.
        /// Applies the best available pricing rule for the customer type.
        /// </summary>
        /// <param name="product">The product to calculate price for</param>
        /// <param name="customerType">The type of customer (Retail or Wholesale)</param>
        /// <param name="quantity">Quantity being purchased (for future extensibility)</param>
        /// <returns>The calculated price after applying discounts</returns>
        public decimal CalculatePrice(Product product, CustomerType customerType, int quantity = 1)
        {
            // For retail customers, return base price (no discount)
            if (customerType == CustomerType.Retail)
            {
                return product.BasePrice;
            }

            // For wholesale customers, find and apply the best pricing rule
            var pricingRule = GetBestPricingRule(customerType, product.BasePrice * quantity);

            if (pricingRule != null)
            {
                // Apply discount: BasePrice * (1 - DiscountPercentage/100)
                return product.BasePrice * pricingRule.DiscountFactor;
            }

            // If no pricing rule found, return base price
            return product.BasePrice;
        }

        /// <summary>
        /// Generates a quote for multiple products for a specific customer.
        /// Returns detailed pricing breakdown including discounts applied.
        /// </summary>
        /// <param name="customer">The customer requesting the quote</param>
        /// <param name="productQuantities">Dictionary of product IDs and quantities</param>
        /// <returns>Detailed quote with pricing breakdown</returns>
        public Quote GenerateQuote(Customer customer, Dictionary<int, int> productQuantities)
        {
            var quote = new Quote
            {
                CustomerId = customer.Id,
                CustomerName = customer.Name,
                CustomerType = customer.CustomerType,
                Items = new List<QuoteItem>(),
                GeneratedAt = DateTime.UtcNow
            };

            decimal subtotal = 0;

            foreach (var (productId, quantity) in productQuantities)
            {
                var product = _context.Products.Find(productId);
                if (product == null || product.Stock < quantity)
                {
                    throw new InvalidOperationException($"Product {productId} not found or insufficient stock");
                }

                var basePrice = product.BasePrice;
                var discountedPrice = CalculatePrice(product, customer.CustomerType, quantity);
                var discountAmount = (basePrice - discountedPrice) * quantity;
                var lineTotal = discountedPrice * quantity;

                quote.Items.Add(new QuoteItem
                {
                    ProductId = productId,
                    ProductName = product.Name,
                    Quantity = quantity,
                    BasePrice = basePrice,
                    DiscountedPrice = discountedPrice,
                    DiscountAmount = discountAmount,
                    LineTotal = lineTotal
                });

                subtotal += lineTotal;
            }

            quote.Subtotal = subtotal;

            // Apply any additional pricing rules based on total order amount
            var orderLevelRule = GetBestPricingRule(customer.CustomerType, subtotal);
            if (orderLevelRule != null && orderLevelRule.MinimumOrderAmount.HasValue &&
                subtotal >= orderLevelRule.MinimumOrderAmount.Value)
            {
                // Apply additional discount on total
                var additionalDiscount = subtotal * (orderLevelRule.DiscountPercentage / 100);
                quote.AdditionalDiscount = additionalDiscount;
                quote.Total = subtotal - additionalDiscount;
            }
            else
            {
                quote.Total = subtotal;
            }

            return quote;
        }

        /// <summary>
        /// Gets the best pricing rule for a customer type and order amount.
        /// Returns the rule with the highest discount percentage that applies.
        /// </summary>
        /// <param name="customerType">The customer type to find rules for</param>
        /// <param name="orderAmount">The total order amount (for minimum order requirements)</param>
        /// <returns>The best applicable pricing rule, or null if none found</returns>
        private PricingRule? GetBestPricingRule(CustomerType customerType, decimal orderAmount)
        {
            var applicableRules = _context.PricingRules
                .Where(r => r.CustomerType == customerType &&
                           r.IsActive &&
                           (!r.MinimumOrderAmount.HasValue || orderAmount >= r.MinimumOrderAmount.Value))
                .OrderByDescending(r => r.DiscountPercentage)
                .ToList();

            return applicableRules.FirstOrDefault();
        }

        /// <summary>
        /// Validates if an order can be placed based on stock availability
        /// </summary>
        /// <param name="productQuantities">Dictionary of product IDs and requested quantities</param>
        /// <returns>True if all products have sufficient stock</returns>
        public bool ValidateStockAvailability(Dictionary<int, int> productQuantities)
        {
            foreach (var (productId, quantity) in productQuantities)
            {
                var product = _context.Products.Find(productId);
                if (product == null || product.Stock < quantity)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Reduces stock levels for products in an order
        /// </summary>
        /// <param name="productQuantities">Dictionary of product IDs and quantities to reduce</param>
        public void ReduceStock(Dictionary<int, int> productQuantities)
        {
            foreach (var (productId, quantity) in productQuantities)
            {
                var product = _context.Products.Find(productId);
                if (product != null)
                {
                    product.Stock -= quantity;
                }
            }
            _context.SaveChanges();
        }
    }

    /// <summary>
    /// Represents a quote for a customer with detailed pricing breakdown
    /// </summary>
    public class Quote
    {
        /// <summary>
        /// Customer ID this quote is for
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// Customer name
        /// </summary>
        public string CustomerName { get; set; } = string.Empty;

        /// <summary>
        /// Customer type (affects pricing)
        /// </summary>
        public CustomerType CustomerType { get; set; }

        /// <summary>
        /// When this quote was generated
        /// </summary>
        public DateTime GeneratedAt { get; set; }

        /// <summary>
        /// Individual quote items with pricing details
        /// </summary>
        public List<QuoteItem> Items { get; set; } = new List<QuoteItem>();

        /// <summary>
        /// Subtotal before additional discounts
        /// </summary>
        public decimal Subtotal { get; set; }

        /// <summary>
        /// Additional discount applied to the order total
        /// </summary>
        public decimal AdditionalDiscount { get; set; }

        /// <summary>
        /// Final total after all discounts
        /// </summary>
        public decimal Total { get; set; }
    }

    /// <summary>
    /// Represents an individual item in a quote with pricing details
    /// </summary>
    public class QuoteItem
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
        /// Quantity requested
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Base price per unit before discounts
        /// </summary>
        public decimal BasePrice { get; set; }

        /// <summary>
        /// Price per unit after applying customer discounts
        /// </summary>
        public decimal DiscountedPrice { get; set; }

        /// <summary>
        /// Total discount amount for this line item
        /// </summary>
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Total price for this line item (DiscountedPrice * Quantity)
        /// </summary>
        public decimal LineTotal { get; set; }
    }
}
