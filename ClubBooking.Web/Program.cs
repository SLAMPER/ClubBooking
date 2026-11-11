using System.Globalization;
using ClubBooking.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin", "admin");
    options.Conventions.AuthorizeFolder("/Bookings", "client");
});
builder.Services.AddHealthChecks();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("admin", policy => policy.RequireRole("admin"))
    .AddPolicy("client", policy => policy.RequireRole("client"));
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<BearerTokenHandler>();
foreach (var name in new[] { "identity", "seats", "bookings" })
{
    builder.Services.AddHttpClient(name, client => client.BaseAddress = new Uri(builder.Configuration[$"Services:{name}"]!))
        .AddHttpMessageHandler<BearerTokenHandler>();
}
builder.Services.AddScoped<ClubApi>();
builder.Services.AddSingleton<ClubTime>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();
app.MapHealthChecks("/health");

app.Run();
