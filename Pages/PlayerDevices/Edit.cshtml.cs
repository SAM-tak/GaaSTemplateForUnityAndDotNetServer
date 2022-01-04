using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using YourGameServer.Data;
using YourGameServer.Models;

namespace YourGameServer.Pages.PlayerDevices
{
    public class EditModel : PageModel
    {
        private readonly YourGameServer.Data.GameDbContext _context;

        public EditModel(YourGameServer.Data.GameDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public PlayerDevice PlayerDevice { get; set; }

        public async Task<IActionResult> OnGetAsync(long? id)
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
           ViewData["OwnerId"] = new SelectList(_context.PlayerAccounts, "Id", "Id");
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(PlayerDevice).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PlayerDeviceExists(PlayerDevice.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool PlayerDeviceExists(long id)
        {
            return _context.PlayerDevices.Any(e => e.Id == id);
        }
    }
}
