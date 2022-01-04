using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using YourGameServer.Data;
using YourGameServer.Models;

namespace YourGameServer.Pages.PlayerProfiles
{
    public class DeleteModel : PageModel
    {
        private readonly YourGameServer.Data.GameDbContext _context;

        public DeleteModel(YourGameServer.Data.GameDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public PlayerProfile PlayerProfile { get; set; }

        public async Task<IActionResult> OnGetAsync(long? id)
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
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            PlayerProfile = await _context.PlayerProfiles.FindAsync(id);

            if (PlayerProfile != null)
            {
                _context.PlayerProfiles.Remove(PlayerProfile);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
