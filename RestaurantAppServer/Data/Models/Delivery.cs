using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantAppServer.Data.Models
{
    public class Delivery
    {
        [Key]
        public int Id { get; set; }
        public string Status { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(deliveryMan))]
        public int deliveryManId { get; set; }
        public DeliveryMan deliveryMan { get; set; }

        [ForeignKey(nameof(order))]
        public int orderId { get; set; }
        public Order order { get; set; }

        
    }
}
