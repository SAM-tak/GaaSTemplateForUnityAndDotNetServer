using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YourProjectName.Data;
using YourProjectName.Models;

namespace YourProjectName.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayerAccountController : ControllerBase
{
    readonly ILogger<PlayerAccountController> _logger;

    readonly GameDbContext _dbContext;

    public PlayerAccountController(GameDbContext dbContext, ILogger<PlayerAccountController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PlayerAccount>>> Get([FromQuery]int? s, [FromQuery]int? c)
    {
        if (await _dbContext.PlayerAccounts.AnyAsync()) {
            return await _dbContext.PlayerAccounts.OrderBy(i => i.ID).Skip(s ?? 0).Take(c ?? 50).ToListAsync();
        }
        return NotFound();
    }

    [HttpGet("{index}")]
    public async Task<ActionResult<PlayerAccount>> Get(int index)
    {
        var ret = await _dbContext.PlayerAccounts.FirstOrDefaultAsync(i => i.ID == index);
        if (ret != null) return ret;
        return NotFound();
    }
}
