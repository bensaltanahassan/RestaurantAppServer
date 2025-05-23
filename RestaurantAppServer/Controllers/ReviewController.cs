﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestaurantAppServer.Data.Models;
using RestaurantAppServer.Data;
using Microsoft.EntityFrameworkCore;
using RestaurantAppServer.Models;

namespace RestaurantAppServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ReviewController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Review>>> GetReviews()
        {

            var reviews = await _db.Reviews
                .OrderByDescending(r => r.CreatedAt)
                .Include(r => r.user)
                .Include(r => r.user.image)
                .ToListAsync();

            return Ok(new { status = true, reviews });
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<IEnumerable<Review>>> GetReviewsByUserId(int userId)
        {
            var reviews = await _db.Reviews
                .OrderByDescending(r => r.CreatedAt)
                .Include(r => r.user)
                .Where(r => r.UserId == userId)
                .Select(r => new
                {
                    r.Id,
                    r.Comment,
                    r.CreatedAt,
                    r.UpdatedAt,
                    user = new
                    {
                        r.user.Id,
                        r.user.FullName,
                        r.user.Email,
                        r.user.Phone,
                        r.user.Address,
                        image = new
                        {
                            r.user.image.Id,
                            r.user.image.Url
                        }
                    }
                })
                .ToListAsync();
            return Ok(new { status = true, reviews });
        }

        [HttpPost]
        public async Task<IActionResult> CreateReview([FromForm] ReviewModel reviewModel)
        {
            try
            {
                var review = new Review
                {
                    UserId = reviewModel.UserId,
                    Comment = reviewModel.Comment,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _db.Reviews.Add(review);
                await _db.SaveChangesAsync();

                return Ok(new { status = true, message = "Review created successfully" });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { status = false, message = "Internal Server Error", error = e.Message });
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            try
            {
                var review = await _db.Reviews.FindAsync(id);

                if (review == null)
                {
                    return NotFound("Review not found.");
                }

                _db.Reviews.Remove(review);
                await _db.SaveChangesAsync();

                return Ok(new { status = true, message = "Review deleted successfully" });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { status = false, message = "Internal Server Error", error = e.Message });
            }
        }

    }
}

