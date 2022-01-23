using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YourGameServer.Data;
using YourGameServer.Models;

namespace YourGameServer.Controllers
{
    [Route("api/[controller]")]
    [ApiAuth]
    [ApiController]
    public class PlayerOwnedFreeServiceTokensController : ControllerBase
    {
        private readonly GameDbContext _context;

        public PlayerOwnedFreeServiceTokensController(GameDbContext context)
        {
            _context = context;
        }

        // GET: api/PlayerOwnedFreeServiceTokens
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PlayerOwnedFreeServiceToken>>?> GetPlayerOwnedFreeServiceToken()
        {
            return await _context.PlayerOwnedFreeServiceTokens.ToListAsync();
        }

        // GET: api/PlayerOwnedFreeServiceTokens/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PlayerOwnedFreeServiceToken>> GetPlayerOwnedFreeServiceToken(ulong id)
        {
            var playerOwnedFreeServiceToken = await _context.PlayerOwnedFreeServiceTokens.FindAsync(id);
 
            if (playerOwnedFreeServiceToken == null)
            {
                return NotFound();
            }

            return playerOwnedFreeServiceToken;
        }

        // PUT: api/PlayerOwnedFreeServiceTokens/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPlayerOwnedFreeServiceToken(ulong id, PlayerOwnedFreeServiceToken playerOwnedFreeServiceToken)
        {
            if (id != playerOwnedFreeServiceToken.Id)
            {
                return BadRequest();
            }

            _context.Entry(playerOwnedFreeServiceToken).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PlayerOwnedFreeServiceTokenExists(id))
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

        // POST: api/PlayerOwnedFreeServiceTokens
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<PlayerOwnedFreeServiceToken>> PostPlayerOwnedFreeServiceToken(PlayerOwnedFreeServiceToken playerOwnedFreeServiceToken)
        {
            _context.PlayerOwnedFreeServiceTokens.Add(playerOwnedFreeServiceToken);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPlayerOwnedFreeServiceToken", new { id = playerOwnedFreeServiceToken.Id }, playerOwnedFreeServiceToken);
        }

        // DELETE: api/PlayerOwnedFreeServiceTokens/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlayerOwnedFreeServiceToken(ulong id)
        {
            var playerOwnedFreeServiceToken = await _context.PlayerOwnedFreeServiceTokens.FindAsync(id);
            if (playerOwnedFreeServiceToken == null)
            {
                return NotFound();
            }

            _context.PlayerOwnedFreeServiceTokens.Remove(playerOwnedFreeServiceToken);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PlayerOwnedFreeServiceTokenExists(ulong id)
        {
            return _context.PlayerOwnedFreeServiceTokens.Any(e => e.Id == id);
        }
    }
}
