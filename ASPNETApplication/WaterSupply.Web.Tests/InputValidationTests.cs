using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WaterSupply.Domain.Enums;
using WaterSupply.Domain.Entities;
using WaterSupply.Web.Controllers;
using WaterSupply.Web.Data;
using WaterSupply.Web.Models.BillingViewModels;
using WaterSupply.Web.Models.ConnectionViewModels;
using WaterSupply.Web.Models.MeterReadingViewModels;
using WaterSupply.Web.Models.ServiceRequestViewModels;

namespace WaterSupply.Web.Tests;

public sealed class InputValidationTests
{
    [Fact]
    public async Task Bill_creation_rejects_unknown_connection_or_reading()
    {
        await using var db = CreateDatabase();
        var controller = new BillsController(db);

        var result = await controller.Create(new BillCreateViewModel
        {
            WaterConnectionId = 999,
            MeterReadingId = 999,
            BillDate = new DateOnly(2026, 9, 30),
            UnitsConsumed = 10,
            RatePerUnit = 5
        });

        result.Should().BeOfType<ViewResult>();
        controller.ModelState.IsValid.Should().BeFalse();
        controller.ModelState.Values.SelectMany(value => value.Errors)
            .Select(error => error.ErrorMessage)
            .Should().Contain(message => message!.Contains("connection", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Meter_reading_creation_rejects_unknown_connection()
    {
        await using var db = CreateDatabase();
        var controller = new MeterReadingsController(db);

        var result = await controller.Create(new MeterReadingEditViewModel
        {
            WaterConnectionId = 999,
            ReadingDate = new DateOnly(2026, 9, 1),
            PreviousReading = 10,
            CurrentReading = 20
        });

        result.Should().BeOfType<ViewResult>();
        controller.ModelState.IsValid.Should().BeFalse();
        (await db.MeterReadings.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Connection_creation_rejects_unknown_resident()
    {
        await using var db = CreateDatabase();
        var controller = new WaterConnectionsController(db);

        var result = await controller.Create(new ConnectionEditViewModel
        {
            ResidentId = 999,
            ConnectionNumber = "WS-999",
            ConnectionType = ConnectionType.Residential,
            MeterNumber = "M-999",
            ConnectionDate = new DateOnly(2026, 9, 1)
        });

        result.Should().BeOfType<ViewResult>();
        controller.ModelState.IsValid.Should().BeFalse();
        (await db.WaterConnections.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Service_request_creation_rejects_unknown_resident_or_connection()
    {
        await using var db = CreateDatabase();
        var controller = new ServiceRequestsController(db);

        var result = await controller.Create(new ServiceRequestCreateViewModel
        {
            ResidentId = 999,
            WaterConnectionId = 999,
            RequestType = RequestType.Leak,
            Description = "Leak reported."
        });

        result.Should().BeOfType<ViewResult>();
        controller.ModelState.IsValid.Should().BeFalse();
        (await db.ServiceRequests.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Resident_delete_keeps_related_resident_and_returns_a_safe_message()
    {
        await using var db = CreateDatabase();
        db.Residents.Add(new Resident(1, "Asha Patil", "asha@example.com", "9876543210", "Main Road", new DateOnly(2026, 1, 1)));
        db.WaterConnections.Add(new WaterConnection(1, "WS-001", ConnectionType.Residential, "M-001", new DateOnly(2026, 1, 1), 1));
        await db.SaveChangesAsync();

        var controller = new ResidentsController(db);
        var result = await controller.Delete(1);

        result.Should().BeOfType<RedirectToActionResult>();
        var redirect = (RedirectToActionResult)result;
        redirect.RouteValues!["error"].Should().Be("Remove related connections and requests before deleting this resident.");
        (await db.Residents.AnyAsync(resident => resident.ResidentId == 1)).Should().BeTrue();
    }

    private static ApplicationDbContext CreateDatabase()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"input-validation-{Guid.NewGuid():N}")
            .Options;
        return new ApplicationDbContext(options);
    }
}
