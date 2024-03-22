using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantAppServer.Data;
using RestaurantAppServer.Data.Models;
using RestaurantAppServer.helpers.enums;
using RestaurantAppServer.Models;

namespace RestaurantAppServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TableController : ControllerBase
    {
        private readonly AppDbContext _db;
        public TableController(AppDbContext db)
        {
            _db = db;
        }


        [HttpGet]
        public async Task<IActionResult> GetAllTables([FromQuery] int page = 1, [FromQuery] int limit = 30)
        {
            try
            {
                if (page <= 0 || limit <= 0) return BadRequest(new { status = false, message = "Invalid page or limit value" });
                int totalItems = await _db.Tables.CountAsync();
                int offset = (page - 1) * limit;
                var tables = await _db.Tables.Skip(offset).Take(limit).ToListAsync();
                return Ok(new { status = true, totalItems, currentPage = page, tables });
            }
            catch (Exception err)
            {
                return StatusCode(500, new { status = false, message = "Internal Serveur Error", error = err.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddTable([FromBody] TableModel table)
        {
            try
            {
                Table tb = new()
                {
                    TableNbr = table.TableNbr,
                    Status = table.Status
                };
                await _db.Tables.AddAsync(tb);
                await _db.SaveChangesAsync();
                return StatusCode(201, new { status = true, message = "Table Added with success", table });

            }
            catch (Exception err)
            {
                return StatusCode(500, new { status = false, message = "Internal Serveur Error", error = err.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTableStatus(int id, [FromQuery] string status)
        {
            try
            {
                var table = await _db.Tables.FindAsync(id);
                if (table == null) return NotFound(new { status = false, message = "Table Not Found" });
                table.Status = status switch
                {
                    "available" => TableStatus.Available.ToString(),
                    "reserved" => TableStatus.Reserved.ToString(),
                    _ => TableStatus.Available.ToString(),
                };
                _db.Tables.Update(table);
                await _db.SaveChangesAsync();
                return Ok(new { status = true, message = "Table Updated With Success", table });
            }
            catch (Exception err)
            {
                return StatusCode(500, new { status = false, message = "Internal Serveur Error", error = err.Message });
            }
        }


    }
}