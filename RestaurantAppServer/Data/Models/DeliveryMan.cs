﻿using System.ComponentModel.DataAnnotations;

namespace RestaurantAppServer.Data.Models
{
    public class DeliveryMan
    {
        [Key]
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Status { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
