using Microsoft.EntityFrameworkCore;
using UserPortal.Entities;

namespace UserPortal.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options) { }

        public DbSet<User> Users { get; set; }
    }
}
