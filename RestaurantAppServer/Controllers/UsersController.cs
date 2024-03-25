using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantAppServer.Data;
using RestaurantAppServer.Data.Models;

namespace RestaurantAppServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {

        private readonly AppDbContext _db;
        public UsersController(AppDbContext db) => _db = db;

        [HttpPost]
        public async Task<IActionResult> AddUserTest()
        {
            try
            {
                User user = new()
                {
                    FullName = "FullName Test",
                    Address = "Address Test",
                    Email = "test@gmail.com",
                    Password = "password",
                    Phone = "0677777777",
                    ImageId = 23
                };
                await _db.Users.AddAsync(user);
                await _db.SaveChangesAsync();
                return StatusCode(201, new { status = true, message = "Add with success", user });

            }
            catch (Exception err)
            {
                return StatusCode(500, new { status = false, message = "Internal Serveur Error", error = err.Message });
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSingleUser(int id)
        {
            try
            {
                var user = await _db.Users.Include(u => u.image).FirstOrDefaultAsync(u => u.Id == id);
                if (user == null)
                {
                    return NotFound(new { status = false, message = "User Not Found" });
                }
                return StatusCode(201, new { status = true, message = "Add with success", user });

            }
            catch (Exception err)
            {
                return StatusCode(500, new { status = false, message = "Internal Serveur Error", error = err.Message });
            }
        }

    }
}