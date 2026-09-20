using FluentAssertions;
using WaterSupply.Application.InMemory;
using WaterSupply.Application.Services;
using WaterSupply.Domain.Entities;
using WaterSupply.Domain.Enums;

namespace WaterSupply.Application.Tests;

public class BillingServiceTests
{
    [Fact]
    public void Recording_a_full_payment_marks_the_bill_paid()
    {
        var billRepository = new InMemoryRepository<Bill>(bill => bill.Id);
        var paymentRepository = new InMemoryRepository<Payment>(payment => payment.Id);
        var connectionRepository = new InMemoryRepository<WaterConnection>(connection => connection.Id);
        var connection = WaterConnection.Create(1, "WS-001", ConnectionType.Residential, "M-001", new DateOnly(2026, 1, 1), id: 1);
        connectionRepository.Add(connection);
        var bill = Bill.Create(1, new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 30), 10, 20, 10, 2, new DateOnly(2026, 10, 15));
        billRepository.Add(bill);
        var service = new BillingService(billRepository, paymentRepository, connectionRepository);

        service.RecordPayment(bill.Id, bill.TotalAmount, PaymentMethod.Online, new DateTime(2026, 9, 15));

        bill.Status.Should().Be(BillStatus.Paid);
        paymentRepository.GetAll().Should().ContainSingle();
    }
}
