using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantAppServer.Data;
using RestaurantAppServer.Data.Models;
using RestaurantAppServer.Interfaces;
using RestaurantAppServer.Models;
using RestaurantAppServer.Models.auth;
using RestaurantAppServer.Services;

namespace RestaurantAppServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IImageService _imageService;
        public UserController(AppDbContext db, IImageService imageService)
        {
            _db = db;
            _imageService = imageService;
        }
        [Authorize(Roles = "admin")]
        [HttpGet("getAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                //TODO: add eager loading
                var users = await _db.Users.Include(u => u.image).ToListAsync();
                var usersList = new List<UserModel>();
                foreach (var user in users)
                {
                    usersList.Add(new UserModel
                    {
                        Id = user.Id,
                        FullName = user.FullName,
                        Email = user.Email,
                        Phone = user.Phone,
                        Address = user.Address,
                        ImageUrl = user.image?.Url,
                        IsVerified = user.IsVerified,
                        CreatedAt = user.CreatedAt,
                        UpdatedAt = user.UpdatedAt
                    });
                }

                return Ok(new { status = "Success", usersList });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { status = false, message = "Couldn't search the users", err = e.Message });
            }
        }
        [Authorize(Roles = "admin")]
        [HttpGet("getUserById")]
        public async Task<IActionResult> GetUserById(int Id)
        {
            try
            {
                var userFromDB = await _db.Users.Include(u => u.image).FirstOrDefaultAsync(u => u.Id == Id);
                if (userFromDB != null)
                {
                    var user = new UserModel
                    {
                        Id = userFromDB.Id,
                        FullName = userFromDB.FullName,
                        Email = userFromDB.Email,
                        Phone = userFromDB.Phone,
                        Address = userFromDB.Address,
                        ImageUrl = userFromDB.image?.Url,
                        IsVerified = userFromDB.IsVerified,
                        CreatedAt = userFromDB.CreatedAt,
                        UpdatedAt = userFromDB.UpdatedAt
                    };
                    return Ok(new { status = "Success", user });
                }
                return NotFound(new { status = false, message = "User not found" });

            }
            catch (Exception e)
            {
                return StatusCode(500, new { status = false, message = "Couldn't search the users", err = e.Message });
            }
        }
        [Authorize(Roles = "user")]
        [HttpPut("updateUser/{Id}")]
        public async Task<IActionResult> UpdateUser(int Id, [FromForm] UpdateUserModel um)
        {
            try
            {
                var user = await _db.Users.Include(u => u.image).FirstOrDefaultAsync(u => u.Id == Id);
                if (user == null)
                    return NotFound(new { status = false, message = "User not found" });
                if (um.File != null)
                {
                    int? oldImgId;
                    if (user.image != null)
                    {
                        oldImgId = user.image.Id;
                    }
                    else
                    {
                        oldImgId = null;
                    }
                    var result = await _imageService.AddImageAsync(um.File);
                    if (result.Error != null)
                        return BadRequest(new { status = false, message = "Image upload failed" });
                    Image image = new()
                    {
                        PublicId = result.PublicId,
                        Url = result.SecureUrl.AbsoluteUri
                    };
                    user.image = image;
                    await _db.Images.AddAsync(image);
                    if (oldImgId != null)
                    {
                        var oldImage = await _db.Images.FirstOrDefaultAsync(i => i.Id == oldImgId);
                        if (oldImage != null)
                        {
                            // Delete image from cloud
                            var imageResult = await _imageService.DeleteImageAsync(oldImage.PublicId);
                            if (imageResult.Error != null)
                                return BadRequest(new { status = false, message = "Failed to delete old image" });
                            _db.Images.Remove(oldImage);
                        }
                    }
                    await _db.SaveChangesAsync();
                }
                user.FullName = um.FullName ?? user.FullName;
                user.Phone = um.Phone ?? user.Phone;
                user.Address = um.Address ?? user.Address;
                if(um.Password != null)
                {
                    user.Password = BCrypt.Net.BCrypt.HashPassword(um.Password);
                }
                user.UpdatedAt = DateTime.Now;
                _db.Users.Update(user);
                await _db.SaveChangesAsync();
                return StatusCode(StatusCodes.Status200OK, new
                {
                    Status = true,
                    Message = "User Updated successfully",
                    user = new
                    {
                        user.Id,
                        user.FullName,
                        user.Email,
                        user.Phone,
                        user.Address,
                        user.CreatedAt,
                        user.UpdatedAt,
                        user.image?.Url
                    }
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { status = false, message = "Internal Server Error", error = e.Message });
            }
        }
        [Authorize(Roles = "user")]
        [HttpDelete("deleteUser/{Id}")]
        public async Task<IActionResult> DeleteUser(int Id)
        {
            try
            {
                var user = await _db.Users.Include(u => u.image).FirstOrDefaultAsync(u => u.Id == Id);
                if (user == null)
                    return NotFound(new { status = false, message = "User not found" });
                if (user.ImageId != null)
                {
                    // Delete image from cloud
                    var imageResult = await _imageService.DeleteImageAsync(user.image.PublicId);
                    if (imageResult.Error != null)
                        return BadRequest(new { status = false, message = "Failed to delete old image" });
                    _db.Images.Remove(user.image);
                }
                _db.Users.Remove(user);
                await _db.SaveChangesAsync();
                return StatusCode(StatusCodes.Status200OK, new Response { Status = true, Message = "User Deleted successfully" });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { status = false, message = "Internal Server Error", err = e.Message });
            }
        }

    }


}
