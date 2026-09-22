using FluentAssertions;
using WaterSupply.Application.InMemory;
using WaterSupply.Application.Services;
using WaterSupply.Domain.Entities;
using WaterSupply.Domain.Enums;
using WaterSupply.Domain.Exceptions;

namespace WaterSupply.Application.Tests;

public sealed class ServiceIntegrityTests
{
    [Fact]
    public void Connection_creation_rejects_unknown_resident()
    {
        var service = new WaterConnectionService(new InMemoryWaterSupplyStore());

        var action = () => service.Create(999, "WS-999", ConnectionType.Residential, "M-999", new DateOnly(2026, 9, 1));

        action.Should().Throw<DomainValidationException>();
    }

    [Fact]
    public void Reading_creation_rejects_unknown_connection()
    {
        var service = new MeterReadingService(new InMemoryWaterSupplyStore());

        var action = () => service.Record(999, new DateOnly(2026, 9, 1), 10, 20);

        action.Should().Throw<DomainValidationException>();
    }

    [Fact]
    public void Bill_generation_rejects_unknown_connection_or_reading()
    {
        var service = new BillingService(new InMemoryWaterSupplyStore());

        var action = () => service.Generate(999, 999, new DateOnly(2026, 9, 30), 10, 5);

        action.Should().Throw<DomainValidationException>();
    }

    [Fact]
    public void Service_request_creation_rejects_unknown_resident_or_connection()
    {
        var service = new ServiceRequestService(new InMemoryWaterSupplyStore());

        var action = () => service.Create(999, 999, RequestType.Leak, "Leak reported.");

        action.Should().Throw<DomainValidationException>();
    }

    [Fact]
    public void Updating_an_unknown_request_throws_key_not_found()
    {
        var service = new ServiceRequestService(new InMemoryWaterSupplyStore());

        var action = () => service.UpdateStatus(999, RequestStatus.Closed);

        action.Should().Throw<KeyNotFoundException>();
    }
}
