using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace RestaurantAppServer.Data.Models
{
    public class Product
    {
        [Key]
        [JsonProperty("Id")]
        public int Id { get; set; }

        [Required]
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("NameAn")]
        public string NameAn { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }

        [JsonProperty("DescriptionAn")]
        public string DescriptionAn { get; set; }

        [JsonProperty("Price")]
        public double Price { get; set; }

        [JsonProperty("Discount")]
        public int Discount { get; set; }

        [JsonProperty("NbrOfSales")]
        public int NbrOfSales { get; set; }

        [JsonProperty("IsAvailable")]
        public bool IsAvailable { get; set; }

        [JsonProperty("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonProperty("UpdatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(Category))]
        [JsonProperty("CategoryId")]
        public int CategoryId { get; set; }

        [JsonProperty("Category")]
        public Category Category { get; set; }

        [JsonProperty("CategoryNameAn")]
        public string CategoryNameAn { get; set; }

        [JsonProperty("ProductImages")]
        public List<ProductImages> ProductImages { get; set; }

        public Product()
        {
            ProductImages = new List<ProductImages>();
        }
    }
}
