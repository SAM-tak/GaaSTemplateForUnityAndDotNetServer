#nullable disable
using Microsoft.EntityFrameworkCore;
using YourGameServer.Models;

namespace YourGameServer.Data;

public class GameDbContext(DbContextOptions<GameDbContext> options) : DbContext(options)
{
    public DbSet<PlayerAccount> PlayerAccounts { get; init; }

    public DbSet<PlayerProfile> PlayerProfiles { get; init; }

    public DbSet<PlayerDevice> PlayerDevices { get; init; }

    public DbSet<PlayerOwnedFreeServiceToken> PlayerOwnedFreeServiceTokens { get; init; }

    public DbSet<PlayerOwnedPaidServiceToken> PlayerOwnedPaidServiceTokens { get; init; }

    public DbSet<PlayerOwnedServiceTicket> PlayerOwnedServiceTickets { get; init; }

    public DbSet<ServiceToken> ServiceTokens { get; init; }

    public DbSet<ServiceTicket> ServiceTickets { get; init; }

    public DbSet<LootBox> LootBoxes { get; init; }
}
