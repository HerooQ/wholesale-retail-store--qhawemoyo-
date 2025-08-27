using System.ComponentModel.DataAnnotations;

namespace WholesaleRetailStore.Models.DTOs
{
    /// <summary>
    /// Data Transfer Object for creating a new order
    /// </summary>
    public class CreateOrderDto
    {
        /// <summary>
        /// Customer ID placing the order
        /// </summary>
        [Required]
        public int CustomerId { get; set; }

        /// <summary>
        /// List of items to include in the order
        /// </summary>
        [Required]
        [MinLength(1)]
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
    }

    /// <summary>
    /// Data Transfer Object for order items
    /// </summary>
    public class OrderItemDto
    {
        /// <summary>
        /// Product ID to order
        /// </summary>
        [Required]
        public int ProductId { get; set; }

        /// <summary>
        /// Quantity of the product to order
        /// </summary>
        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }

    /// <summary>
    /// Data Transfer Object for order responses
    /// </summary>
    public class OrderDto
    {
        /// <summary>
        /// Unique identifier for the order
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Customer ID who placed this order
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
        /// Date and time when the order was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Total amount of the order
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Current status of the order
        /// </summary>
        public OrderStatus Status { get; set; }

        /// <summary>
        /// Items included in this order
        /// </summary>
        public List<OrderItemDetailDto> Items { get; set; } = new List<OrderItemDetailDto>();
    }

    /// <summary>
    /// Data Transfer Object for order item details in responses
    /// </summary>
    public class OrderItemDetailDto
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
        /// Quantity ordered
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Unit price at time of order
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Total price for this item (Quantity * UnitPrice)
        /// </summary>
        public decimal TotalPrice { get; set; }
    }
}
