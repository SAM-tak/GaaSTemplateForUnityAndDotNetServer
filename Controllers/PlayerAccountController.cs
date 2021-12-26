using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YourProjectName.Data;

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
    public async Task<IActionResult> Get()
    {
        if (_dbContext.PlayerAccounts.Any()) {
            var ret = await _dbContext.PlayerAccounts.OrderBy(i => i.ID).Take(50).ToListAsync();
            return Ok(ret);
        }
        return NotFound();
    }

    [HttpGet("{index}")]
    public async Task<IActionResult> Get(int index)
    {
        var ret = await _dbContext.PlayerAccounts.FirstOrDefaultAsync(i => i.ID == index);
        if (ret != null) return Ok(ret);
        return NotFound();
    }
}
