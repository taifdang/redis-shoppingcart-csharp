using Microsoft.EntityFrameworkCore;
using Server.API.Models;

namespace Server.API.Data
{
    public class DatabaseContext:DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }
        public DbSet<Products> products { get; set; }
    }
}
