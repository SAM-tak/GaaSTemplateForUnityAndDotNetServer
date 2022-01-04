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
        private readonly YourGameServer.Data.GameDbContext _context;

        public IndexModel(YourGameServer.Data.GameDbContext context)
        {
            _context = context;
        }

        public PaginatedList<PlayerAccount> PlayerAccounts { get;set; }

        public async Task OnGetAsync(int pageIndex = 1, int? maxCount = null)
        {
            PlayerAccounts = await PaginatedList<PlayerAccount>.CreateAsync(_context.PlayerAccounts, pageIndex, maxCount ?? 20);
        }
    }
}
