using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WaterSupply.Web.Controllers;
using WaterSupply.Web.Data;
using WaterSupply.Web.Models.ResidentViewModels;

namespace WaterSupply.Web.Tests;

public class ResidentCrudTests
{
    [Fact]
    public async Task Administrator_create_adds_a_resident_to_the_database()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase($"crud-{Guid.NewGuid():N}").Options;
        await using var db = new ApplicationDbContext(options);
        var controller = new ResidentsController(db);

        var result = await controller.Create(new ResidentEditViewModel
        {
            FullName = "New Resident",
            Email = "new@example.com",
            Phone = "9999999999",
            Address = "New Street",
            RegistrationDate = new DateOnly(2026, 9, 20),
            IsActive = true
        });

        result.Should().BeOfType<Microsoft.AspNetCore.Mvc.RedirectToActionResult>();
        (await db.Residents.CountAsync()).Should().Be(1);
    }
}
