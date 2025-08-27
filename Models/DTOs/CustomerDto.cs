using System.ComponentModel.DataAnnotations;

namespace WholesaleRetailStore.Models.DTOs
{
    /// <summary>
    /// Data Transfer Object for creating a new customer
    /// </summary>
    public class CreateCustomerDto
    {
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
        /// Type of customer (Retail or Wholesale)
        /// </summary>
        [Required]
        public CustomerType CustomerType { get; set; }
    }

    /// <summary>
    /// Data Transfer Object for customer responses
    /// </summary>
    public class CustomerDto
    {
        /// <summary>
        /// Unique identifier for the customer
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Customer's full name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Customer's email address
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Type of customer (Retail or Wholesale)
        /// </summary>
        public CustomerType CustomerType { get; set; }
    }
}
