using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YourGameServer.Data;
using YourGameServer.Models;

namespace YourGameServer.Controllers
{
    [Route("api/[controller]")]
    [ApiAuth]
    [ApiController]
    public class PlayerOwnedPaidServiceTokensController(GameDbContext context) : ControllerBase
    {
        private readonly GameDbContext _context = context;

        // GET: api/PlayerOwnedPaidServiceTokens
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PlayerOwnedPaidServiceToken>>> GetPlayerOwnedPaidServiceToken()
        {
            return await _context.PlayerOwnedPaidServiceTokens.ToListAsync();
        }

        // GET: api/PlayerOwnedPaidServiceTokens/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PlayerOwnedPaidServiceToken>> GetPlayerOwnedPaidServiceToken(ulong id)
        {
            var playerOwnedPaidServiceToken = await _context.PlayerOwnedPaidServiceTokens.FindAsync(id);

            if(playerOwnedPaidServiceToken == null) {
                return NotFound();
            }

            return playerOwnedPaidServiceToken;
        }

        // PUT: api/PlayerOwnedPaidServiceTokens/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPlayerOwnedPaidServiceToken(ulong id, PlayerOwnedPaidServiceToken playerOwnedPaidServiceToken)
        {
            if(id != playerOwnedPaidServiceToken.Id) {
                return BadRequest();
            }

            _context.Entry(playerOwnedPaidServiceToken).State = EntityState.Modified;

            try {
                await _context.SaveChangesAsync();
            }
            catch(DbUpdateConcurrencyException) {
                if(!PlayerOwnedPaidServiceTokenExists(id)) {
                    return NotFound();
                }
                else {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/PlayerOwnedPaidServiceTokens
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<PlayerOwnedPaidServiceToken>> PostPlayerOwnedPaidServiceToken(PlayerOwnedPaidServiceToken playerOwnedPaidServiceToken)
        {
            _context.PlayerOwnedPaidServiceTokens.Add(playerOwnedPaidServiceToken);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPlayerOwnedPaidServiceToken", new { id = playerOwnedPaidServiceToken.Id }, playerOwnedPaidServiceToken);
        }

        // DELETE: api/PlayerOwnedPaidServiceTokens/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlayerOwnedPaidServiceToken(ulong id)
        {
            var playerOwnedPaidServiceToken = await _context.PlayerOwnedPaidServiceTokens.FindAsync(id);
            if(playerOwnedPaidServiceToken == null) {
                return NotFound();
            }

            _context.PlayerOwnedPaidServiceTokens.Remove(playerOwnedPaidServiceToken);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PlayerOwnedPaidServiceTokenExists(ulong id)
        {
            return _context.PlayerOwnedPaidServiceTokens.Any(e => e.Id == id);
        }
    }
}
