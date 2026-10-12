using Kestrel.Web.Data;
using Kestrel.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Kestrel.Web.Pages.Projects;

public class IndexModel : PageModel
{
    private const int PageSize = 6;
    private readonly AppDbContext _db;

    public IndexModel(AppDbContext db) => _db = db;

    [BindProperty(Name = "category", SupportsGet = true)]
    public int? CategoryId { get; set; }

    [BindProperty(Name = "q", SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(Name = "p", SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public List<Category> Categories { get; set; } = new();
    public List<Project> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }

    public async Task OnGetAsync()
    {
        Categories = await _db.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync();

        var query = _db.Projects.AsNoTracking().Include(p => p.Category).AsQueryable();

        if (CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == CategoryId);

        if (!string.IsNullOrWhiteSpace(Search))
        {
            var term = Search.Trim();
            query = query.Where(p => p.Title.Contains(term) || p.Location.Contains(term));
        }

        TotalCount = await query.CountAsync();
        TotalPages = Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
        PageNumber = Math.Clamp(PageNumber, 1, TotalPages);

        Items = await query
            .OrderByDescending(p => p.Year)
            .ThenBy(p => p.Title)
            .Skip((PageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();
    }
}
