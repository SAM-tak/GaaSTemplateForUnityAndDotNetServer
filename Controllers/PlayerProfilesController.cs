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

[Route("api/[controller]")]
[ApiController]
[ApiAuth]
public class PlayerProfilesController : ControllerBase
{
    private readonly GameDbContext _context;

    public PlayerProfilesController(GameDbContext context)
    {
        _context = context;
    }

    // GET: api/PlayerProfiles?id=102&id=103&id=104
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PlayerProfile>>> GetPlayerProfiles([FromHeader] ulong playerId, [FromQuery] ulong[] id)
    {
        return await _context.PlayerProfiles.Where(x => id.Any(i => i == x.Id)).Select(i => i.OwnerId == playerId ? i : i /* return masked data...*/).ToListAsync();
    }

    // GET: api/PlayerProfiles/5
    [HttpGet("{id}")]
    public async Task<ActionResult<PlayerProfile>> GetPlayerProfile([FromHeader] ulong playerId, ulong id)
    {
        var playerProfile = await _context.PlayerProfiles.FindAsync(id);

        if(playerProfile == null) {
            return NotFound();
        }

        if(playerId != playerProfile.OwnerId) {
            // return masked data...
        }

        return playerProfile;
    }

    // PUT: api/PlayerProfiles/id
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutPlayerProfile([FromHeader] ulong playerId, [FromQuery] ulong id, PlayerProfile playerProfile)
    {
        if(!PlayerProfileExists(id)) {
            return NotFound();
        }

        if(playerId != playerProfile.OwnerId || id != playerProfile.Id) {
            return BadRequest();
        }

        playerProfile.LastUpdate = DateTime.UtcNow;

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

    // POST: api/PlayerProfiles
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<PlayerProfile>> PostPlayerProfile([FromHeader] ulong playerId, PlayerProfile playerProfile)
    {
        if(playerId != playerProfile.OwnerId) {
            return BadRequest();
        }
        
        playerProfile.LastUpdate = DateTime.UtcNow;

        _context.PlayerProfiles.Add(playerProfile);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetPlayerProfile", new { id = playerProfile.Id }, playerProfile);
    }

    // DELETE: api/PlayerProfiles
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePlayerProfile([FromHeader] ulong playerId, ulong id)
    {
        var playerProfile = await _context.PlayerProfiles.FindAsync(id);
        if(playerProfile == null) {
            return NotFound();
        }
        if(playerId != playerProfile.OwnerId) {
            return BadRequest();
        }

        _context.PlayerProfiles.Remove(playerProfile);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool PlayerProfileExists(ulong id)
    {
        return _context.PlayerProfiles.Any(e => e.Id == id);
    }
}
