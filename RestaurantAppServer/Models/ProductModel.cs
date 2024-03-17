namespace RestaurantAppServer.Models
{
    public class ProductModel
    {
        public string Name { get; set; }
        public string NameAn { get; set; }
        public string Description { get; set; }
        public string DescriptionAn { get; set; }
        public double Price { get; set; }
        public int Discount { get; set; }
        public bool IsAvailable { get; set; }
        public int CategoryId { get; set; }
        public int NbrOfSales { get; set; }
    }
}
