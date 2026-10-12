using System.ComponentModel.DataAnnotations;
using Kestrel.Web.Data;
using Kestrel.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Kestrel.Web.Pages;

public class QuoteModel : PageModel
{
    private readonly AppDbContext _db;
    public QuoteModel(AppDbContext db) => _db = db;

    public static readonly string[] Budgets =
    {
        "Under €50,000", "€50,000 to €250,000", "€250,000 to €1 million", "Over €1 million", "Not sure yet"
    };

    [BindProperty] public InputModel Input { get; set; } = new();

    // Hidden field. Real visitors never see it, simple bots fill it in.
    [BindProperty] public string? Website { get; set; }

    public List<Category> Categories { get; set; } = new();

    public class InputModel
    {
        [Required, StringLength(100), Display(Name = "Full name")]
        public string FullName { get; set; } = "";

        [Required, EmailAddress, StringLength(120)]
        public string Email { get; set; } = "";

        [Phone, StringLength(30)]
        public string? Phone { get; set; }

        [StringLength(100)]
        public string? Company { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Choose a project type.")]
        [Display(Name = "Project type")]
        public int CategoryId { get; set; }

        public string? Budget { get; set; }

        [Required(ErrorMessage = "Tell us a little about the project.")]
        [StringLength(2000, MinimumLength = 20, ErrorMessage = "Please write at least 20 characters.")]
        [Display(Name = "About the project")]
        public string Message { get; set; } = "";
    }

    public async Task OnGetAsync(int? category)
    {
        if (category.HasValue) Input.CategoryId = category.Value;
        await LoadCategoriesAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // bot filled the hidden field: pretend it worked
        if (!string.IsNullOrEmpty(Website)) return RedirectToPage("QuoteSent");

        if (!await _db.Categories.AnyAsync(c => c.Id == Input.CategoryId))
            ModelState.AddModelError("Input.CategoryId", "Choose a project type.");

        if (!ModelState.IsValid)
        {
            await LoadCategoriesAsync();
            return Page();
        }

        _db.QuoteRequests.Add(new QuoteRequest
        {
            FullName = Input.FullName.Trim(),
            Email = Input.Email.Trim(),
            Phone = Input.Phone?.Trim(),
            Company = Input.Company?.Trim(),
            CategoryId = Input.CategoryId,
            Budget = Input.Budget,
            Message = Input.Message.Trim()
        });
        await _db.SaveChangesAsync();

        return RedirectToPage("QuoteSent");
    }

    private async Task LoadCategoriesAsync()
        => Categories = await _db.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
}
