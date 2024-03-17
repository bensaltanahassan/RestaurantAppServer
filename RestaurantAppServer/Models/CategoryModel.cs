using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RestaurantAppServer.Models
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryModel : ControllerBase
    {
        public string Name { get; set; }
        public string NameAn { get; set; }
        public int? ImageId { get; set; }
    }
}
