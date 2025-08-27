using System.ComponentModel.DataAnnotations;

namespace WholesaleRetailStore.Models
{
    /// <summary>
    /// Represents a customer in the wholesale/retail store system.
    /// Customers can be either Retail or Wholesale type, which affects pricing.
    /// </summary>
    public class Customer
    {
        /// <summary>
        /// Unique identifier for the customer
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Customer's full name
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Customer's email address
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Type of customer (Retail or Wholesale) - determines pricing rules
        /// </summary>
        [Required]
        public CustomerType CustomerType { get; set; }

        /// <summary>
        /// Navigation property for orders placed by this customer
        /// </summary>
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }

    /// <summary>
    /// Enumeration for customer types that determine pricing rules
    /// </summary>
    public enum CustomerType
    {
        /// <summary>
        /// Retail customers pay full base price
        /// </summary>
        Retail,

        /// <summary>
        /// Wholesale customers receive discounts based on pricing rules
        /// </summary>
        Wholesale
    }
}
