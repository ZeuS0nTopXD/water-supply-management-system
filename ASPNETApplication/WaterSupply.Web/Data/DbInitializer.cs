using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WaterSupply.Domain.Entities;
using WaterSupply.Domain.Enums;

namespace WaterSupply.Web.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;
        var context = provider.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = provider.GetRequiredService<UserManager<IdentityUser>>();
        await EnsureRoleAsync(roleManager, "Administrator");
        await EnsureRoleAsync(roleManager, "Resident");

        var admin = await EnsureUserAsync(userManager, "admin@watersupply.local", "Admin@12345", "Administrator");
        var residentUser = await EnsureUserAsync(userManager, "resident@watersupply.local", "Resident@12345", "Resident");

        if (!await context.Residents.AnyAsync())
        {
            var resident = new Resident(0, "Asha Patil", residentUser.Email!, "9876543210", "Main Road", new DateOnly(2026, 1, 1), residentUser.Id);
            context.Residents.Add(resident);
            await context.SaveChangesAsync();
            var connection = WaterConnection.Create(resident.Id, "WS-010", ConnectionType.Residential, "M-010", new DateOnly(2026, 1, 5));
            context.WaterConnections.Add(connection);
            context.MeterReadings.Add(MeterReading.Create(connection.Id, new DateOnly(2026, 9, 1), 100, 120));
            context.Bills.Add(Bill.Create(0, new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 30), 20, 5, 10, 2, new DateOnly(2026, 10, 15), connection.Id));
            context.ServiceRequests.Add(ServiceRequest.Create(resident.Id, connection.Id, ServiceRequestType.Leakage, "Leakage reported near the meter.", DateTime.UtcNow));
            await context.SaveChangesAsync();
        }

        _ = admin;
    }

    private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleManager, string role)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            var result = await roleManager.CreateAsync(new IdentityRole(role));
            if (!result.Succeeded) throw new InvalidOperationException(string.Join("; ", result.Errors.Select(error => error.Description)));
        }
    }

    private static async Task<IdentityUser> EnsureUserAsync(UserManager<IdentityUser> userManager, string email, string password, string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded) throw new InvalidOperationException(string.Join("; ", result.Errors.Select(error => error.Description)));
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            var result = await userManager.AddToRoleAsync(user, role);
            if (!result.Succeeded) throw new InvalidOperationException(string.Join("; ", result.Errors.Select(error => error.Description)));
        }

        return user;
    }
}
