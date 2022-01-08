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
    [Route("api/{pid}/[controller]")]
    [ApiController]
    [ApiAuth]
    public class PlayerDevicesController : ControllerBase
    {
        private readonly GameDbContext _context;

        public PlayerDevicesController(GameDbContext context)
        {
            _context = context;
        }

        // GET: api/101/PlayerDevices
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PlayerDevice>>> GetPlayerDevices(long pid)
        {
            return await _context.PlayerDevices.Where(i => i.OwnerId == pid).ToListAsync();
        }

        // GET: api/101/PlayerDevices/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PlayerDevice>> GetPlayerDevice(long pid, long id)
        {
            Console.WriteLine($"User = {User.Identity}");

            var playerDevice = await _context.PlayerDevices.FindAsync(id);

            if (playerDevice == null)
            {
                return NotFound();
            }

            if(pid != playerDevice.OwnerId) {
                return BadRequest();
            }

            return playerDevice;
        }

        // PUT: api/101/PlayerDevices/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPlayerDevice(long pid, long id, PlayerDevice playerDevice)
        {
            if (id != playerDevice.Id || pid != playerDevice.OwnerId)
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

        // POST: api/PlayerDevices/{playerId}
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<PlayerDevice>> PostPlayerDevice(long pid, PlayerDevice playerDevice)
        {
            if(pid != playerDevice.OwnerId) {
                return BadRequest();
            }
            _context.PlayerDevices.Add(playerDevice);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPlayerDevice", new { id = playerDevice.Id }, playerDevice);
        }

        // DELETE: api/101/PlayerDevices/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlayerDevice(long pid, long id)
        {
            var playerDevice = await _context.PlayerDevices.FindAsync(id);
            if (playerDevice == null)
            {
                return NotFound();
            }

            if(pid != playerDevice.OwnerId) {
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
