using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YourGameServer.Data;
using YourGameServer.Models;

namespace YourGameServer.Controllers;

[Route("api/[controller]")]
//[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[ApiAuth]
[ApiController]
public class PlayerAccountsController(GameDbContext context) : ControllerBase
{
    private readonly GameDbContext _context = context;

    /// <summary>
    /// GET: api/PlayerAccounts?id=111&amp;id=112&amp;id=113
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="id"></param>
    /// <param name="s"></param>
    /// <param name="c"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MaskedPlayerAccount>>> GetPlayerAccounts([FromHeader] ulong playerId, [FromQuery] ulong[] id
#if DEBUG
        , [FromQuery] int? s, [FromQuery] int? c
#endif
    )
    {
        // Console.WriteLine($"User = {User.Identity.Name} {User.Identity.IsAuthenticated}");
        if(!await _context.PlayerAccounts.AnyAsync()) {
            return NotFound();
        }

        if(id != null && id.Length > 0) {
            return await _context.PlayerAccounts.Include(i => i.Profile)
                .Where(i => id.Contains(i.Id) && i.Status < PlayerAccountStatus.Banned).Select(i => i.MakeMasked()).ToListAsync();
        }
#if DEBUG
        return await _context.PlayerAccounts.Skip(s ?? 0).Take(c ?? 1).Select(i => i.MakeMasked()).ToListAsync();
#else
        return NoContent();
#endif
    }

    /// <summary>
    /// GET: api/PlayerAccounts/5
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<FormalPlayerAccount>> GetPlayerAccount([FromHeader] ulong playerId, ulong id)
    {
        if(playerId != id) {
            return BadRequest();
        }

        var playerAccount = await _context.PlayerAccounts.FindAsync(playerId);

        if(playerAccount == null) {
            return NotFound();
        }

        return playerAccount.MakeFormal();
    }

    /// <summary>
    /// PUT: api/PlayerAccounts/5
    /// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="id"></param>
    /// <param name="playerAccount"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> PutPlayerAccount([FromHeader] ulong playerId, ulong id, PlayerAccount playerAccount)
    {
        if(playerId != id || playerId != playerAccount.Id) {
            return BadRequest();
        }
        await Task.CompletedTask;
        return Forbid();

        // _context.Entry(playerAccount).State = EntityState.Modified;

        // try {
        //     await _context.SaveChangesAsync();
        // }
        // catch(DbUpdateConcurrencyException) {
        //     if(!PlayerAccountExists(playerId)) {
        //         return NotFound();
        //     }
        //     else {
        //         throw;
        //     }
        // }

        // return NoContent();
    }

    /// <summary>
    /// POST: api/PlayerAccounts
    /// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="playerAccount"></param>
    /// <returns>BadRequest or Forbid</returns>
    [HttpPost]
    public async Task<ActionResult<PlayerAccount>> PostPlayerAccount([FromHeader] ulong playerId, PlayerAccount playerAccount)
    {
        if(playerAccount.Id != playerId) {
            return BadRequest();
        }
        await Task.CompletedTask;
        return Forbid();
    }

    /// <summary>
    /// DELETE: api/PlayerAccounts/5
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePlayerAccount([FromHeader] ulong playerId, ulong id)
    {
        if(id != playerId) {
            return BadRequest();
        }
        var playerAccount = await _context.PlayerAccounts.FindAsync(playerId);
        if(playerAccount == null) {
            return NotFound();
        }

        //_context.PlayerAccounts.Remove(playerAccount);
        playerAccount.Status = PlayerAccountStatus.Expired;
        _context.Entry(playerAccount).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // private bool PlayerAccountExists(ulong id)
    // {
    //     return _context.PlayerAccounts.Any(e => e.Id == id);
    // }
}
