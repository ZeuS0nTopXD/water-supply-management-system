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
        await context.Database.EnsureCreatedAsync();

        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = provider.GetRequiredService<UserManager<IdentityUser>>();
        await EnsureRoleAsync(roleManager, "Administrator");
        await EnsureRoleAsync(roleManager, "Resident");

        var admin = await EnsureUserAsync(userManager, "admin@watersupply.local", "Admin@12345", "Administrator");
        var residentUser = await EnsureUserAsync(userManager, "resident@watersupply.local", "Resident@12345", "Resident");

        if (!await context.Residents.AnyAsync())
        {
            var resident = new Resident(0, "Asha Patil", residentUser.Email!, "9876543210", "Main Road", new DateOnly(2026, 1, 1), identityUserId: residentUser.Id);
            context.Residents.Add(resident);
            await context.SaveChangesAsync();

            var connection = new WaterConnection(resident.ResidentId, "WS-010", ConnectionType.Residential, "M-010", new DateOnly(2026, 1, 5));
            context.WaterConnections.Add(connection);
            await context.SaveChangesAsync();

            var reading = new MeterReading(connection.WaterConnectionId, new DateOnly(2026, 9, 1));
            reading.RecordReading(100, 120);
            context.MeterReadings.Add(reading);
            await context.SaveChangesAsync();

            var bill = new Bill(connection.WaterConnectionId, reading.MeterReadingId, new DateOnly(2026, 9, 30), (int)reading.Consumption, 5m);
            bill.CalculateTotal();
            context.Bills.Add(bill);
            context.ServiceRequests.Add(new ServiceRequest(resident.ResidentId, connection.WaterConnectionId, RequestType.Leak, "Leak reported near the meter."));
            await context.SaveChangesAsync();
        }

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
