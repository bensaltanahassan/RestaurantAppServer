using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestaurantAppServer.Data;
using RestaurantAppServer.Data.Models;
using RestaurantAppServer.Models;

namespace RestaurantAppServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationController : ControllerBase
    {
        private readonly AppDbContext _db;
        public ReservationController(AppDbContext db)
        {
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> AddReservation([FromBody] ReservationModel rm) {
            try {
                Reservation rv = new()
                {
                    FirstName = rm.FirstName,
                    LastName = rm.LastName,
                    Phone = rm.Phone,
                    Date = rm.Date,
                    Status = rm.Status,
                    NbrOfPeople = rm.NbrOfPeople,
                    TableId = rm.TableId,
                };
                await _db.Reservations.AddAsync(rv);
                await _db.SaveChangesAsync();

                return Ok(new { status = true, message = "Reservation Added with success" });

            }
            catch (Exception ex)
            {
                return BadRequest(new {status=false,message="Internal Serveur Error"});
            }
        }
    }
}
