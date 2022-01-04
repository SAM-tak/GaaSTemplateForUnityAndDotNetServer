using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using YourGameServer.Models;

namespace YourGameServer.Data;

public class GameDbContext : DbContext
{
    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options)
    {
    }

    public DbSet<PlayerAccount> PlayerAccounts { get; set; }

    public DbSet<PlayerProfile> PlayerProfiles { get; set; }

    public DbSet<PlayerDevice> PlayerDevices { get; set; }
}
