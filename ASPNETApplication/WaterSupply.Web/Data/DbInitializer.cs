using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace WaterSupply.Web.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;
        var context = provider.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureCreatedAsync();

        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = provider.GetRequiredService<UserManager<IdentityUser>>();
        await EnsureRoleAsync(roleManager, "Administrator");
        await EnsureRoleAsync(roleManager, "Resident");

        var admin = await EnsureUserAsync(userManager, "admin@watersupply.local", "Admin@12345", "Administrator");
        var residentUser = await EnsureUserAsync(userManager, "resident@watersupply.local", "Resident@12345", "Resident");

        await DemoDataSeeder.SeedAsync(context, residentUser.Id);

        _ = admin;
    }

    private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleManager, string role)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            var result = await roleManager.CreateAsync(new IdentityRole(role));
            EnsureSucceeded(result);
        }
    }

    private static async Task<IdentityUser> EnsureUserAsync(UserManager<IdentityUser> userManager, string email, string password, string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
            var result = await userManager.CreateAsync(user, password);
            EnsureSucceeded(result);
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            var result = await userManager.AddToRoleAsync(user, role);
            EnsureSucceeded(result);
        }

        return user;
    }

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(error => error.Description)));
        }
    }
}
