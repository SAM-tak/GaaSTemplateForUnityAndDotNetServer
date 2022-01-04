#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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

    // GET: api/PlayerAccounts
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PlayerAccount>>> GetPlayerAccounts([FromQuery] int? s, [FromQuery] int? c, [FromQuery] long[] id)
    {
        if(await _context.PlayerAccounts.AnyAsync()) {
            if(id != null) return await _context.PlayerAccounts.Where(i => id.Contains(i.Id)).ToListAsync();
            return await _context.PlayerAccounts.OrderBy(i => i.Id).Skip(s ?? 0).Take(c ?? 0).ToListAsync();
        }
        return NotFound();
    }

    // GET: api/PlayerAccounts/5
    [HttpGet("{id}")]
    public async Task<ActionResult<PlayerAccount>> GetPlayerAccount(long id)
    {
        var playerAccount = await _context.PlayerAccounts.FindAsync(id);

        if(playerAccount == null) {
            return NotFound();
        }

        return playerAccount;
    }

    // PUT: api/PlayerAccounts/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutPlayerAccount(long id, PlayerAccount playerAccount)
    {
        if(id != playerAccount.Id) {
            return BadRequest();
        }

        _context.Entry(playerAccount).State = EntityState.Modified;

        try {
            await _context.SaveChangesAsync();
        }
        catch(DbUpdateConcurrencyException) {
            if(!PlayerAccountExists(id)) {
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
    [Authorize]
    public async Task<ActionResult<PlayerAccount>> PostPlayerAccount(PlayerAccount playerAccount)
    {
        var playerId = await LUID.NewLUIDStringAsync(async (i) => ! await _context.PlayerAccounts.AnyAsync(x => x.PlayerId == i));
        _context.PlayerAccounts.Add(playerAccount with {
            PlayerId = playerId,
            Secret = RandomNumberGenerator.GetBytes(32)
        });
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetPlayerAccount", new { id = playerAccount.Id }, playerAccount);
    }

    // DELETE: api/PlayerAccounts/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePlayerAccount(long id)
    {
        var playerAccount = await _context.PlayerAccounts.FindAsync(id);
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

    public static async Task<PlayerAccount> CreateAsync(GameDbContext context, AccountCreationModel accountCreationModel)
    {
        var playerId = await LUID.NewLUIDStringAsync(async (i) => !await context.PlayerAccounts.AnyAsync(x => x.PlayerId == i));
        var curDateTime = DateTime.UtcNow;
        return new PlayerAccount {
            PlayerId = playerId,
            Secret = RandomNumberGenerator.GetBytes(32),
            DeviceList = new() {
                new() {
                    DeviceType = accountCreationModel.DeviceType,
                    DeviceId = accountCreationModel.DeviceId,
                    Since = curDateTime,
                    LastUsed = curDateTime,
                }
            },
            Since = curDateTime,
            LastLogin = curDateTime,
            Profile = new()
        };
    }
}
