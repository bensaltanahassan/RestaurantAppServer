using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantAppServer.Data.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
        public string NameAn { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(image))]
        public int? ImageId { get; set; }
        public Image image { get; set; }


    }
}
