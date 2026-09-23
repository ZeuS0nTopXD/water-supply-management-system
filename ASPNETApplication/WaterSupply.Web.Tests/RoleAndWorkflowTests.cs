using System.Net;
using System.Reflection;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using WaterSupply.Domain.Entities;
using WaterSupply.Domain.Enums;
using WaterSupply.Web.Controllers;
using WaterSupply.Web.Data;
using WaterSupply.Web.Models.BillingViewModels;
using WaterSupply.Web.Models.ConnectionViewModels;
using WaterSupply.Web.Models.MeterReadingViewModels;
using WaterSupply.Web.Models.AccountViewModels;
using WaterSupply.Web.Models.ServiceRequestViewModels;

namespace WaterSupply.Web.Tests;

public sealed class RoleAndWorkflowTests
{
    [Fact]
    public void Staff_management_controllers_require_the_administrator_role()
    {
        typeof(ResidentsController).GetCustomAttribute<AuthorizeAttribute>()!.Roles.Should().Be("Administrator");
        typeof(WaterConnectionsController).GetCustomAttribute<AuthorizeAttribute>()!.Roles.Should().Be("Administrator");
        typeof(MeterReadingsController).GetCustomAttribute<AuthorizeAttribute>()!.Roles.Should().Be("Administrator");
        typeof(BillsController).GetCustomAttribute<AuthorizeAttribute>()!.Roles.Should().Be("Administrator");
        typeof(ReportsController).GetCustomAttribute<AuthorizeAttribute>()!.Roles.Should().Be("Administrator");
    }

    [Fact]
    public void Service_request_status_updates_require_the_administrator_role()
    {
        var method = typeof(ServiceRequestsController).GetMethod(
            nameof(ServiceRequestsController.UpdateStatus),
            [typeof(int), typeof(ServiceRequestStatusViewModel)]);

        method.Should().NotBeNull();
        method!.GetCustomAttribute<AuthorizeAttribute>()!.Roles.Should().Be("Administrator");
    }

    [Fact]
    public void Connections_expose_a_management_action_for_removal_or_deactivation()
    {
        typeof(WaterConnectionsController).GetMethod("Deactivate").Should().NotBeNull();
    }

    [Fact]
    public void Connections_support_editing_existing_records()
    {
        typeof(WaterConnectionsController).GetMethods()
            .Should().Contain(method => method.Name == "Edit");
    }

    [Fact]
    public async Task Editing_a_connection_updates_its_human_readable_details()
    {
        await using var db = CreateDatabase();
        db.Residents.AddRange(
            new Resident(1, "Asha Patil", "asha@example.com", "9876543210", "Main Road", new DateOnly(2026, 1, 1)),
            new Resident(2, "Ravi Shah", "ravi@example.com", "9876543211", "Lake Road", new DateOnly(2026, 1, 1)));
        db.WaterConnections.Add(new WaterConnection(1, "WS-001", ConnectionType.Residential, "M-001", new DateOnly(2026, 1, 1), id: 1));
        await db.SaveChangesAsync();

        var controller = new WaterConnectionsController(db)
        {
            ControllerContext = ContextFor("admin-user", "Administrator")
        };
        var result = await controller.Edit(1, new ConnectionEditViewModel
        {
            ResidentId = 2,
            ConnectionNumber = "WS-009",
            ConnectionType = ConnectionType.Commercial,
            MeterNumber = "M-009",
            ConnectionDate = new DateOnly(2026, 2, 1),
            Status = ConnectionStatus.Active
        });

        result.Should().BeOfType<RedirectToActionResult>();
        var connection = await db.WaterConnections.SingleAsync();
        connection.ResidentId.Should().Be(2);
        connection.ConnectionNumber.Should().Be("WS-009");
        connection.MeterNumber.Should().Be("M-009");
        connection.ConnectionType.Should().Be(ConnectionType.Commercial);
    }

    [Fact]
    public async Task Resident_request_list_is_scoped_to_the_signed_in_resident()
    {
        await using var db = CreateDatabase();
        db.Residents.AddRange(
            new Resident(1, "Asha Patil", "asha@example.com", "9876543210", "Main Road", new DateOnly(2026, 1, 1), identityUserId: "asha-user"),
            new Resident(2, "Ravi Shah", "ravi@example.com", "9876543211", "Lake Road", new DateOnly(2026, 1, 1), identityUserId: "ravi-user"));
        db.ServiceRequests.AddRange(
            new ServiceRequest(1, null, RequestType.Leak, "Asha request."),
            new ServiceRequest(2, null, RequestType.Other, "Ravi request."));
        await db.SaveChangesAsync();

        var controller = new ServiceRequestsController(db);
        controller.ControllerContext = ContextFor("asha-user", "Resident");

        var result = await controller.Index();
        var model = result.Should().BeOfType<ViewResult>().Subject.Model
            .Should().BeAssignableTo<IEnumerable<WaterSupply.Web.Models.ServiceRequestViewModels.ServiceRequestListItemViewModel>>().Subject;

        model.Should().ContainSingle();
        model.Single().ResidentName.Should().Be("Asha Patil");
    }

    [Fact]
    public async Task Resident_cannot_create_a_request_for_another_resident()
    {
        await using var db = CreateDatabase();
        db.Residents.AddRange(
            new Resident(1, "Asha Patil", "asha@example.com", "9876543210", "Main Road", new DateOnly(2026, 1, 1), identityUserId: "asha-user"),
            new Resident(2, "Ravi Shah", "ravi@example.com", "9876543211", "Lake Road", new DateOnly(2026, 1, 1), identityUserId: "ravi-user"));
        await db.SaveChangesAsync();

        var controller = new ServiceRequestsController(db);
        controller.ControllerContext = ContextFor("asha-user", "Resident");

        var result = await controller.Create(new ServiceRequestCreateViewModel
        {
            ResidentId = 2,
            RequestType = RequestType.Other,
            Description = "Attempted cross-resident request."
        });

        result.Should().BeOfType<ViewResult>();
        controller.ModelState.IsValid.Should().BeFalse();
        (await db.ServiceRequests.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Connection_without_history_can_be_removed()
    {
        await using var db = CreateDatabase();
        db.Residents.Add(new Resident(1, "Asha Patil", "asha@example.com", "9876543210", "Main Road", new DateOnly(2026, 1, 1)));
        db.WaterConnections.Add(new WaterConnection(1, "WS-001", ConnectionType.Residential, "M-001", new DateOnly(2026, 1, 1)));
        await db.SaveChangesAsync();

        var controller = CreateAdministratorConnectionsController(db);
        var result = await controller.Deactivate(1);

        result.Should().BeOfType<RedirectToActionResult>();
        (await db.WaterConnections.AnyAsync()).Should().BeFalse();
    }

    [Fact]
    public async Task Connection_with_history_is_deactivated_without_deleting_history()
    {
        await using var db = CreateDatabase();
        db.Residents.Add(new Resident(1, "Asha Patil", "asha@example.com", "9876543210", "Main Road", new DateOnly(2026, 1, 1)));
        db.WaterConnections.Add(new WaterConnection(1, "WS-001", ConnectionType.Residential, "M-001", new DateOnly(2026, 1, 1)));
        var reading = new MeterReading(1, new DateOnly(2026, 9, 1));
        reading.RecordReading(100, 120);
        db.MeterReadings.Add(reading);
        await db.SaveChangesAsync();

        await CreateAdministratorConnectionsController(db).Deactivate(1);

        var connection = await db.WaterConnections.SingleAsync();
        connection.Status.Should().Be(ConnectionStatus.Inactive);
        (await db.MeterReadings.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task Resident_portal_only_shows_records_for_the_signed_in_resident()
    {
        await using var db = CreateDatabase();
        db.Residents.AddRange(
            new Resident(1, "Asha Patil", "asha@example.com", "9876543210", "Main Road", new DateOnly(2026, 1, 1), identityUserId: "asha-user"),
            new Resident(2, "Ravi Shah", "ravi@example.com", "9876543211", "Lake Road", new DateOnly(2026, 1, 1), identityUserId: "ravi-user"));
        db.WaterConnections.AddRange(
            new WaterConnection(1, "WS-001", ConnectionType.Residential, "M-001", new DateOnly(2026, 1, 1)),
            new WaterConnection(2, "WS-002", ConnectionType.Residential, "M-002", new DateOnly(2026, 1, 1)));
        var ashaReading = new MeterReading(1, new DateOnly(2026, 9, 1));
        ashaReading.RecordReading(100, 120);
        db.MeterReadings.Add(ashaReading);
        var ashaBill = new Bill(1, 1, new DateOnly(2026, 9, 30), 20, 5);
        ashaBill.CalculateTotal();
        db.Bills.Add(ashaBill);
        db.ServiceRequests.AddRange(
            new ServiceRequest(1, 1, RequestType.Leak, "Asha request."),
            new ServiceRequest(2, 2, RequestType.Other, "Ravi request."));
        await db.SaveChangesAsync();

        var controller = new ResidentPortalController(db);
        controller.ControllerContext = ContextFor("asha-user", "Resident");

        var result = await controller.Index();
        var model = result.Should().BeOfType<ViewResult>().Subject.Model
            .Should().BeOfType<WaterSupply.Web.Models.ResidentPortalViewModel>().Subject;

        model.ResidentName.Should().Be("Asha Patil");
        model.Connections.Should().ContainSingle().Which.ConnectionNumber.Should().Be("WS-001");
        model.Readings.Should().ContainSingle().Which.ConnectionNumber.Should().Be("WS-001");
        model.Bills.Should().ContainSingle().Which.ConnectionNumber.Should().Be("WS-001");
        model.ServiceRequests.Should().ContainSingle();
    }

    [Fact]
    public async Task Resident_dashboard_redirects_to_the_resident_portal()
    {
        await using var db = CreateDatabase();
        var controller = new DashboardController(new WaterSupply.Web.Services.DashboardQueryService(db))
        {
            ControllerContext = ContextFor("resident-user", "Resident")
        };

        var result = await controller.Index();

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ControllerName.Should().Be("ResidentPortal");
        redirect.ActionName.Should().Be(nameof(ResidentPortalController.Index));
    }

    [Fact]
    public async Task Inactive_connections_are_not_offered_for_new_transactions()
    {
        await using var db = CreateDatabase();
        db.Residents.Add(new Resident(1, "Asha Patil", "asha@example.com", "9876543210", "Main Road", new DateOnly(2026, 1, 1)));
        var active = new WaterConnection(1, "WS-001", ConnectionType.Residential, "M-001", new DateOnly(2026, 1, 1));
        var inactive = new WaterConnection(1, "WS-002", ConnectionType.Residential, "M-002", new DateOnly(2026, 1, 1));
        inactive.ChangeStatus(ConnectionStatus.Inactive);
        db.WaterConnections.AddRange(active, inactive);
        await db.SaveChangesAsync();

        var readingResult = await new MeterReadingsController(db).Create();
        var billResult = await new BillsController(db).Create();

        var readings = readingResult.Should().BeOfType<ViewResult>().Subject.ViewData["Connections"]
            .Should().BeAssignableTo<IEnumerable<WaterConnection>>().Subject;
        var bills = billResult.Should().BeOfType<ViewResult>().Subject.ViewData["Connections"]
            .Should().BeAssignableTo<IEnumerable<WaterConnection>>().Subject;
        readings.Should().ContainSingle().Which.ConnectionNumber.Should().Be("WS-001");
        bills.Should().ContainSingle().Which.ConnectionNumber.Should().Be("WS-001");
    }

    [Fact]
    public async Task Inactive_connections_are_rejected_for_new_readings_bills_and_requests()
    {
        await using var db = CreateDatabase();
        db.Residents.Add(new Resident(1, "Asha Patil", "asha@example.com", "9876543210", "Main Road", new DateOnly(2026, 1, 1)));
        var inactive = new WaterConnection(1, "WS-002", ConnectionType.Residential, "M-002", new DateOnly(2026, 1, 1), id: 2);
        inactive.ChangeStatus(ConnectionStatus.Inactive);
        db.WaterConnections.Add(inactive);
        var reading = new MeterReading(2, new DateOnly(2026, 9, 1), id: 2);
        reading.RecordReading(100, 120);
        db.MeterReadings.Add(reading);
        await db.SaveChangesAsync();

        var readingController = new MeterReadingsController(db);
        var readingResult = await readingController.Create(new MeterReadingEditViewModel
        {
            WaterConnectionId = 2,
            ReadingDate = new DateOnly(2026, 10, 1),
            PreviousReading = 120,
            CurrentReading = 140
        });

        var billController = new BillsController(db);
        var billResult = await billController.Create(new BillCreateViewModel
        {
            WaterConnectionId = 2,
            MeterReadingId = 2,
            BillDate = new DateOnly(2026, 10, 1),
            UnitsConsumed = 20,
            RatePerUnit = 5
        });

        var requestController = new ServiceRequestsController(db)
        {
            ControllerContext = ContextFor("admin-user", "Administrator")
        };
        var requestResult = await requestController.Create(new ServiceRequestCreateViewModel
        {
            ResidentId = 1,
            WaterConnectionId = 2,
            RequestType = RequestType.Leak,
            Description = "Request on an inactive connection."
        });

        readingResult.Should().BeOfType<ViewResult>();
        readingController.ModelState.IsValid.Should().BeFalse();
        billResult.Should().BeOfType<ViewResult>();
        billController.ModelState.IsValid.Should().BeFalse();
        requestResult.Should().BeOfType<ViewResult>();
        requestController.ModelState.IsValid.Should().BeFalse();
        (await db.MeterReadings.CountAsync()).Should().Be(1);
        (await db.Bills.CountAsync()).Should().Be(0);
        (await db.ServiceRequests.CountAsync()).Should().Be(0);
    }

    [Fact]
    public void User_facing_pages_use_connection_numbers_instead_of_database_ids()
    {
        ReadWebFile("Views", "Bills", "Index.cshtml")
            .Should().Contain("Connection number")
            .And.NotContain("Connection ID")
            .And.NotContain("@bill.WaterConnectionId");
        ReadWebFile("Views", "MeterReadings", "Index.cshtml")
            .Should().Contain("Connection number")
            .And.NotContain("Connection ID")
            .And.NotContain("@reading.WaterConnectionId");
        ReadWebFile("Views", "Reports", "Index.cshtml")
            .Should().Contain("Connection number")
            .And.NotContain("Connection ID")
            .And.NotContain("@row.WaterConnectionId");
    }

    [Fact]
    public void Sql_demo_seed_matches_the_schema_constraints()
    {
        var seed = ReadRepositoryFile("Database", "SQLScripts", "004_SeedDemoData.sql");

        seed.Should().NotContain("N'Leakage'");
        seed.Should().Contain("Consumption");
    }

    [Fact]
    public void Resident_dashboard_has_a_separate_portal_controller()
    {
        Type.GetType("WaterSupply.Web.Controllers.ResidentPortalController, WaterSupply.Web").Should().NotBeNull();
    }

    [Fact]
    public void Web_host_uses_explicit_console_logging_instead_of_the_windows_event_log_provider()
    {
        var program = ReadWebFile("Program.cs");

        program.Should().Contain("builder.Logging.ClearProviders()");
        program.Should().Contain("AddConsole()");
    }

    [Fact]
    public void Unauthorized_role_requests_use_a_friendly_access_denied_page()
    {
        var program = ReadWebFile("Program.cs");

        program.Should().Contain("AccessDeniedPath = \"/Home/AccessDenied\"");
        typeof(HomeController).GetMethod(nameof(HomeController.AccessDenied)).Should().NotBeNull();
        ReadWebFile("Views", "Home", "AccessDenied.cshtml")
            .Should().Contain("Access denied");
    }

    [Fact]
    public void Residents_have_a_resident_only_profile_edit_workflow()
    {
        var method = typeof(AccountController).GetMethod("Profile", [typeof(ResidentProfileViewModel)]);

        method.Should().NotBeNull();
        method!.GetCustomAttributes<AuthorizeAttribute>().Should().Contain(attribute => attribute.Roles == "Resident");
        ReadWebFile("Views", "Account", "Profile.cshtml")
            .Should().Contain("Update profile");
    }

    [Fact]
    public void Only_residents_can_raise_service_requests()
    {
        var getMethod = typeof(ServiceRequestsController).GetMethod("Create", Type.EmptyTypes);
        var postMethod = typeof(ServiceRequestsController).GetMethod("Create", [typeof(ServiceRequestCreateViewModel)]);

        getMethod.Should().NotBeNull();
        postMethod.Should().NotBeNull();
        getMethod!.GetCustomAttributes<AuthorizeAttribute>().Should().Contain(attribute => attribute.Roles == "Resident");
        postMethod!.GetCustomAttributes<AuthorizeAttribute>().Should().Contain(attribute => attribute.Roles == "Resident");
    }

    [Fact]
    public void Admin_dashboard_keeps_request_work_in_the_review_area()
    {
        var dashboard = ReadWebFile("Views", "Dashboard", "Index.cshtml");

        dashboard.Should().NotContain("asp-controller=\"ServiceRequests\" asp-action=\"Create\"")
            .And.Contain("Review service requests")
            .And.Contain("Recommended workflow");
    }

    [Fact]
    public void Resident_portal_explains_when_a_connection_is_not_assigned()
    {
        ReadWebFile("Views", "ResidentPortal", "Index.cshtml")
            .Should().Contain("administrator must assign a water connection")
            .And.Contain("Report a service issue");
    }

    [Fact]
    public void Service_request_list_only_offers_creation_to_residents()
    {
        var requests = ReadWebFile("Views", "ServiceRequests", "Index.cshtml");

        requests.Should().Contain("User.IsInRole(\"Resident\")")
            .And.Contain("Report a service issue")
            .And.NotContain(">Add request</a>");
    }

    [Fact]
    public void Registration_explains_that_connections_are_assigned_after_signup()
    {
        ReadWebFile("Views", "Account", "Register.cshtml")
            .Should().Contain("administrator")
            .And.Contain("water connection");
    }

    private static ApplicationDbContext CreateDatabase()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"role-workflow-{Guid.NewGuid():N}")
            .Options;
        return new ApplicationDbContext(options);
    }

    private static ControllerContext ContextFor(string userId, string role) => new()
    {
        HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, userId),
                    new Claim(ClaimTypes.Role, role)
                ],
                authenticationType: "Test"))
        }
    };

    private static WaterConnectionsController CreateAdministratorConnectionsController(ApplicationDbContext db)
    {
        var controller = new WaterConnectionsController(db)
        {
            ControllerContext = ContextFor("admin-user", "Administrator"),
            TempData = new TempDataDictionary(new DefaultHttpContext(), new TestTempDataProvider())
        };
        return controller;
    }

    private static string ReadWebFile(params string[] segments)
    {
        var path = Path.GetFullPath(Path.Combine([AppContext.BaseDirectory, "..", "..", "..", "..", "WaterSupply.Web", .. segments]));
        return File.ReadAllText(path);
    }

    private sealed class TestTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object?> LoadTempData(HttpContext context) => new Dictionary<string, object?>();

        public void SaveTempData(HttpContext context, IDictionary<string, object?> values)
        {
        }
    }

    private static string ReadRepositoryFile(params string[] segments)
    {
        var path = Path.GetFullPath(Path.Combine([AppContext.BaseDirectory, "..", "..", "..", "..", "..", .. segments]));
        return File.ReadAllText(path);
    }
}
