using Microsoft.EntityFrameworkCore;


namespace RestaurantAppServer.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        { }
        //public DbSet<Item> Items { get; set; }
    }
}
