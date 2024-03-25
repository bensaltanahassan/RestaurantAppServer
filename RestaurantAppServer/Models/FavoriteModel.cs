using System.ComponentModel.DataAnnotations;

namespace RestaurantAppServer.Models
{
    public class FavoriteModel
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int ProductId { get; set; }
    }
}
