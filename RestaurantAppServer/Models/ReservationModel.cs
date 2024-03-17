namespace RestaurantAppServer.Models
{
    public class ReservationModel
    {
        public DateTime Date { get; set; }
        public int NbrOfPeople { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Status { get; set; } = "Pending";
        public int TableId { get; set; }
    }
}
