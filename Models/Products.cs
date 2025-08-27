using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WholesaleRetailStore.Models
{
    /// <summary>
    /// Represents a product in the wholesale/retail store inventory.
    /// Contains basic product information and stock levels.
    /// </summary>
    public class Product
    {
        /// <summary>
        /// Unique identifier for the product
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Product name or title
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Detailed description of the product
        /// </summary>
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Current stock quantity available for sale
        /// </summary>
        [Required]
        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        /// <summary>
        /// Base retail price before any discounts are applied
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue)]
        public decimal BasePrice { get; set; }

        /// <summary>
        /// Navigation property for order items that reference this product
        /// </summary>
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
