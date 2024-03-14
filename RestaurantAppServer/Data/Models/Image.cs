using System.ComponentModel.DataAnnotations;

namespace RestaurantAppServer.Data.Models
{
    public class Image
    {
        [Key]
        public int Id { get; set; }

        public string PublicId { get; set; }
        public string Url { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    }
}
