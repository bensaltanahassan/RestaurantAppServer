using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantAppServer.Data.Models
{
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }
        public int Quantity { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(order))]
        public int OrderId { get; set; }
        public Order order { get; set; }

        [ForeignKey(nameof(product))]
        public int ProductId { get; set; }
        public Product product { get; set; }
    }
}
