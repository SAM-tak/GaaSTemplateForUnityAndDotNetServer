using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace YourGameServer.Explorer.Data;

public class SqliteExplorerDbContext(DbContextOptions<ExplorerDbContext> options) : ExplorerDbContext(options)
{
    protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite("Data Source=explorer.db");
}

public class SqliteGameDbContextFactory : IDesignTimeDbContextFactory<SqliteExplorerDbContext>
{
    public SqliteExplorerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ExplorerDbContext>();
        optionsBuilder.UseSqlite("Data Source=explorer.db");

        return new SqliteExplorerDbContext(optionsBuilder.Options);
    }
}
