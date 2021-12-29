using Microsoft.EntityFrameworkCore;

namespace YourGameServer.Data;

public class MySqlGameDbContext : GameDbContext
{
    public MySqlGameDbContext(DbContextOptions<GameDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseMySql(new MySqlServerVersion(new Version(10, 6, 5)));
}
