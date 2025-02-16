using Microsoft.EntityFrameworkCore;
using YourGameServer.Explorer.Models;

namespace YourGameServer.Explorer.Data;

public class ExplorerDbContext(DbContextOptions<ExplorerDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; init; }
    public DbSet<RoleAssign> RoleAssigns { get; init; }
}
