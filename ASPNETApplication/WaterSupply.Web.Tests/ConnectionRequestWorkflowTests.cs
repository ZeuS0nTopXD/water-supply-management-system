using System.Net;
using System.Reflection;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WaterSupply.Domain.Entities;
using WaterSupply.Domain.Enums;
using WaterSupply.Web.Controllers;
using WaterSupply.Web.Data;
using WaterSupply.Web.Models;
using WaterSupply.Web.Models.ConnectionRequestViewModels;

namespace WaterSupply.Web.Tests;

public sealed class ConnectionRequestWorkflowTests
{
    [Fact]
    public async Task Resident_can_submit_a_connection_request_and_admin_can_update_it()
    {
        await using var db = CreateDatabase();
        db.Residents.Add(new Resident(1, "Asha Patil", "asha@example.com", "9876543210", "Main Road", new DateOnly(2026, 1, 1)));
        await db.SaveChangesAsync();

        var residentController = new ConnectionRequestsController(db)
        {
            ControllerContext = ContextFor("asha-user", "Resident")
        };

        var createResult = await residentController.Create(new ConnectionRequestCreateViewModel
        {
            RequestedType = ConnectionType.Residential,
            ServiceAddress = "New Street",
            Notes = "Please connect the new house."
        });

        createResult.Should().BeOfType<RedirectToActionResult>();
        var request = await db.WaterConnectionRequests.SingleAsync();
        request.Status.Should().Be(ConnectionRequestStatus.Pending);
        request.ResidentId.Should().Be(1);

        var adminController = new ConnectionRequestsController(db)
        {
            ControllerContext = ContextFor("admin-user", "Administrator")
        };
        var updateResult = await adminController.UpdateStatus( request.WaterConnectionRequestId,
            new ConnectionRequestStatusViewModel
            {
                Status = ConnectionRequestStatus.InReview,
                StaffNotes = "Application received and assigned for review."
            });

        updateResult.Should().BeOfType<RedirectToActionResult>();
        (await db.WaterConnectionRequests.SingleAsync()).Status.Should().Be(ConnectionRequestStatus.InReview);
        (await db.WaterConnectionRequests.SingleAsync()).StaffNotes.Should().Contain("assigned");
    }

    [Fact]
    public async Task Resident_connection_form_uses_a_property_identifier_not_the_profile_address()
    {
        await using var db = CreateDatabase();
        db.Residents.Add(new Resident(1, "Asha Patil", "asha@example.com", "9876543210", "Main Road", new DateOnly(2026, 1, 1), identityUserId: "asha-user"));
        await db.SaveChangesAsync();

        var controller = new ConnectionRequestsController(db)
        {
            ControllerContext = ContextFor("asha-user", "Resident")
        };

        var result = await controller.Create();
        var model = result.Should().BeOfType<ViewResult>().Subject.Model.Should().BeOfType<ConnectionRequestCreateViewModel>().Subject;

        model.ServiceAddress.Should().BeEmpty();
        ReadWebFile("Views", "ConnectionRequests", "Create.cshtml")
            .Should().Contain("asp-for=\"ServiceAddress\"")
            .And.Contain("e.g., Flat 4B, Block A, or Unit 12")
            .And.NotContain("Where should the connection be installed?");
    }

    [Fact]
    public async Task Resident_connection_request_list_is_scoped_to_the_signed_in_resident()
    {
        await using var db = CreateDatabase();
        db.Residents.AddRange(
            new Resident(1, "Asha Patil", "asha@example.com", "9876543210", "Main Road", new DateOnly(2026, 1, 1)),
            new Resident(2, "Ravi Shah", "ravi@example.com", "9876543211", "Lake Road", new DateOnly(2026, 1, 1)));
        db.WaterConnectionRequests.AddRange(
            new WaterConnectionRequest(1, ConnectionType.Residential, "Asha Street", "Asha request"),
            new WaterConnectionRequest(2, ConnectionType.Commercial, "Ravi Street", "Ravi request"));
        await db.SaveChangesAsync();

        var controller = new ConnectionRequestsController(db)
        {
            ControllerContext = ContextFor("asha-user", "Resident")
        };

        var result = await controller.Index();
        var model = result.Should().BeOfType<ViewResult>().Subject.Model
            .Should().BeAssignableTo<IEnumerable<ConnectionRequestListItemViewModel>>().Subject;

        model.Should().ContainSingle().Which.ServiceAddress.Should().Be("Asha Street");
    }

    [Fact]
    public async Task Duplicate_pending_request_is_rejected_with_a_friendly_validation_message()
    {
        await using var db = CreateDatabase();
        db.Residents.Add(new Resident(1, "Asha Patil", "asha@example.com", "9876543210", "Main Road", new DateOnly(2026, 1, 1)));
        db.WaterConnectionRequests.Add(new WaterConnectionRequest(1, ConnectionType.Residential, "Asha Street", "Existing request"));
        await db.SaveChangesAsync();

        var controller = new ConnectionRequestsController(db)
        {
            ControllerContext = ContextFor("asha-user", "Resident")
        };

        var result = await controller.Create(new ConnectionRequestCreateViewModel
        {
            RequestedType = ConnectionType.Residential,
            ServiceAddress = "Another Street"
        });

        result.Should().BeOfType<ViewResult>();
        controller.ModelState.Values.SelectMany(value => value.Errors)
            .Select(error => error.ErrorMessage)
            .Should().Contain(message => message!.Contains("already have", StringComparison.OrdinalIgnoreCase));
        (await db.WaterConnectionRequests.CountAsync()).Should().Be(1);
    }

    [Fact]
    public void Connection_request_permissions_are_split_between_residents_and_staff()
    {
        typeof(ConnectionRequestsController).GetMethod(nameof(ConnectionRequestsController.Create), Type.EmptyTypes)!
            .GetCustomAttributes<AuthorizeAttribute>()
            .Should().Contain(attribute => attribute.Roles == "Resident");
        typeof(ConnectionRequestsController).GetMethod(nameof(ConnectionRequestsController.Create), [typeof(ConnectionRequestCreateViewModel)])!
            .GetCustomAttributes<AuthorizeAttribute>()
            .Should().Contain(attribute => attribute.Roles == "Resident");
        typeof(ConnectionRequestsController).GetMethod(nameof(ConnectionRequestsController.UpdateStatus), [typeof(int), typeof(ConnectionRequestStatusViewModel)])!
            .GetCustomAttributes<AuthorizeAttribute>()
            .Should().Contain(attribute => attribute.Roles == "Administrator");
    }

    [Fact]
    public async Task Dashboard_counts_pending_connection_requests_for_staff_attention()
    {
        await using var db = CreateDatabase();
        db.Residents.Add(new Resident(1, "Asha Patil", "asha@example.com", "9876543210", "Main Road", new DateOnly(2026, 1, 1)));
        db.WaterConnectionRequests.Add(new WaterConnectionRequest(1, ConnectionType.Residential, "Asha Street", "New connection"));
        await db.SaveChangesAsync();

        var summary = await new WaterSupply.Web.Services.DashboardQueryService(db)
            .GetSummaryAsync(new DateOnly(2026, 9, 1));

        summary.PendingConnectionRequestCount.Should().Be(1);
    }

    [Fact]
    public void Admin_and_resident_views_explain_the_connection_request_workflow()
    {
        ReadWebFile("Views", "ResidentPortal", "Index.cshtml")
            .Should().Contain("Request a water connection")
            .And.Contain("Request a connection")
            .And.Contain("Each apartment can have one water connection");
        ReadWebFile("Views", "Shared", "_Layout.cshtml")
            .Should().Contain("Request a connection")
            .And.NotContain("My connection requests");
        ReadWebFile("Views", "ConnectionRequests", "Index.cshtml")
            .Should().Contain("Review connection requests")
            .And.Contain("Submit one connection application for your apartment")
            .And.Contain("Create connection after approval");
        ReadWebFile("Views", "Residents", "_Form.cshtml")
            .Should().Contain("register using this exact email")
            .And.Contain("Not activated");
    }

    private static ApplicationDbContext CreateDatabase()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"connection-request-{Guid.NewGuid():N}")
            .Options;
        return new ApplicationDbContext(options);
    }

    private static ControllerContext ContextFor(string userId, string role) => new()
    {
        HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                role == "Resident"
                    ? [
                        new Claim(ClaimTypes.NameIdentifier, userId),
                        new Claim(ClaimTypes.Role, role),
                        new Claim(ClaimTypes.Email, "asha@example.com")
                    ]
                    : [
                        new Claim(ClaimTypes.NameIdentifier, userId),
                        new Claim(ClaimTypes.Role, role)
                    ],
                authenticationType: "Test"))
        }
    };

    private static string ReadWebFile(params string[] segments)
    {
        var path = Path.GetFullPath(Path.Combine([AppContext.BaseDirectory, "..", "..", "..", "..", "WaterSupply.Web", .. segments]));
        return File.ReadAllText(path);
    }
}
