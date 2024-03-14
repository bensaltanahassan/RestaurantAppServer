using System.ComponentModel.DataAnnotations;

namespace RestaurantAppServer.Data.Models
{
    public class Admin
    {
        [Key]
        public int Id { get; set; }


        public string Email { get; set; }
        public string Password { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    }
}
