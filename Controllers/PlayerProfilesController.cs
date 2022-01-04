using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YourGameServer.Data;
using YourGameServer.Models;

namespace YourGameServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlayerProfilesController : ControllerBase
    {
        private readonly GameDbContext _context;

        public PlayerProfilesController(GameDbContext context)
        {
            _context = context;
        }

        // GET: api/PlayerProfiles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PlayerProfile>>> GetPlayerProfiles()
        {
            return await _context.PlayerProfiles.ToListAsync();
        }

        // GET: api/PlayerProfiles/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PlayerProfile>> GetPlayerProfile(long id)
        {
            var playerProfile = await _context.PlayerProfiles.FindAsync(id);

            if (playerProfile == null)
            {
                return NotFound();
            }

            return playerProfile;
        }

        // PUT: api/PlayerProfiles/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPlayerProfile(long id, PlayerProfile playerProfile)
        {
            if (id != playerProfile.Id)
            {
                return BadRequest();
            }

            _context.Entry(playerProfile).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PlayerProfileExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/PlayerProfiles
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<PlayerProfile>> PostPlayerProfile(PlayerProfile playerProfile)
        {
            _context.PlayerProfiles.Add(playerProfile);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPlayerProfile", new { id = playerProfile.Id }, playerProfile);
        }

        // DELETE: api/PlayerProfiles/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlayerProfile(long id)
        {
            var playerProfile = await _context.PlayerProfiles.FindAsync(id);
            if (playerProfile == null)
            {
                return NotFound();
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
}
