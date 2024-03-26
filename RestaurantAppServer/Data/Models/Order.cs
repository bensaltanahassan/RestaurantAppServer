using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace RestaurantAppServer.Data.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }
        public double TotalPrice { get; set; }
        public string Adress { get; set; }
        public string PhoneNumber { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public string OrderStatus { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(user))]
        public int UserId { get; set; }
        public User user { get; set; }


        [JsonProperty("ProductImages")]
        public List<OrderItem> orderItems { get; set; }
    }
}
