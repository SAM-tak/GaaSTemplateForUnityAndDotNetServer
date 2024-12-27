using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YourGameServer.Data;
using YourGameServer.Models;

namespace YourGameServer.Controllers;

[Route("api/[controller]")]
[ApiAuth]
[ApiController]
public class PlayerDevicesController(GameDbContext context) : ControllerBase
{
    private readonly GameDbContext _context = context;

    // GET: api/PlayerDevices
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PlayerDevice>>> GetPlayerDevices([FromHeader] ulong playerId)
    {
        return await _context.PlayerDevices.Where(i => i.OwnerId == playerId).ToListAsync();
    }

    // GET: api/101/PlayerDevices/5
    [HttpGet("{id}")]
    public async Task<ActionResult<PlayerDevice>> GetPlayerDevice([FromHeader] ulong playerId, ulong id)
    {
        Console.WriteLine($"User = {User.Identity}");

        var playerDevice = await _context.PlayerDevices.FindAsync(id);

        if(playerDevice == null) {
            return NotFound();
        }

        if(playerId != playerDevice.OwnerId) {
            return BadRequest();
        }

        return playerDevice;
    }

    // PUT: api/PlayerDevices/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutPlayerDevice([FromHeader] ulong playerId, ulong id, PlayerDevice playerDevice)
    {
        if(id != playerDevice.Id || playerId != playerDevice.OwnerId) {
            return BadRequest();
        }

        _context.Entry(playerDevice).State = EntityState.Modified;

        try {
            await _context.SaveChangesAsync();
        }
        catch(DbUpdateConcurrencyException) {
            if(!PlayerDeviceExists(id)) {
                return NotFound();
            }
            else {
                throw;
            }
        }

        return NoContent();
    }

    // POST: api/PlayerDevices/{playerId}
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<PlayerDevice>> PostPlayerDevice([FromHeader] ulong playerId, PlayerDevice playerDevice)
    {
        if(playerId != playerDevice.OwnerId) {
            return BadRequest();
        }
        _context.PlayerDevices.Add(playerDevice);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetPlayerDevice", new { id = playerDevice.Id }, playerDevice);
    }

    // DELETE: api/101/PlayerDevices/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePlayerDevice([FromHeader] ulong playerId, ulong id)
    {
        var playerDevice = await _context.PlayerDevices.FindAsync(id);
        if(playerDevice == null) {
            return NotFound();
        }

        if(playerId != playerDevice.OwnerId) {
            return BadRequest();
        }

        _context.PlayerDevices.Remove(playerDevice);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool PlayerDeviceExists(ulong id)
    {
        return _context.PlayerDevices.Any(e => e.Id == id);
    }
}
