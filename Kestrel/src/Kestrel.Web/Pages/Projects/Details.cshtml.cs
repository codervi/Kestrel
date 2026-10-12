using Kestrel.Web.Data;
using Kestrel.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Kestrel.Web.Pages.Projects;

public class DetailsModel : PageModel
{
    private readonly AppDbContext _db;
    public DetailsModel(AppDbContext db) => _db = db;

    public Project Project { get; set; } = null!;
    public List<Project> Related { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var project = await _db.Projects.AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null) return NotFound();
        Project = project;

        Related = await _db.Projects.AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.CategoryId == project.CategoryId && p.Id != project.Id)
            .OrderByDescending(p => p.Year)
            .Take(3)
            .ToListAsync();

        return Page();
    }
}
