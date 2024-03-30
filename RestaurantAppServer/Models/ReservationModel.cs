namespace RestaurantAppServer.Models
{
    public class ReservationModel
    {
        public DateTime Date { get; set; }
        public int NbrOfPeople { get; set; }
        public string fullName { get; set; }
        public string Phone { get; set; }
        public string Status { get; set; } = "Pending";
    }
}
