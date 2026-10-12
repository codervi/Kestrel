using Kestrel.Web.Data;
using Kestrel.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Kestrel.Web.Pages;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    public List<Project> Featured { get; set; } = new();
    public int ProjectCount { get; set; }
    public int TotalArea { get; set; }
    public int CategoryCount { get; set; }

    public async Task OnGetAsync()
    {
        Featured = await _db.Projects.AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.IsFeatured)
            .OrderByDescending(p => p.Year)
            .Take(3)
            .ToListAsync();

        ProjectCount = await _db.Projects.CountAsync();
        TotalArea = await _db.Projects.SumAsync(p => p.AreaM2 ?? 0);
        CategoryCount = await _db.Categories.CountAsync();
    }
}
