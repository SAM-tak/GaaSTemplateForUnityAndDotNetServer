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

namespace YourGameServer.Pages.PlayerAccounts
{
    public class EditModel : PageModel
    {
        private readonly YourGameServer.Data.GameDbContext _context;

        public EditModel(YourGameServer.Data.GameDbContext context)
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

            PlayerAccount = await _context.PlayerAccounts.FirstOrDefaultAsync(m => m.Id == id);

            if (PlayerAccount == null)
            {
                return NotFound();
            }
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

            _context.Attach(PlayerAccount).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PlayerAccountExists(PlayerAccount.Id))
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

        private bool PlayerAccountExists(long id)
        {
            return _context.PlayerAccounts.Any(e => e.Id == id);
        }
    }
}
