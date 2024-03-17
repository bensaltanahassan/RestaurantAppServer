using RestaurantAppServer.Data.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RestaurantAppServer.Models
{
    public class OrderItemModel
    {
        public int Quantity { get; set; }
        public int ProductId { get; set; }
        public int UserId { get; set; }
    }
}
