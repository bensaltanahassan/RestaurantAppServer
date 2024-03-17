namespace RestaurantAppServer.Models
{
    public class OrderModel
    {
        public int UserId { get; set; }
        public double TotalPrice { get; set; }
        public string Adress { get; set; }
        public string PhoneNumber { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public string OrderStatus { get; set; }
    }
}
