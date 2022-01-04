using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using YourGameServer.Data;
using YourGameServer.Models;

namespace YourGameServer.Pages.PlayerDevices
{
    public class CreateModel : PageModel
    {
        private readonly YourGameServer.Data.GameDbContext _context;

        public CreateModel(YourGameServer.Data.GameDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
        ViewData["OwnerId"] = new SelectList(_context.PlayerAccounts, "Id", "Id");
            return Page();
        }

        [BindProperty]
        public PlayerDevice PlayerDevice { get; set; }

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.PlayerDevices.Add(PlayerDevice);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
