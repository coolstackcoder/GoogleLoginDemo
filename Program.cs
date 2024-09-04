using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddGoogle(options =>
    {
        options.ClientId = "YOUR_GOOGLE_CLIENT_ID";
        options.ClientSecret = "YOUR_GOOGLE_CLIENT_SECRET";
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", (HttpContext context) =>
{
    return context.User.Identity?.IsAuthenticated ?? false
        ? Results.Content($"Hello, {context.User.Identity.Name}! <a href='/logout'>Logout</a>", contentType: "text/html")
        : Results.Content("Welcome! <a href='/login'>Login with Google</a>", contentType: "text/html");
});

app.MapGet("/secure", (HttpContext context) =>
{
    return context.User.Identity?.IsAuthenticated ?? false
        ? Results.Content($"This is a secure page. Welcome, {context.User.Identity.Name}! <a href='/logout'>Logout</a>", contentType: "text/html")
        : Results.Challenge(new AuthenticationProperties { RedirectUri = "/secure" });
}).RequireAuthorization();

app.MapGet("/login", () => Results.Challenge(
    new AuthenticationProperties { RedirectUri = "/api/hello" }, 
    new[] { GoogleDefaults.AuthenticationScheme }));

app.MapGet("/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/");
});

app.MapGet("/api/hello", () => new { Message = "Hello, World!" });

app.Run();