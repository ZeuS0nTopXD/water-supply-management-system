using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WaterSupply.Domain.Entities;
using WaterSupply.Domain.Enums;
using WaterSupply.Web.Controllers;
using WaterSupply.Web.Data;
using WaterSupply.Web.Models.ServiceRequestViewModels;

namespace WaterSupply.Web.Tests;

public sealed class ServiceRequestListTests
{
    [Fact]
    public async Task Request_list_uses_resident_names_and_readable_labels()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"requests-{Guid.NewGuid():N}")
            .Options;
        await using var db = new ApplicationDbContext(options);
        var resident = new Resident(1, "Asha Patil", "asha@example.com", "9876543210", "Main Road", new DateOnly(2026, 1, 1));
        db.Residents.Add(resident);
        db.ServiceRequests.Add(new ServiceRequest(1, null, RequestType.NoSupply, "No supply since yesterday."));
        await db.SaveChangesAsync();

        var result = await new ServiceRequestsController(db).Index();

        var model = result.Should().BeOfType<ViewResult>().Subject.Model
            .Should().BeAssignableTo<IEnumerable<ServiceRequestListItemViewModel>>().Subject
            .Should().ContainSingle().Which;
        model.ResidentName.Should().Be("Asha Patil");
        model.RequestTypeLabel.Should().Be("No supply");
        model.StatusLabel.Should().Be("Open");
    }
}
