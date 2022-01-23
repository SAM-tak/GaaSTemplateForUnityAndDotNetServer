using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YourGameServer.Data;
using YourGameServer.Models;

namespace YourGameServer.Controllers
{
    [Route("api/[controller]")]
    [ApiAuth]
    [ApiController]
    public class PlayerOwnedServiceTicketsController : ControllerBase
    {
        private readonly GameDbContext _context;

        public PlayerOwnedServiceTicketsController(GameDbContext context)
        {
            _context = context;
        }

        // GET: api/PlayerOwnedServiceTickets
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PlayerOwnedServiceTicket>>> GetPlayerOwnedServiceTicket()
        {
            return await _context.PlayerOwnedServiceTickets.ToListAsync();
        }

        // GET: api/PlayerOwnedServiceTickets/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PlayerOwnedServiceTicket>> GetPlayerOwnedServiceTicket(ulong id)
        {
            var playerOwnedServiceTicket = await _context.PlayerOwnedServiceTickets.FindAsync(id);

            if (playerOwnedServiceTicket == null)
            {
                return NotFound();
            }

            return playerOwnedServiceTicket;
        }

        // PUT: api/PlayerOwnedServiceTickets/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPlayerOwnedServiceTicket(ulong id, PlayerOwnedServiceTicket playerOwnedServiceTicket)
        {
            if (id != playerOwnedServiceTicket.Id)
            {
                return BadRequest();
            }

            _context.Entry(playerOwnedServiceTicket).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PlayerOwnedServiceTicketExists(id))
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

        // POST: api/PlayerOwnedServiceTickets
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<PlayerOwnedServiceTicket>> PostPlayerOwnedServiceTicket(PlayerOwnedServiceTicket playerOwnedServiceTicket)
        {
            _context.PlayerOwnedServiceTickets.Add(playerOwnedServiceTicket);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPlayerOwnedServiceTicket", new { id = playerOwnedServiceTicket.Id }, playerOwnedServiceTicket);
        }

        // DELETE: api/PlayerOwnedServiceTickets/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlayerOwnedServiceTicket(ulong id)
        {
            var playerOwnedServiceTicket = await _context.PlayerOwnedServiceTickets.FindAsync(id);
            if (playerOwnedServiceTicket == null)
            {
                return NotFound();
            }

            _context.PlayerOwnedServiceTickets.Remove(playerOwnedServiceTicket);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PlayerOwnedServiceTicketExists(ulong id)
        {
            return _context.PlayerOwnedServiceTickets.Any(e => e.Id == id);
        }
    }
}
