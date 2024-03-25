using System.ComponentModel.DataAnnotations;

namespace RestaurantAppServer.Models
{
    public class UpdateUserModel
    {
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public ImageModel? Image { get; set; }
    }
}
