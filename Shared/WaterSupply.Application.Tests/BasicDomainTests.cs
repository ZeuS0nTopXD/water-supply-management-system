using FluentAssertions;
using WaterSupply.Domain.Entities;
using WaterSupply.Domain.Enums;
using WaterSupply.Domain.Exceptions;
using WaterSupply.Application.InMemory;
using WaterSupply.Application.Services;

namespace WaterSupply.Application.Tests;

public class BasicDomainTests
{
    [Fact]
    public void Reading_rejects_current_value_below_previous_value()
    {
        var reading = new MeterReading(1, DateOnly.FromDateTime(DateTime.Today));

        Assert.Throws<DomainValidationException>(() => reading.RecordReading(100, 90));
    }

    [Fact]
    public void Bill_calculates_total_from_units_and_rate()
    {
        var bill = new Bill(1, 1, DateOnly.FromDateTime(DateTime.Today), 12, 25m);

        bill.CalculateTotal();

        bill.TotalAmount.Should().Be(300m);
    }

    [Fact]
    public void Resident_search_matches_name_without_case_sensitivity()
    {
        var service = new ResidentService(new InMemoryWaterSupplyStore
        {
            Residents =
            {
                new Resident(1, "Asha Kumar", "asha@example.com", "9999999999", "Main Road", DateOnly.FromDateTime(DateTime.Today))
            }
        });

        service.Search("asha").Should().ContainSingle();
    }

    [Fact]
    public void Summary_counts_only_open_service_requests()
    {
        var store = new InMemoryWaterSupplyStore();
        store.ServiceRequests.Add(new ServiceRequest(1, null, RequestType.Leak, "Tap leak"));
        var closed = new ServiceRequest(1, null, RequestType.Other, "Closed repair");
        closed.ChangeStatus(RequestStatus.Closed, "Closed for test");
        store.ServiceRequests.Add(closed);

        new ReportService(store).GetSummary().OpenRequests.Should().Be(1);
    }
}
