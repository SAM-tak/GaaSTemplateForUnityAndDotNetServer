using Microsoft.EntityFrameworkCore;

namespace YourGameServer.Data;

public class GameDbContext : DbContext
{
    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options)
    {
    }

    public DbSet<Models.PlayerAccount> PlayerAccounts { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Models.PlayerAccount>().ToTable("PlayerAccount");
    }
}
