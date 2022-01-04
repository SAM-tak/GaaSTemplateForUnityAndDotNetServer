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
    [Route("api/[controller]/{playerid}")]
    [ApiController]
    [ApiAuth]
    public class PlayerDevicesController : ControllerBase
    {
        private readonly GameDbContext _context;

        public PlayerDevicesController(GameDbContext context)
        {
            _context = context;
        }

        // GET: api/PlayerDevices/9
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PlayerDevice>>> GetPlayerDevice(string playerId)
        {
            var playerIntId = await _context.PlayerAccounts.Where(x => x.PlayerId == playerId).Select(i => i.Id).FirstOrDefaultAsync();
            return await _context.PlayerDevices.Where(i => i.OwnerId == playerIntId).ToListAsync();
        }

        // GET: api/PlayerDevices/9/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PlayerDevice>> GetPlayerDevice(string playerId, long id)
        {
            var playerDevice = await _context.PlayerDevices.FindAsync(id);

            if (playerDevice == null)
            {
                return NotFound();
            }

            if(await _context.PlayerAccounts.Where(x => x.PlayerId == playerId).Select(i => i.Id).FirstOrDefaultAsync() != playerDevice.OwnerId) {
                return BadRequest();
            }

            return playerDevice;
        }

        // PUT: api/PlayerDevices/9/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPlayerDevice(string playerId, long id, PlayerDevice playerDevice)
        {
            if (id != playerDevice.Id || await _context.PlayerAccounts.Where(x => x.PlayerId == playerId).Select(i => i.Id).FirstOrDefaultAsync() != playerDevice.OwnerId)
            {
                return BadRequest();
            }

            _context.Entry(playerDevice).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PlayerDeviceExists(id))
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

        // POST: api/PlayerDevices
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<PlayerDevice>> PostPlayerDevice(string playerId, PlayerDevice playerDevice)
        {
            if(await _context.PlayerAccounts.Where(x => x.PlayerId == playerId).Select(i => i.Id).FirstOrDefaultAsync() != playerDevice.OwnerId) {
                return BadRequest();
            }
            _context.PlayerDevices.Add(playerDevice);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPlayerDevice", new { id = playerDevice.Id }, playerDevice);
        }

        // DELETE: api/PlayerDevices/9/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlayerDevice(string playerId, long id)
        {
            var playerDevice = await _context.PlayerDevices.FindAsync(id);
            if (playerDevice == null)
            {
                return NotFound();
            }

            if(await _context.PlayerAccounts.Where(x => x.PlayerId == playerId).Select(i => i.Id).FirstOrDefaultAsync() != playerDevice.OwnerId) {
                return BadRequest();
            }

            _context.PlayerDevices.Remove(playerDevice);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PlayerDeviceExists(long id)
        {
            return _context.PlayerDevices.Any(e => e.Id == id);
        }
    }
}
