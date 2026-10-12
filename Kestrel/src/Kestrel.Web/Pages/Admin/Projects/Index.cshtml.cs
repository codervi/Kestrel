using Kestrel.Web.Data;
using Kestrel.Web.Models;
using Kestrel.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Kestrel.Web.Pages.Admin.Projects;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly ImageStore _images;

    public IndexModel(AppDbContext db, ImageStore images)
    {
        _db = db;
        _images = images;
    }

    public List<Project> Items { get; set; } = new();

    public async Task OnGetAsync()
    {
        Items = await _db.Projects.AsNoTracking()
            .Include(p => p.Category)
            .OrderByDescending(p => p.Year)
            .ThenBy(p => p.Title)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var project = await _db.Projects.FindAsync(id);
        if (project != null)
        {
            _images.Delete(project.CoverImage);
            _db.Projects.Remove(project);
            await _db.SaveChangesAsync();
            TempData["Ok"] = $"\"{project.Title}\" was deleted.";
        }
        return RedirectToPage();
    }
}
