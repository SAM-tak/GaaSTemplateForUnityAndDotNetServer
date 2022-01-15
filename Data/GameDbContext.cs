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

    public DbSet<PlayerOwnedFreeServiceToken> PlayerOwnedFreeServiceTokens { get; set; }

    public DbSet<PlayerOwnedPaidServiceToken> PlayerOwnedPaidServiceTokens { get; set; }

    public DbSet<PlayerOwnedServiceTicket> PlayerOwnedServiceTickets { get; set; }

    public DbSet<ServiceToken> ServiceTokens { get; set; }

    public DbSet<ServiceTicket> ServiceTickets { get; set; }

    public DbSet<LootBox> LootBoxes { get; set; }
}
