using GabsHybridApp.Shared.Data;
using GabsHybridApp.Shared.Services;
using GabsHybridApp.Web.Components;
using GabsHybridApp.Web.Extensions;
using GabsHybridApp.Web.Services;
using Microsoft.EntityFrameworkCore;
using GabsHybridApp.Shared;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSharedCore();

builder.Services.AddDbContextFactory<HybridAppDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"), builder.Environment.ContentRootPath));

// Add device-specific services used by the GabsHybridApp.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();

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

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(GabsHybridApp.Shared._Imports).Assembly);

app.Run();
