using System.Net;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WaterSupply.Domain.Entities;
using WaterSupply.Domain.Enums;
using WaterSupply.Web.Controllers;
using WaterSupply.Web.Data;

namespace WaterSupply.Web.Tests;

public class AuthorizationTests
{
    [Fact]
    public async Task Anonymous_user_is_redirected_to_login_for_dashboard()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync("/Dashboard");

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location!.AbsolutePath.Should().Contain("/Account/Login");
    }

    [Fact]
    public async Task Resident_cannot_read_another_residents_bill()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase($"ownership-{Guid.NewGuid():N}").Options;
        await using var db = new ApplicationDbContext(options);
        db.Residents.AddRange(
            new Resident(1, "Resident One", "one@example.com", "111", "One Street", new DateOnly(2026, 1, 1), "identity-one"),
            new Resident(2, "Resident Two", "two@example.com", "222", "Two Street", new DateOnly(2026, 1, 1), "identity-two"));
        db.WaterConnections.Add(WaterConnection.Create(2, "WS-002", ConnectionType.Residential, "M-002", new DateOnly(2026, 1, 1), id: 2));
        db.Bills.Add(Bill.Create(2, new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 30), 10, 5, 10, 2, new DateOnly(2026, 10, 15), 2));
        await db.SaveChangesAsync();
        var controller = new ResidentPortalController(db)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "identity-one") }, "test"))
                }
            }
        };

        var result = await controller.Bill(2);

        result.Should().BeOfType<ForbidResult>();
    }
}
