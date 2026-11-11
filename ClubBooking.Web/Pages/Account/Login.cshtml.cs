using System.ComponentModel.DataAnnotations;
using ClubBooking.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClubBooking.Web.Pages.Account;

public class LoginModel(ClubApi api) : PageModel
{
    [BindProperty, Required(ErrorMessage = "Введите email"), EmailAddress(ErrorMessage = "Неверный формат email")]
    public string Email { get; set; } = "";

    [BindProperty, Required(ErrorMessage = "Введите пароль"), DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await api.LoginAsync(Email, Password);
        if (result.Value == null)
        {
            ModelState.AddModelError("", result.Error ?? "Не удалось войти");
            return Page();
        }

        await CookieSignIn.WithTokenAsync(HttpContext, result.Value);
        return LocalRedirect(Url.IsLocalUrl(ReturnUrl) ? ReturnUrl : "/");
    }
}
