using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WholesaleRetailStore.Models
{
    /// <summary>
    /// Represents a pricing rule that defines discounts for different customer types.
    /// Used to automatically calculate prices based on customer type.
    /// </summary>
    public class PricingRule
    {
        /// <summary>
        /// Unique identifier for the pricing rule
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Customer type this pricing rule applies to
        /// </summary>
        [Required]
        public CustomerType CustomerType { get; set; }

        /// <summary>
        /// Discount percentage to apply (0-100).
        /// For example: 10 means 10% discount, 0 means no discount.
        /// </summary>
        [Required]
        [Range(0, 100)]
        public decimal DiscountPercentage { get; set; }

        /// <summary>
        /// Optional minimum order amount required for this discount to apply
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MinimumOrderAmount { get; set; }

        /// <summary>
        /// Whether this pricing rule is currently active
        /// </summary>
        [Required]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Description of this pricing rule for administrative purposes
        /// </summary>
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Calculated discount factor (1 - DiscountPercentage/100)
        /// Used for multiplying base price to get discounted price
        /// </summary>
        [NotMapped]
        public decimal DiscountFactor => 1 - (DiscountPercentage / 100);
    }
}
