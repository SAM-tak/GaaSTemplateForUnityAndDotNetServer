using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YourGameServer.Data;
using YourGameServer.Models;

namespace YourGameServer.Controllers;

[Route("api/[controller]")]
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

    // GET: api/PlayerAccounts?id=111&id=112&id=113
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MaskedPlayerAccount>>> GetPlayerAccounts([FromHeader] ulong playerId, [FromQuery] ulong[] id
#if DEBUG
        , [FromQuery] int? s, [FromQuery] int? c
#endif
    )
    {
        //Console.WriteLine($"User = {User.Identity.Name} {User.Identity.IsAuthenticated}");
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

    // GET: api/PlayerAccounts/5
    [HttpGet("{id}")]
    public async Task<ActionResult<PlayerAccount>> GetPlayerAccount([FromHeader] ulong playerId, ulong id)
    {
        if(playerId != id) {
            return BadRequest();
        }

        var playerAccount = await _context.PlayerAccounts.FindAsync(playerId);

        if(playerAccount == null) {
            return NotFound();
        }

        return playerAccount;
    }

    // PUT: api/PlayerAccounts/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutPlayerAccount([FromHeader] ulong playerId, ulong id, PlayerAccount playerAccount)
    {
        if(playerId != id || playerId != playerAccount.Id) {
            return BadRequest();
        }

        _context.Entry(playerAccount).State = EntityState.Modified;

        try {
            await _context.SaveChangesAsync();
        }
        catch(DbUpdateConcurrencyException) {
            if(!PlayerAccountExists(playerId)) {
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
    // [HttpPost]
    // public async Task<ActionResult<PlayerAccount>> PostPlayerAccount(PlayerAccount playerAccount)
    // {
    //     var playerId = await LUID.NewLUIDStringAsync(async (i) => ! await _context.PlayerAccounts.AnyAsync(x => x.Luid == i));
    //     _context.PlayerAccounts.Add(playerAccount with {
    //         Luid = playerId
    //     });
    //     await _context.SaveChangesAsync();

    //     return CreatedAtAction("GetPlayerAccount", new { id = playerAccount.Id }, playerAccount);
    // }

    // DELETE: api/PlayerAccounts/5
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

    private bool PlayerAccountExists(ulong id)
    {
        return _context.PlayerAccounts.Any(e => e.Id == id);
    }
}
