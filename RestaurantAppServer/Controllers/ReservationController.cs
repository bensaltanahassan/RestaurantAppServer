using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantAppServer.Data;
using RestaurantAppServer.Data.Models;
using RestaurantAppServer.helpers.enums;
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

        [HttpGet]
        public async Task<IActionResult> GetAllReservations([FromQuery] int page = 1, [FromQuery] int limit = 30, [FromQuery] string status = "all")
        {
            try
            {
                if (page <= 0 || limit <= 0) return BadRequest(new { status = false, message = "Invalid page or limit value" });
                IQueryable<Reservation> query = _db.Reservations.Include(r => r.table);
                if (status.ToLower() != "all")
                    query = query.Where(r => r.Status.ToLower() == status.ToLower());

                int totalItems = await query.CountAsync();


                int offset = (page - 1) * limit;
                var reservations = await query.Skip(offset).Take(limit).ToListAsync();

                return Ok(new { status = true, totalItems, currentPage = page, reservations });

            }
            catch (Exception err)
            {
                return StatusCode(500, new { status = false, message = "Internal Serveur Error", error = err.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddReservation([FromBody] ReservationModel rm)
        {
            try
            {
                Reservation rv = new()
                {
                    FirstName = rm.FirstName,
                    LastName = rm.LastName,
                    Phone = rm.Phone,
                    Date = rm.Date,
                    Status = ReservationStatus.Pending.ToString(),
                    NbrOfPeople = rm.NbrOfPeople,
                    TableId = rm.TableId,
                };
                await _db.Reservations.AddAsync(rv);
                await _db.SaveChangesAsync();

                return Ok(new { status = true, message = "Reservation Added with success" });

            }
            catch (Exception err)
            {
                return StatusCode(500, new { status = false, message = "Internal Serveur Error", error = err.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReservationStatus(int id, [FromQuery] string status)
        {
            try
            {
                var rv = await _db.Reservations.FindAsync(id);
                if (rv == null) return StatusCode(404, new { status = false, message = "Reservation Not Found" });
                rv.Status = status switch
                {
                    "confirm" => ReservationStatus.Confirmed.ToString(),
                    "cancel" => ReservationStatus.Canceled.ToString(),
                    "check-in" => ReservationStatus.CheckedIn.ToString(),
                    "check-out" => ReservationStatus.CheckedOut.ToString(),
                    "no-show" => ReservationStatus.NoShow.ToString(),
                    _ => ReservationStatus.Pending.ToString(),
                };
                _db.Reservations.Update(rv);
                await _db.SaveChangesAsync();
                return Ok(new { status = true, message = $"Reservation {status}ed With Success" });
            }
            catch (Exception err)
            {
                return StatusCode(500, new { status = false, message = "Internal Serveur Error", error = err.Message });
            }
        }



    }
}
