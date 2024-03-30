using System.ComponentModel.DataAnnotations;
#nullable enable


namespace RestaurantAppServer.Models
{
    public class UpdateUserModel
    {
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }


        public IFormFile? File { get; set; }

    }
}
