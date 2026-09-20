using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WaterSupply.Domain.Entities;
using WaterSupply.Domain.Enums;
using WaterSupply.Web.Controllers;
using WaterSupply.Web.Data;
using WaterSupply.Web.Models.ServiceRequestViewModels;

namespace WaterSupply.Web.Tests;

public sealed class ResidentOwnershipTests
{
    [Fact]
    public async Task Resident_bill_list_contains_only_the_residents_own_bills()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase($"bill-list-{Guid.NewGuid():N}").Options;
        await using var db = new ApplicationDbContext(options);
        db.Residents.AddRange(
            new Resident(1, "Resident One", "one@example.com", "111", "One Street", new DateOnly(2026, 1, 1), "identity-one"),
            new Resident(2, "Resident Two", "two@example.com", "222", "Two Street", new DateOnly(2026, 1, 1), "identity-two"));
        db.WaterConnections.AddRange(
            WaterConnection.Create(1, "WS-001", ConnectionType.Residential, "M-001", new DateOnly(2026, 1, 1), id: 1),
            WaterConnection.Create(2, "WS-002", ConnectionType.Residential, "M-002", new DateOnly(2026, 1, 1), id: 2));
        db.Bills.AddRange(
            Bill.Create(1, new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 30), 10, 5, 10, 2, new DateOnly(2026, 10, 15), 1),
            Bill.Create(2, new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 30), 10, 5, 10, 2, new DateOnly(2026, 10, 15), 2));
        await db.SaveChangesAsync();
        var controller = WithResident(new BillsController(db), "identity-one");

        var result = await controller.Index(null, null);

        var view = result.Should().BeOfType<ViewResult>().Subject;
        view.Model.Should().BeAssignableTo<IEnumerable<Bill>>().Which.Should().ContainSingle(bill => bill.WaterConnectionId == 1);
    }

    [Fact]
    public async Task Resident_cannot_create_a_request_for_another_residents_connection()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase($"request-owner-{Guid.NewGuid():N}").Options;
        await using var db = new ApplicationDbContext(options);
        db.Residents.AddRange(
            new Resident(1, "Resident One", "one@example.com", "111", "One Street", new DateOnly(2026, 1, 1), "identity-one"),
            new Resident(2, "Resident Two", "two@example.com", "222", "Two Street", new DateOnly(2026, 1, 1), "identity-two"));
        db.WaterConnections.Add(WaterConnection.Create(2, "WS-002", ConnectionType.Residential, "M-002", new DateOnly(2026, 1, 1), id: 2));
        await db.SaveChangesAsync();
        var controller = WithResident(new ServiceRequestsController(db), "identity-one");

        var result = await controller.Create(new ServiceRequestCreateViewModel
        {
            WaterConnectionId = 2,
            RequestType = ServiceRequestType.Leakage,
            Description = "Unauthorized connection request"
        });

        result.Should().BeOfType<ViewResult>();
        db.ServiceRequests.Should().BeEmpty();
    }

    private static T WithResident<T>(T controller, string identityUserId) where T : Controller
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, identityUserId),
                    new Claim(ClaimTypes.Role, "Resident")
                }, "test"))
            }
        };
        return controller;
    }
}
