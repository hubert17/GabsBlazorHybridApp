using GabsHybridApp.Shared;
using GabsHybridApp.Shared.Data;
using GabsHybridApp.Shared.Services;
using GabsHybridApp.Web.Components;
using GabsHybridApp.Web.Endpoints;
using GabsHybridApp.Web.Extensions;
using GabsHybridApp.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSharedCore();

builder.Services.AddHttpContextAccessor();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o => { o.LoginPath = "/account/login"; o.LogoutPath = "/account/logout"; })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opt =>
    {
        opt.RequireHttpsMetadata = true;
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ClockSkew = TimeSpan.Zero
        };
    });

// Authorization
builder.Services.AddAuthorization(options =>
{
    // Only API JWT (Bearer) + role MobileSync can access sync endpoints
    options.AddPolicy("SyncAccess", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole("MobileSync");
        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
    });
});

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddDbContextFactory<HybridAppDbContext>(option =>
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), sqlServer => sqlServer.MigrationsAssembly("GabsHybridApp.Web")));
    //option.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnectionSqlite"), builder.Environment.ContentRootPath));

// Add device-specific services used by the GabsHybridApp.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();
builder.Services.AddScoped<IHostCapabilities, WebHostCapabilities>();
builder.Services.AddScoped<IAuthService, ServerCookieAuthService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ILocationService, WebLocationService>();
builder.Services.AddScoped<ICameraService, WebCameraService>();
builder.Services.AddSingleton<IFlashlightService, WebFlashlightService>();

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<INonceCache, MemoryNonceCache>();
builder.Services.AddSingleton<ApiJwtIssuer>();

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

app.MapStaticAssets();
// Only keep this if you need a custom folder or options:
//app.UseStaticFiles(new StaticFileOptions
//{
//    FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "uploads")),
//    RequestPath = "/uploads"
//});

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

app.MapAuthExchangeEndpoints();
app.MapSyncEndpoints();

app.MapHub<NotificationHub>("/notificationhub");

app.UseWhen(ctx => !ctx.Request.Path.StartsWithSegments("/api"), then =>
{
    then.UseStatusCodePagesWithRedirects("/404");
});

app.Run();
