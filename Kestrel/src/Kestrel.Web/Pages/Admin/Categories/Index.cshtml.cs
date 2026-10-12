using System.ComponentModel.DataAnnotations;
using Kestrel.Web.Data;
using Kestrel.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Kestrel.Web.Pages.Admin.Categories;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    public record Row(int Id, string Name, int ProjectCount);

    public List<Row> Items { get; set; } = new();

    [BindProperty, Required, StringLength(60)]
    [Display(Name = "Category name")]
    public string NewName { get; set; } = "";

    public async Task OnGetAsync() => await LoadAsync();

    public async Task<IActionResult> OnPostAddAsync()
    {
        var name = NewName?.Trim() ?? "";

        if (name.Length > 0 && await _db.Categories.AnyAsync(c => c.Name == name))
            ModelState.AddModelError(nameof(NewName), "That category already exists.");

        if (!ModelState.IsValid)
        {
            await LoadAsync();
            return Page();
        }

        _db.Categories.Add(new Category { Name = name });
        await _db.SaveChangesAsync();
        TempData["Ok"] = $"Category \"{name}\" added.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category == null) return RedirectToPage();

        var inUse = await _db.Projects.AnyAsync(p => p.CategoryId == id)
                    || await _db.QuoteRequests.AnyAsync(q => q.CategoryId == id);
        if (inUse)
        {
            TempData["Error"] = $"\"{category.Name}\" is used by projects or quote requests, so it can't be deleted.";
            return RedirectToPage();
        }

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
        TempData["Ok"] = $"Category \"{category.Name}\" deleted.";
        return RedirectToPage();
    }

    private async Task LoadAsync()
    {
        Items = await _db.Categories.AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new Row(c.Id, c.Name, c.Projects.Count))
            .ToListAsync();
    }
}
