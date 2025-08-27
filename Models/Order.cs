using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WholesaleRetailStore.Models
{
    /// <summary>
    /// Represents an order placed by a customer.
    /// Contains order items and calculates total amount automatically.
    /// </summary>
    public class Order
    {
        /// <summary>
        /// Unique identifier for the order
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Foreign key to the customer who placed this order
        /// </summary>
        [Required]
        public int CustomerId { get; set; }

        /// <summary>
        /// Navigation property to the customer who placed this order
        /// </summary>
        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }

        /// <summary>
        /// Date and time when the order was created
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Total amount of the order (calculated from order items)
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Status of the order
        /// </summary>
        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        /// <summary>
        /// Navigation property for items in this order
        /// </summary>
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }

    /// <summary>
    /// Enumeration for order statuses
    /// </summary>
    public enum OrderStatus
    {
        /// <summary>
        /// Order is pending processing
        /// </summary>
        Pending,

        /// <summary>
        /// Order has been confirmed and stock reduced
        /// </summary>
        Confirmed,

        /// <summary>
        /// Order has been shipped
        /// </summary>
        Shipped,

        /// <summary>
        /// Order has been cancelled
        /// </summary>
        Cancelled
    }
}
