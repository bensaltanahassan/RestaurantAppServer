using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantAppServer.Data.Models
{
    public class Favorite
    {
        [Key]
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


        [ForeignKey(nameof(user))]
        public int UserId { get; set; }
        public User user { get; set; }

        [ForeignKey(nameof(product))]
        public int ProductId { get; set; }
        public Product product { get; set; }
    }
}
