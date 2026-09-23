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

        // Database schema already exists in production.
        // Do not call EnsureCreatedAsync() here because multiple
        // Vercel instances may try to create the schema simultaneously.

        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = provider.GetRequiredService<UserManager<IdentityUser>>();

        await EnsureRoleAsync(roleManager, "Administrator");
        await EnsureRoleAsync(roleManager, "Resident");

        var admin = await EnsureUserAsync(
            userManager,
            "admin@watersupply.local",
            "Admin@12345",
            "Administrator");

        var residentUser = await EnsureUserAsync(
            userManager,
            "resident@watersupply.local",
            "Resident@12345",
            "Resident");

        await LinkResidentProfileAsync(context, residentUser);
        await DemoDataSeeder.SeedAsync(context, residentUser.Id);

        _ = admin;
    }

    private static async Task EnsureRoleAsync(
        RoleManager<IdentityRole> roleManager,
        string role)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            var result = await roleManager.CreateAsync(new IdentityRole(role));

            // Another instance may have created the role first.
            if (!result.Succeeded &&
                !await roleManager.RoleExistsAsync(role))
            {
                EnsureSucceeded(result);
            }
        }
    }

    private static async Task<IdentityUser> EnsureUserAsync(
        UserManager<IdentityUser> userManager,
        string email,
        string password,
        string role)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
        {
            var newUser = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(newUser, password);

            if (result.Succeeded)
            {
                user = newUser;
            }
            else
            {
                // Another instance may have created the user first.
                user = await userManager.FindByEmailAsync(email);

                if (user is null)
                {
                    EnsureSucceeded(result);
                }
            }
        }

        if (!await userManager.IsInRoleAsync(user!, role))
        {
            var result = await userManager.AddToRoleAsync(user!, role);

            if (!result.Succeeded &&
                !await userManager.IsInRoleAsync(user!, role))
            {
                EnsureSucceeded(result);
            }
        }

        return user!;
    }

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join("; ",
                    result.Errors.Select(error => error.Description)));
        }
    }

    private static async Task LinkResidentProfileAsync(
        ApplicationDbContext context,
        IdentityUser residentUser)
    {
        if (await context.Residents.AnyAsync(resident => resident.IdentityUserId == residentUser.Id)) return;

        var profile = await context.Residents
            .Where(resident => resident.IdentityUserId == null
                && resident.Email.ToLower() == residentUser.Email!.ToLower())
            .FirstOrDefaultAsync();
        if (profile is null) return;

        profile.LinkIdentityUser(residentUser.Id);
        await context.SaveChangesAsync();
    }
}
