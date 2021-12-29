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
    public class IndexModel : PageModel
    {
        private readonly GameDbContext _context;

        public IndexModel(GameDbContext context)
        {
            _context = context;
        }

        public IList<PlayerAccount> PlayerAccount { get;set; }

        public async Task OnGetAsync()
        {
            PlayerAccount = await _context.PlayerAccounts.ToListAsync();
        }
    }
}
