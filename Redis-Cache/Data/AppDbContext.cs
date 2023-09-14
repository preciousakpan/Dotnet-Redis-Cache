using Microsoft.EntityFrameworkCore;
using Redis_Cache.Models;

namespace Redis_Cache.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Driver> Drivers { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options):
            base(options)
        {
            
        }
        
    }
}