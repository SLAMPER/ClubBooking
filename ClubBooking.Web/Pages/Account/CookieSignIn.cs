using System.Security.Claims;
using ClubBooking.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace ClubBooking.Web.Pages.Account;

public static class CookieSignIn
{
    public static Task WithTokenAsync(HttpContext http, TokenResponse token)
    {
        var identity = new ClaimsIdentity(
            [
                new Claim("email", token.Email),
                new Claim("role", token.Role),
                new Claim("access_token", token.Token)
            ],
            CookieAuthenticationDefaults.AuthenticationScheme,
            "email",
            "role");

        return http.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = true, ExpiresUtc = token.ExpiresAt });
    }
}
