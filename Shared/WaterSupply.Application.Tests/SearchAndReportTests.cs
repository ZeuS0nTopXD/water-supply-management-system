using FluentAssertions;
using WaterSupply.Application.InMemory;
using WaterSupply.Application.Services;
using WaterSupply.Domain.Entities;
using WaterSupply.Domain.Enums;

namespace WaterSupply.Application.Tests;

public class SearchAndReportTests
{
    [Fact]
    public void Resident_search_is_case_insensitive_and_matches_name_or_phone()
    {
        var repository = new InMemoryRepository<Resident>(resident => resident.Id);
        repository.Add(new Resident(1, "Asha Patil", "asha@example.com", "9876543210", "Main Road", new DateOnly(2026, 1, 1)));
        var service = new ResidentService(repository);

        service.Search("asha").Should().ContainSingle(resident => resident.FullName == "Asha Patil");
        service.Search("9876543210").Should().ContainSingle(resident => resident.FullName == "Asha Patil");
    }

    [Fact]
    public void Resident_bill_query_returns_only_bills_for_the_authenticated_resident()
    {
        var residents = new InMemoryRepository<Resident>(resident => resident.Id);
        var connections = new InMemoryRepository<WaterConnection>(connection => connection.Id);
        var bills = new InMemoryRepository<Bill>(bill => bill.Id);
        var payments = new InMemoryRepository<Payment>(payment => payment.Id);
        residents.Add(new Resident(10, "Asha Patil", "asha@example.com", "9876543210", "Main Road", new DateOnly(2026, 1, 1)));
        residents.Add(new Resident(20, "Ravi Shah", "ravi@example.com", "9876543211", "Lake Road", new DateOnly(2026, 1, 1)));
        connections.Add(WaterConnection.Create(10, "WS-010", ConnectionType.Residential, "M-010", new DateOnly(2026, 1, 1), id: 100));
        connections.Add(WaterConnection.Create(20, "WS-020", ConnectionType.Residential, "M-020", new DateOnly(2026, 1, 1), id: 200));
        bills.Add(Bill.Create(1000, new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 30), 10, 20, 10, 2, new DateOnly(2026, 10, 15), waterConnectionId: 100));
        bills.Add(Bill.Create(2000, new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 30), 10, 20, 10, 2, new DateOnly(2026, 10, 15), waterConnectionId: 200));
        var service = new BillingService(bills, payments, connections);

        service.GetBillsForResident(10).Should().OnlyContain(bill => bill.WaterConnectionId == 100);
    }
}
