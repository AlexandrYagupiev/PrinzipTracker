using Microsoft.EntityFrameworkCore;


namespace PrinzipTrackerTest.Models
{
    public class PrinzipContext : DbContext
    {
        public PrinzipContext(DbContextOptions<PrinzipContext> options)
            : base(options)
        {
        }

        public DbSet<Subscription> Subscriptions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=./Data/subscriptions.db");
            }
        }
    }
}
