using Epsilon.Models;
using Microsoft.EntityFrameworkCore;

namespace Epsilon.Data
{
    public class EpsilonDbContext : DbContext
    {
        public EpsilonDbContext(DbContextOptions<EpsilonDbContext> options) : base(options) { }
        
        public DbSet<Message> Messages { get; set; }
        public DbSet<User> Users { get; set; }
    }
}