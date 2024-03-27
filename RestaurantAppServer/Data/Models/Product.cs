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
        [JsonProperty(nameof(Id))]
        public int Id { get; set; }

        [Required]
        [JsonProperty(nameof(Name))]
        public string Name { get; set; }

        [JsonProperty(nameof(NameAn))]
        public string NameAn { get; set; }

        [JsonProperty(nameof(Description))]
        public string Description { get; set; }

        [JsonProperty(nameof(DescriptionAn))]
        public string DescriptionAn { get; set; }

        [JsonProperty(nameof(Price))]
        public double Price { get; set; }

        [JsonProperty(nameof(Discount))]
        public int Discount { get; set; }

        [JsonProperty(nameof(NbrOfSales))]
        public int NbrOfSales { get; set; }

        [JsonProperty(nameof(IsAvailable))]
        public bool IsAvailable { get; set; }

        [JsonProperty(nameof(CreatedAt))]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonProperty(nameof(UpdatedAt))]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(Category))]
        [JsonProperty(nameof(CategoryId))]
        public int CategoryId { get; set; }

        [JsonProperty(nameof(Category))]
        public Category Category { get; set; }

        [JsonProperty(nameof(ProductImages))]
        public List<ProductImages> ProductImages { get; set; }

        public Product()
        {
            ProductImages = new List<ProductImages>();
        }
    }
}
