using Kestrel.Web.Data;
using Kestrel.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Kestrel.Web.Pages.Admin.Quotes;

public class IndexModel : PageModel
{
    private const int PageSize = 10;
    private readonly AppDbContext _db;

    public IndexModel(AppDbContext db) => _db = db;

    [BindProperty(Name = "status", SupportsGet = true)]
    public QuoteStatus? Filter { get; set; }

    [BindProperty(Name = "p", SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public List<QuoteRequest> Items { get; set; } = new();
    public int TotalPages { get; set; }

    public async Task OnGetAsync()
    {
        var query = _db.QuoteRequests.AsNoTracking().Include(q => q.Category).AsQueryable();
        if (Filter.HasValue) query = query.Where(q => q.Status == Filter);

        var total = await query.CountAsync();
        TotalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
        PageNumber = Math.Clamp(PageNumber, 1, TotalPages);

        Items = await query
            .OrderByDescending(q => q.CreatedAt)
            .Skip((PageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostStatusAsync(int id, QuoteStatus newStatus, QuoteStatus? filter, int p = 1)
    {
        var quote = await _db.QuoteRequests.FindAsync(id);
        if (quote == null) return NotFound();

        quote.Status = newStatus;
        await _db.SaveChangesAsync();
        TempData["Ok"] = $"Request from {quote.FullName} marked as {newStatus.ToString().ToLower()}.";

        return RedirectToPage(new { status = filter, p });
    }
}
