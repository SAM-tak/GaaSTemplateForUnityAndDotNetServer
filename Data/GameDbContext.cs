using YourProjectName.Models;
using Microsoft.EntityFrameworkCore;

namespace YourProjectName.Data;

public class GameDbContext : DbContext
{
    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options)
    {
    }

    public DbSet<PlayerAccount> PlayerAccounts { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PlayerAccount>().ToTable("PlayerAccount");
    }
}
