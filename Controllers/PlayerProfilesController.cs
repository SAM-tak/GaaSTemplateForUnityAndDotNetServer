using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YourGameServer.Data;
using YourGameServer.Models;

namespace YourGameServer.Controllers;

[Route("api/{pid}/[controller]")]
[ApiController]
[ApiAuth]
public class PlayerProfilesController : ControllerBase
{
    private readonly GameDbContext _context;

    public PlayerProfilesController(GameDbContext context)
    {
        _context = context;
    }

    // GET: api/101/PlayerProfiles?id=102&id=103&id=104
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PlayerProfile>>> GetPlayerProfiles(long? pid, [FromQuery] long[] id)
    {
        if(pid is null) {
            throw new ArgumentNullException(nameof(pid));
        }

        return await _context.PlayerAccounts.Where(x => id.Any(i => i == x.Id)).Select(i => i.Profile).ToListAsync();
    }

    // GET: api/101/PlayerProfiles/self
    [HttpGet("self")]
    public async Task<ActionResult<PlayerProfile>> GetPlayerProfile(long pid)
    {
        var playerProfile = await _context.PlayerProfiles.Where(i => i.OwnerId == pid).FirstOrDefaultAsync();

        if(playerProfile == null) {
            return NotFound();
        }

        return playerProfile;
    }

    // PUT: api/101/PlayerProfiles
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut]
    public async Task<IActionResult> PutPlayerProfile(long? pid, PlayerProfile playerProfile)
    {
        var id = await _context.PlayerProfiles.Where(i => i.OwnerId == pid).Select(i => i.Id).FirstOrDefaultAsync(); // 0 is invalid value
        if(pid != playerProfile.OwnerId || id != playerProfile.Id) {
            return BadRequest();
        }

        _context.Entry(playerProfile).State = EntityState.Modified;

        try {
            await _context.SaveChangesAsync();
        }
        catch(DbUpdateConcurrencyException) {
            if(!PlayerProfileExists(playerProfile.Id)) {
                return NotFound();
            }
            else {
                throw;
            }
        }

        return NoContent();
    }

    // POST: api/101/PlayerProfiles
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<PlayerProfile>> PostPlayerProfile(long pid, PlayerProfile playerProfile)
    {
        if(pid != playerProfile.OwnerId) {
            return BadRequest();
        }
        _context.PlayerProfiles.Add(playerProfile);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetPlayerProfile", new { id = playerProfile.Id }, playerProfile);
    }

    // DELETE: api/101/PlayerProfiles
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePlayerProfile(long pid, long id)
    {
        var playerProfile = await _context.PlayerProfiles.FindAsync(id);
        if(playerProfile == null) {
            return NotFound();
        }
        if(pid != playerProfile.OwnerId) {
            return BadRequest();
        }

        _context.PlayerProfiles.Remove(playerProfile);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool PlayerProfileExists(long id)
    {
        return _context.PlayerProfiles.Any(e => e.Id == id);
    }
}
