using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace YourGameServer.Explorer.Data;

public class ExplorerDbContext(DbContextOptions<ExplorerDbContext> options) : IdentityDbContext<ExplorerUser>(options)
{
}
