#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using YourGameServer.Data;
using YourGameServer.Models;

namespace YourGameServer.Pages.PlayerAccounts
{
    public class DeleteModel : PageModel
    {
        private readonly GameDbContext _context;

        public DeleteModel(GameDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public PlayerAccount PlayerAccount { get; set; }

        public async Task<IActionResult> OnGetAsync(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            PlayerAccount = await _context.PlayerAccounts.FirstOrDefaultAsync(m => m.ID == id);

            if (PlayerAccount == null)
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

            PlayerAccount = await _context.PlayerAccounts.FindAsync(id);

            if (PlayerAccount != null)
            {
                _context.PlayerAccounts.Remove(PlayerAccount);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
