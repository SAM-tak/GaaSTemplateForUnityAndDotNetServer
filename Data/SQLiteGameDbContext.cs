using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace YourProjectName.Data;

public class SqliteGameDbContext : GameDbContext
{
    public SqliteGameDbContext(DbContextOptions<GameDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite("Data Source=game.db");
}

public class SqliteGameDbContextFactory : IDesignTimeDbContextFactory<SqliteGameDbContext>
{
    public SqliteGameDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<GameDbContext>();
        optionsBuilder.UseSqlite("Data Source=game.db");

        return new SqliteGameDbContext(optionsBuilder.Options);
    }
}
