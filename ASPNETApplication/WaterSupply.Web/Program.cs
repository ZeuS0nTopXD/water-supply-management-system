using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WaterSupply.Web.Data;
using WaterSupply.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Keep hosting diagnostics portable. The Windows Event Log provider can throw
// when the process does not have permission to write to the machine event log.
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' not found.");

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (builder.Environment.IsEnvironment("Testing"))
    {
        options.UseInMemoryDatabase("WaterSupplyPreview");
    }
    else
    {
        options.UseSqlServer(connectionString);
    }
});

// Persist Data Protection keys in the database.
// This keeps antiforgery and authentication cookies valid
// across different/restarted deployment instances.
builder.Services.AddDataProtection()
    .PersistKeysToDbContext<ApplicationDbContext>()
    .SetApplicationName("WaterSupplyManagementSystem");

// ASP.NET Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Home/AccessDenied";
});

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<DashboardQueryService>();

var app = builder.Build();

// Production error handling
if (!app.Environment.IsDevelopment() &&
    !app.Environment.IsEnvironment("Testing"))
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed database
var seedPreviewData = string.Equals(
    Environment.GetEnvironmentVariable("WATER_SUPPLY_DEMO_DATA"),
    "true",
    StringComparison.OrdinalIgnoreCase);

if (!app.Environment.IsEnvironment("Testing") || seedPreviewData)
{
    await DbInitializer.InitializeAsync(app.Services);
}

app.Run();

public partial class Program
{
}
