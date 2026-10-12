using Kestrel.Web.Data;
using Kestrel.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Kestrel.Web.Pages.Admin;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    public int NewQuotes { get; set; }
    public int ProjectCount { get; set; }
    public int CategoryCount { get; set; }
    public List<QuoteRequest> Latest { get; set; } = new();

    public async Task OnGetAsync()
    {
        NewQuotes = await _db.QuoteRequests.CountAsync(q => q.Status == QuoteStatus.New);
        ProjectCount = await _db.Projects.CountAsync();
        CategoryCount = await _db.Categories.CountAsync();

        Latest = await _db.QuoteRequests.AsNoTracking()
            .Include(q => q.Category)
            .OrderByDescending(q => q.CreatedAt)
            .Take(5)
            .ToListAsync();
    }
}
