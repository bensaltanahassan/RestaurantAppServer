using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantAppServer.Models
{
    public class ConfirmOrderModel
    {
        public int OrderId { get; set; }
        public int DeliveryManId { get; set; }
    }
}