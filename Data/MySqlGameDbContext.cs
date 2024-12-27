using System;
using Microsoft.EntityFrameworkCore;

namespace YourGameServer.Data;

public class MySqlGameDbContext(DbContextOptions<GameDbContext> options) : GameDbContext(options)
{
    protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseMySql(new MySqlServerVersion(new Version(10, 6, 5)));
}
