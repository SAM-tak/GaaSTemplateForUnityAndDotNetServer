using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using YourGameServer.Data;
using YourGameServer.Models;

namespace YourGameServer.Pages.PlayerProfiles
{
    public class IndexModel : PageModel
    {
        private readonly YourGameServer.Data.GameDbContext _context;

        public IndexModel(YourGameServer.Data.GameDbContext context)
        {
            _context = context;
        }

        public PaginatedList<PlayerProfile> PlayerProfiles { get;set; }

        public async Task OnGetAsync(int pageIndex = 1, int? maxCount = null)
        {
            PlayerProfiles = await PaginatedList<PlayerProfile>.CreateAsync(_context.PlayerProfiles
                .Include(p => p.Owner), pageIndex, maxCount ?? 20);
        }
    }
}
