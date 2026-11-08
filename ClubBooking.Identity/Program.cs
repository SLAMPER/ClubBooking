using ClubBooking.Identity.Data;
using ClubBooking.Identity.Models;
using ClubBooking.Identity.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<UsersDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddHealthChecks()
    .AddDbContextCheck<UsersDbContext>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<TokenService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
    db.Database.Migrate();
    SeedData.Seed(db, scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>());
}

app.MapOpenApi();
app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "ClubBooking.Identity"));

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
