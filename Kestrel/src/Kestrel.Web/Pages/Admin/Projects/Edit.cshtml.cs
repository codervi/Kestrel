using System.ComponentModel.DataAnnotations;
using Kestrel.Web.Data;
using Kestrel.Web.Models;
using Kestrel.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Kestrel.Web.Pages.Admin.Projects;

// One page for both "add" and "edit". No id in the URL means a new project.
public class EditModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly ImageStore _images;

    public EditModel(AppDbContext db, ImageStore images)
    {
        _db = db;
        _images = images;
    }

    [BindProperty] public InputModel Input { get; set; } = new();
    [BindProperty] public IFormFile? Image { get; set; }
    [BindProperty] public bool RemoveImage { get; set; }

    public int? Id { get; set; }
    public string? CurrentImage { get; set; }
    public List<Category> Categories { get; set; } = new();

    public class InputModel
    {
        [Required, StringLength(120)]
        public string Title { get; set; } = "";

        [Required, StringLength(100)]
        public string Location { get; set; } = "";

        [Range(1950, 2100, ErrorMessage = "Enter a valid year.")]
        public int Year { get; set; }

        [StringLength(100), Display(Name = "Client")]
        public string? ClientName { get; set; }

        [Range(1, 10000000, ErrorMessage = "Enter the area in square metres.")]
        [Display(Name = "Area (m²)")]
        public int? AreaM2 { get; set; }

        [Required, StringLength(3000)]
        public string Description { get; set; } = "";

        [Range(1, int.MaxValue, ErrorMessage = "Choose a category.")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Display(Name = "Show on the home page")]
        public bool IsFeatured { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        await LoadCategoriesAsync();
        Id = id;

        if (id == null)
        {
            Input.Year = DateTime.Now.Year;
            return Page();
        }

        var p = await _db.Projects.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (p == null) return NotFound();

        CurrentImage = p.CoverImage;
        Input = new InputModel
        {
            Title = p.Title,
            Location = p.Location,
            Year = p.Year,
            ClientName = p.ClientName,
            AreaM2 = p.AreaM2,
            Description = p.Description,
            CategoryId = p.CategoryId,
            IsFeatured = p.IsFeatured
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        Id = id;

        Project? project = null;
        if (id != null)
        {
            project = await _db.Projects.FindAsync(id);
            if (project == null) return NotFound();
            CurrentImage = project.CoverImage;
        }

        if (!await _db.Categories.AnyAsync(c => c.Id == Input.CategoryId))
            ModelState.AddModelError("Input.CategoryId", "Choose a category.");

        string? newImage = null;
        if (ModelState.IsValid && Image is { Length: > 0 })
        {
            var (name, error) = await _images.SaveAsync(Image);
            if (error != null) ModelState.AddModelError(nameof(Image), error);
            else newImage = name;
        }

        if (!ModelState.IsValid)
        {
            await LoadCategoriesAsync();
            return Page();
        }

        if (project == null)
        {
            project = new Project();
            _db.Projects.Add(project);
        }

        project.Title = Input.Title.Trim();
        project.Location = Input.Location.Trim();
        project.Year = Input.Year;
        project.ClientName = string.IsNullOrWhiteSpace(Input.ClientName) ? null : Input.ClientName.Trim();
        project.AreaM2 = Input.AreaM2;
        project.Description = Input.Description.Trim();
        project.CategoryId = Input.CategoryId;
        project.IsFeatured = Input.IsFeatured;

        if (newImage != null)
        {
            _images.Delete(project.CoverImage);
            project.CoverImage = newImage;
        }
        else if (RemoveImage)
        {
            _images.Delete(project.CoverImage);
            project.CoverImage = null;
        }

        await _db.SaveChangesAsync();
        TempData["Ok"] = $"\"{project.Title}\" was saved.";
        return RedirectToPage("./Index");
    }

    private async Task LoadCategoriesAsync()
        => Categories = await _db.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
}
