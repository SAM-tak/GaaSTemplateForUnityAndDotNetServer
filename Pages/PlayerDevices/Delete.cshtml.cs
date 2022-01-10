using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using YourGameServer.Data;
using YourGameServer.Models;

namespace YourGameServer.Pages.PlayerDevices
{
    public class DeleteModel : PageModel
    {
        private readonly YourGameServer.Data.GameDbContext _context;

        public DeleteModel(YourGameServer.Data.GameDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public PlayerDevice PlayerDevice { get; set; }

        public async Task<IActionResult> OnGetAsync(ulong? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            PlayerDevice = await _context.PlayerDevices
                .Include(p => p.Owner).FirstOrDefaultAsync(m => m.Id == id);

            if (PlayerDevice == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            PlayerDevice = await _context.PlayerDevices.FindAsync(id);

            if (PlayerDevice != null)
            {
                _context.PlayerDevices.Remove(PlayerDevice);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
