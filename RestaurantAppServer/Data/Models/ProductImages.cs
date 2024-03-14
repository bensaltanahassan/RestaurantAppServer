using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantAppServer.Data.Models
{
    public class ProductImages
    {
        [Key]
        public int Id { get; set; }
        public bool isMain { get; set; }=false;


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(product))]
        public int ProductId { get; set; }
        public Product product { get; set; }

        [ForeignKey(nameof(image))]
        public int ImageId { get; set; }
        public Image image { get; set; }
    }
}
