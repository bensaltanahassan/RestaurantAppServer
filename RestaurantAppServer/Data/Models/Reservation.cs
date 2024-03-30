using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
#nullable enable

namespace RestaurantAppServer.Data.Models
{
    public class Reservation
    {
        [Key]
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int NbrOfPeople { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Status { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(table))]
        public int? TableId { get; set; }
        public Table? table { get; set; }
    }
}
