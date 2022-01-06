#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YourGameServer.Data;
using YourGameServer.Models;

namespace YourGameServer.Controllers;

[Route("api/{pid}/[controller]")]
//[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiAuth]
[ApiController]
public class PlayerAccountsController : ControllerBase
{
    private readonly GameDbContext _context;

    public PlayerAccountsController(GameDbContext context)
    {
        _context = context;
    }

    // GET: api/101/PlayerAccounts?id=111&id=112&id=113
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PlayerAccount>>> GetPlayerAccounts(long? pid, [FromQuery] long[] id
#if DEBUG
        , [FromQuery] int? s, [FromQuery] int? c
#endif
    )
    {
        if(pid is null) {
            throw new ArgumentNullException(nameof(pid));
        }

        Console.WriteLine($"User = {User.Identity.Name} {User.Identity.IsAuthenticated}");
        //var idList = ids.Split('&').Distinct().ToArray();
        if(await _context.PlayerAccounts.AnyAsync()) {
            if(id != null && id.Length > 0) return await _context.PlayerAccounts.Where(i => id.Contains(i.Id)).ToListAsync();
#if DEBUG
            return await _context.PlayerAccounts.Skip(s ?? 0).Take(c ?? 1).ToListAsync();
#endif
        }
        return NotFound();
    }

    // GET: api/101/PlayerAccounts/self
    [HttpGet("self")]
    public async Task<ActionResult<PlayerAccount>> GetPlayerAccount(long pid)
    {
        var playerAccount = await _context.PlayerAccounts.FindAsync(pid);

        if(playerAccount == null) {
            return NotFound();
        }

        return playerAccount;
    }

    // PUT: api/PlayerAccounts/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut]
    public async Task<IActionResult> PutPlayerAccount(long pid, PlayerAccount playerAccount)
    {
        if(pid != playerAccount.Id) {
            return BadRequest();
        }

        _context.Entry(playerAccount).State = EntityState.Modified;

        try {
            await _context.SaveChangesAsync();
        }
        catch(DbUpdateConcurrencyException) {
            if(!PlayerAccountExists(pid)) {
                return NotFound();
            }
            else {
                throw;
            }
        }

        return NoContent();
    }

    // POST: api/PlayerAccounts
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<PlayerAccount>> PostPlayerAccount(PlayerAccount playerAccount)
    {
        var playerId = await LUID.NewLUIDStringAsync(async (i) => ! await _context.PlayerAccounts.AnyAsync(x => x.Luid == i));
        _context.PlayerAccounts.Add(playerAccount with {
            Luid = playerId
        });
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetPlayerAccount", new { id = playerAccount.Id }, playerAccount);
    }

    // DELETE: api/101/PlayerAccounts
    [HttpDelete]
    public async Task<IActionResult> DeletePlayerAccount(long pid)
    {
        var playerAccount = await _context.PlayerAccounts.FindAsync(pid);
        if(playerAccount == null) {
            return NotFound();
        }

        _context.PlayerAccounts.Remove(playerAccount);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool PlayerAccountExists(long id)
    {
        return _context.PlayerAccounts.Any(e => e.Id == id);
    }
}
