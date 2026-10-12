using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Kestrel.Web.Pages.Account;

public class LoginModel : PageModel
{
    private readonly SignInManager<IdentityUser> _signIn;
    public LoginModel(SignInManager<IdentityUser> signIn) => _signIn = signIn;

    [BindProperty] public InputModel Input { get; set; } = new();

    [BindProperty(SupportsGet = true)] public string? ReturnUrl { get; set; }

    public class InputModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Display(Name = "Keep me signed in")]
        public bool RememberMe { get; set; }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var result = await _signIn.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                return LocalRedirect(ReturnUrl);
            return RedirectToPage("/Admin/Index");
        }

        ModelState.AddModelError(string.Empty, result.IsLockedOut
            ? "Too many attempts. Try again in a few minutes."
            : "Wrong email or password.");
        return Page();
    }
}
