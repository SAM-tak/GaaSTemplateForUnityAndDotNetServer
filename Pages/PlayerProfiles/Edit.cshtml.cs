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

namespace YourGameServer.Pages.PlayerProfiles
{
    public class EditModel : PageModel
    {
        private readonly YourGameServer.Data.GameDbContext _context;

        public EditModel(YourGameServer.Data.GameDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public PlayerProfile PlayerProfile { get; set; }

        public async Task<IActionResult> OnGetAsync(ulong? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            PlayerProfile = await _context.PlayerProfiles
                .Include(p => p.Owner).FirstOrDefaultAsync(m => m.Id == id);

            if (PlayerProfile == null)
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

            _context.Attach(PlayerProfile).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PlayerProfileExists(PlayerProfile.Id))
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

        private bool PlayerProfileExists(ulong id)
        {
            return _context.PlayerProfiles.Any(e => e.Id == id);
        }
    }
}
