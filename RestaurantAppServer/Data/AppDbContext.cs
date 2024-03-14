using Microsoft.EntityFrameworkCore;
using RestaurantAppServer.Data.Models;


namespace RestaurantAppServer.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        { }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<DeliveryMan> DeliveryMen { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<Table> Tables { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Delivery> Delivries { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImages> ProductsImages { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Reservation> Reservations { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProductImages>()
                .HasOne(pi => pi.product)
                .WithMany(p => p.ProductImages)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductImages>()
                .HasOne(pi => pi.image)
                .WithMany()
                .HasForeignKey(pi => pi.ImageId)
                .OnDelete(DeleteBehavior.Restrict);
        }

    }
}
