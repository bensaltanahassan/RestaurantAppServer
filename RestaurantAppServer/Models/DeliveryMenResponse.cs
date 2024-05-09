namespace RestaurantAppServer.Models
{
    public class DeliveryMenResponse
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Status { get; set; }
        public int OrdersOnShipping { get; set; }
        public int OrdersDelivered { get; set; }

    }
}
