using Microsoft.AspNetCore.Mvc;

namespace YourProjectName.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayerAccountController : ControllerBase
{
    readonly ILogger<PlayerAccountController> _logger;

    public PlayerAccountController(ILogger<PlayerAccountController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IEnumerable<Models.PlayerAccount> Get()
    {
        return Enumerable.Range(1, 5).Select(index => {
            var now = DateTime.Now.AddDays(index);
            return new Models.PlayerAccount {
                ID = index,
                Since = now,
                LastLogin = now,
            };
        });
    }

    [HttpGet("{index}")]
    public Models.PlayerAccount Get(int index)
    {
        var now = DateTime.Now.AddDays(index);
        return new Models.PlayerAccount {
            ID = index,
            Since = now,
            LastLogin = now,
        };
    }
}
