using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantAppServer.Data.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string NameAn { get; set; }
        public string Description { get; set; }
        public string DescriptionAn { get; set; }
        public double Price { get; set; }
        public int discount { get; set; }
        public int NbrOfSales { get; set; }
        public bool isAvailable { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(category))]
        public int CategoryId { get; set; }
        public Category category { get; set; }

        public List<ProductImages>? ProductImages { get; set; }
        public Product()
        {
            ProductImages = new List<ProductImages>();
        }
    }
}
