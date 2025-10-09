using GabsHybridApp.Shared;
using GabsHybridApp.Shared.Data;
using GabsHybridApp.Shared.Services;
using GabsHybridApp.Web.Components;
using GabsHybridApp.Web.Extensions;
using GabsHybridApp.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSharedCore();

builder.Services.AddHttpContextAccessor();
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/account/login";
        o.LogoutPath = "/account/logout";
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

// NOTE: If you switch Sqlite ↔ SqlServer, delete the existing Migrations folder before running Add-Migration
builder.Services.AddDbContextFactory<HybridAppDbContext>(option =>
    option.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnectionSqlite"), builder.Environment.ContentRootPath));
    //option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add device-specific services used by the GabsHybridApp.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();
builder.Services.AddScoped<IHostCapabilities, WebHostCapabilities>();
builder.Services.AddScoped<IAuthService, ServerCookieAuthService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ILocationService, WebLocationService>();
builder.Services.AddScoped<ICameraService, WebCameraService>();
builder.Services.AddSingleton<IFlashlightService, WebFlashlightService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.MigrateDb<HybridAppDbContext>(true);

app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication(); 
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(GabsHybridApp.Shared._Imports).Assembly);

app.MapPost("/_internal/logout", async (HttpContext ctx) =>
{
    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/account/login");
}).AllowAnonymous().DisableAntiforgery();

app.MapHub<NotificationHub>("/notificationhub");

app.UseStatusCodePagesWithRedirects("/404");

app.Run();
