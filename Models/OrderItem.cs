using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WholesaleRetailStore.Models
{
    /// <summary>
    /// Represents an individual item within an order.
    /// Contains product details, quantity, and calculated pricing at time of order.
    /// </summary>
    public class OrderItem
    {
        /// <summary>
        /// Unique identifier for the order item
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Foreign key to the order this item belongs to
        /// </summary>
        [Required]
        public int OrderId { get; set; }

        /// <summary>
        /// Navigation property to the order this item belongs to
        /// </summary>
        [ForeignKey("OrderId")]
        public Order? Order { get; set; }

        /// <summary>
        /// Foreign key to the product being ordered
        /// </summary>
        [Required]
        public int ProductId { get; set; }

        /// <summary>
        /// Navigation property to the product being ordered
        /// </summary>
        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        /// <summary>
        /// Quantity of the product ordered
        /// </summary>
        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        /// <summary>
        /// Unit price for this item at the time of order (after applying customer discounts)
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Total price for this order item (Quantity * UnitPrice)
        /// </summary>
        [NotMapped]
        public decimal TotalPrice => Quantity * UnitPrice;
    }
}
