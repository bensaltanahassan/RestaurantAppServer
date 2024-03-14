using System.ComponentModel.DataAnnotations;

namespace RestaurantAppServer.Data.Models
{
    public class Table
    {
        [Key]
        public int Id { get; set; }
        public int TableNbr { get; set; }
        public string Status { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
