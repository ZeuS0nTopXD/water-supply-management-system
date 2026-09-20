using FluentAssertions;
using WaterSupply.Domain.Entities;
using WaterSupply.Domain.Enums;
using WaterSupply.Domain.Exceptions;
using WaterSupply.Application.InMemory;

namespace WaterSupply.Application.Tests;

public class DomainValidationTests
{
    [Fact]
    public void MeterReading_rejects_current_value_below_previous_value()
    {
        Action act = () => MeterReading.Create(1, new DateOnly(2026, 9, 1), 120, 119);

        act.Should().Throw<DomainValidationException>();
    }

    [Fact]
    public void Bill_rejects_payment_above_outstanding_balance_and_keeps_unpaid_status()
    {
        var bill = Bill.Create(1, new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 30), 10, 20, 10, 2, new DateOnly(2026, 10, 15));

        Action act = () => bill.RecordPayment(500, PaymentMethod.Cash, new DateTime(2026, 9, 15));

        act.Should().Throw<DomainValidationException>();
        bill.Status.Should().Be(BillStatus.Unpaid);
        bill.PaidAmount.Should().Be(0);
    }

    [Fact]
    public void In_memory_reading_repository_rejects_duplicate_reading_date_for_connection()
    {
        var repository = new InMemoryMeterReadingRepository();
        repository.Add(MeterReading.Create(1, new DateOnly(2026, 9, 1), 100, 120, 1));

        Action act = () => repository.Add(MeterReading.Create(1, new DateOnly(2026, 9, 1), 120, 140, 2));

        act.Should().Throw<DomainValidationException>();
    }
}
