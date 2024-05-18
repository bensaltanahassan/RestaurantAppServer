using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantAppServer.Data;
using RestaurantAppServer.Data.Models;
using RestaurantAppServer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestaurantAppServer.Interfaces;
using RestaurantAppServer.Services;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Globalization;


namespace RestaurantAppServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IImageService _imageService;

        public ProductController(AppDbContext db, IImageService imageService)
        {
            _db = db;
            _imageService = imageService;
        }


        [HttpGet]
        public async Task<IActionResult> GetAllProductsInCategory([FromQuery] int? categoryId, [FromQuery] int page = 1, [FromQuery] int limit = 20)
        {
            try
            {
                if (page <= 0 || limit <= 0)
                    return BadRequest(new { status = false, message = "Invalid page or limit value" });

                IQueryable<Product> query = _db.Products.Where(p => p.IsAvailable).Include(p => p.Category).Include(p => p.ProductImages);

                if (categoryId != null)
                {
                    query = query.Where(p => p.CategoryId == categoryId);
                }

                int totalItems = await query.CountAsync();

                int offset = (page - 1) * limit;
                var products = await query.Skip(offset).Take(limit)
                .Select(p => new Product
                {
                    Id = p.Id,
                    Name = p.Name,
                    NameAn = p.NameAn,
                    Description = p.Description,
                    DescriptionAn = p.DescriptionAn,
                    Price = p.Price,
                    Discount = p.Discount,
                    NbrOfSales = p.NbrOfSales,
                    IsAvailable = p.IsAvailable,
                    CategoryId = p.CategoryId,
                    Category = new Category
                    {
                        Id = p.Category.Id,
                        Name = p.Category.Name,
                        NameAn = p.Category.NameAn
                    },
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    ProductImages = p.ProductImages.Select(pi => new ProductImages
                    {
                        Id = pi.Id,
                        ImageId = pi.ImageId,
                        image = new Image
                        {
                            Id = pi.image.Id,
                            PublicId = pi.image.PublicId,
                            Url = pi.image.Url
                        }
                    }).ToList()
                }).ToListAsync();

                return Ok(new { status = true, totalItems, currentPage = page, products });
            }
            catch (Exception err)
            {
                return StatusCode(500, new { status = false, message = "Internal Server Error", error = err.Message });
            }
        }


        [HttpGet]
        [Route("Promo")]
        public async Task<IActionResult> GetAllProductswithDiscount([FromQuery] int? categoryId, [FromQuery] int page = 1, [FromQuery] int limit = 20)
        {
            try
            {
                if (page <= 0 || limit <= 0)
                    return BadRequest(new { status = false, message = "Invalid page or limit value" });

                IQueryable<Product> query = _db.Products
                    .Where(p => p.IsAvailable && p.Discount > 0)
                    .Include(p => p.Category)
                    .Include(p => p.ProductImages);

                if (categoryId != null)
                {
                    query = query.Where(p => p.CategoryId == categoryId);
                }

                int totalItems = await query.CountAsync();

                int offset = (page - 1) * limit;
                var products = await query.Skip(offset).Take(limit)
                    .Select(p => new Product
                    {
                        Id = p.Id,
                        Name = p.Name,
                        NameAn = p.NameAn,
                        Description = p.Description,
                        DescriptionAn = p.DescriptionAn,
                        Price = p.Price,
                        Discount = p.Discount,
                        NbrOfSales = p.NbrOfSales,
                        IsAvailable = p.IsAvailable,
                        CategoryId = p.CategoryId,
                        Category = new Category
                        {
                            Id = p.Category.Id,
                            Name = p.Category.Name,
                            NameAn = p.Category.NameAn
                        },
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt,
                        ProductImages = p.ProductImages.Select(pi => new ProductImages
                        {
                            Id = pi.Id,
                            ImageId = pi.ImageId,
                            image = new Image
                            {
                                Id = pi.image.Id,
                                PublicId = pi.image.PublicId,
                                Url = pi.image.Url
                            }
                        }).ToList()
                    }).ToListAsync();

                return Ok(new { status = true, totalItems, currentPage = page, products });
            }
            catch (Exception err)
            {
                return StatusCode(500, new { status = false, message = "Internal Server Error", error = err.Message });
            }
        }


        [HttpPost]
        [Route("CreateItems")]
        public async Task<IActionResult> CreateProduct([FromForm] ProductModel pm, IFormFile file)
        {
            try
            {
                if (double.TryParse(pm.Price, NumberStyles.Float, CultureInfo.InvariantCulture, out double price) == false)
                {
                    return BadRequest(new { status = false, message = "Invalid price value" });
                }

                var result = await _imageService.AddImageAsync(file);
                if (result.Error != null)
                    return BadRequest(new { status = false, message = "Image upload failed" });

                ImageModel img = new()
                {
                    PublicId = result.PublicId,
                    Url = result.SecureUrl.AbsoluteUri
                };

                Image image = new()
                {
                    PublicId = img.PublicId,
                    Url = img.Url
                };

                await _db.Images.AddAsync(image);
                await _db.SaveChangesAsync();




                Product product = new()
                {
                    Name = pm.Name,
                    NameAn = pm.NameAn,
                    Description = pm.Description,
                    DescriptionAn = pm.DescriptionAn,
                    Price = price,
                    Discount = pm.Discount,
                    NbrOfSales = pm.NbrOfSales,
                    IsAvailable = pm.IsAvailable,
                    CategoryId = pm.CategoryId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    ProductImages = new List<ProductImages>
                    {
                        new ProductImages
                        {
                            ImageId = image.Id
                        }
                    }
                };

                await _db.Products.AddAsync(product);
                await _db.SaveChangesAsync();

                return Ok(new { status = true, message = "Product created successfully" });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { status = false, message = "Internal Server Error", error = e.Message, });
            }
        }

        [HttpPost]
        [Route("AddImages/{id}")]
        public async Task<IActionResult> AddImages(int id, IEnumerable<IFormFile> files)
        {
            try
            {
                var product = await _db.Products.FindAsync(id);
                if (product == null)
                {
                    return NotFound(new { status = false, message = "Product not found" });
                }

                List<Image> images = [];
                foreach (var file in files)
                {
                    var result = await _imageService.AddImageAsync(file);
                    if (result.Error != null)
                        return BadRequest(new { status = false, message = "Image upload failed" });

                    ImageModel img = new()
                    {
                        PublicId = result.PublicId,
                        Url = result.SecureUrl.AbsoluteUri
                    };

                    Image image = new()
                    {
                        PublicId = img.PublicId,
                        Url = img.Url
                    };
                    images.Add(image);
                }

                await _db.Images.AddRangeAsync(images);
                await _db.SaveChangesAsync();

                List<ProductImages> productImages = [];
                foreach (var img in images)
                {
                    productImages.Add(new ProductImages
                    {
                        ImageId = img.Id,
                        ProductId = product.Id,
                    });
                }

                await _db.ProductsImages.AddRangeAsync(productImages);
                await _db.SaveChangesAsync();

                return Ok(new { status = true, message = "Images added successfully" });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { status = false, message = "Internal Server Error", error = e.Message });
            }
        }



        [HttpPut]
        [Route("UpdateProduct/{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] ProductModel pm, IFormFile file)
        {
            try
            {
                var product = await _db.Products.FindAsync(id);
                if (product == null)
                {
                    return NotFound(new { status = false, message = "Product not found" });
                }

                if (double.TryParse(pm.Price, NumberStyles.Float, CultureInfo.InvariantCulture, out double price) == false)
                {
                    return BadRequest(new { status = false, message = "Invalid price value" });
                }

                product.Name = pm.Name;
                product.NameAn = pm.NameAn;
                product.Description = pm.Description;
                product.DescriptionAn = pm.DescriptionAn;
                product.Price = price; 
                product.Discount = pm.Discount;
                product.NbrOfSales = pm.NbrOfSales;
                product.IsAvailable = pm.IsAvailable;
                product.CategoryId = pm.CategoryId;
                product.UpdatedAt = DateTime.UtcNow;

                if (file != null)
                {
                    var result = await _imageService.AddImageAsync(file);
                    if (result.Error != null)
                        return BadRequest(new { status = false, message = "Image upload failed" });

                    ImageModel img = new()
                    {
                        PublicId = result.PublicId,
                        Url = result.SecureUrl.AbsoluteUri
                    };

                    Image image = new()
                    {
                        PublicId = img.PublicId,
                        Url = img.Url
                    };

                    await _db.Images.AddAsync(image);
                    await _db.SaveChangesAsync();

                    if (product.ProductImages.Any())
                    {
                        var oldImageId = product.ProductImages.First().ImageId;
                        var oldImage = await _db.Images.FindAsync(oldImageId);
                        _db.Images.Remove(oldImage);
                    }

                    product.ProductImages = new List<ProductImages>
            {
                new ProductImages
                {
                    ImageId = image.Id
                }
            };
                }

                _db.Products.Update(product);
                await _db.SaveChangesAsync();

                return Ok(new { status = true, message = "Product updated successfully" });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { status = false, message = "Internal Server Error", error = e.Message });
            }
        }




        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var product = await _db.Products.FindAsync(id);
                if (product == null)
                {
                    return BadRequest(new { status = false, message = "Product not found" });
                }

                _db.Products.Remove(product);
                await _db.SaveChangesAsync();

                return Ok(new { status = true, message = "Product deleted successfully" });
            }
            catch
            {
                return BadRequest(new { status = false, message = "Internal Server Error" });
            }
        }

        [HttpGet]
        [Route("SearchProduct")]
        public async Task<ActionResult<IEnumerable<Product>>> SearchProduct([FromQuery] string productName, [FromQuery] int? categoryId, [FromQuery] int page = 1, [FromQuery] int limit = 20)
        {
            try
            {
                if (string.IsNullOrEmpty(productName))
                {
                    return BadRequest(new { status = false, message = "Product name is required." });
                }

                if (page <= 0 || limit <= 0)
                    return BadRequest(new { status = false, message = "Invalid page or limit value" });
                int offset = (page - 1) * limit;

                var products = await _db.Products
                    .Where(p => (p.Name.Contains(productName) || p.NameAn.Contains(productName))
                                 && (categoryId == null || p.CategoryId == categoryId)
                                 && p.IsAvailable
                    )
                    .Include(p => p.Category)
                    .Include(p => p.ProductImages)
                        .ThenInclude(pi => pi.image)

                    .Skip(offset)
                    .Take(limit)
                    .Select(p => new Product
                    {
                        Id = p.Id,
                        Name = p.Name,
                        NameAn = p.NameAn,
                        Description = p.Description,
                        DescriptionAn = p.DescriptionAn,
                        Price = p.Price,
                        Discount = p.Discount,
                        NbrOfSales = p.NbrOfSales,
                        IsAvailable = p.IsAvailable,
                        CategoryId = p.CategoryId,
                        Category = new Category
                        {
                            Id = p.Category.Id,
                            Name = p.Category.Name,
                            NameAn = p.Category.NameAn
                        },
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt,
                        ProductImages = p.ProductImages.Select(pi => new ProductImages
                        {
                            Id = pi.Id,
                            image = pi.image
                        }).ToList()
                    })
                    .ToListAsync();


                return Ok(new { status = true, products });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = false, message = ex.Message });
            }
        }



    }
}
