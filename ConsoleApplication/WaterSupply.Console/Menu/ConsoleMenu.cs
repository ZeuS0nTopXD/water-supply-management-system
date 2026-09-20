using WaterSupply.Application.Abstractions;
using WaterSupply.Domain.Enums;
using WaterSupply.Domain.Exceptions;

namespace WaterSupply.ConsoleApp.Menu;

public sealed class ConsoleMenu
{
    private readonly InputReader _input;
    private readonly TextWriter _output;
    private readonly IResidentService? _residents;
    private readonly IWaterConnectionService? _connections;
    private readonly IMeterReadingService? _readings;
    private readonly IBillingService? _billing;
    private readonly IServiceRequestService? _requests;
    private readonly IReportService? _reports;

    public ConsoleMenu(InputReader input, TextWriter output, IResidentService? residents = null, IWaterConnectionService? connections = null, IMeterReadingService? readings = null, IBillingService? billing = null, IServiceRequestService? requests = null, IReportService? reports = null)
    {
        _input = input;
        _output = output;
        _residents = residents;
        _connections = connections;
        _readings = readings;
        _billing = billing;
        _requests = requests;
        _reports = reports;
    }

    public void Run()
    {
        var running = true;
        while (running)
        {
            PrintMainMenu();
            try
            {
                var choice = _input.ReadInt("Choose an option: ", 0, 9);
                running = choice switch
                {
                    0 => false,
                    1 => RunResidentMenu(),
                    2 => RunConnectionMenu(),
                    3 => RunReadingMenu(),
                    4 => RunBillMenu(),
                    5 => RunPaymentMenu(),
                    6 => RunRequestMenu(),
                    7 => RunSearch(),
                    8 => RunReports(),
                    9 => PrintHelp(),
                    _ => true
                };
            }
            catch (DomainValidationException exception)
            {
                _output.WriteLine($"Validation error: {exception.Message}");
            }
            catch (KeyNotFoundException exception)
            {
                _output.WriteLine($"Record not found: {exception.Message}");
            }
            catch (Exception exception) when (exception is FormatException or ArgumentException or InvalidOperationException)
            {
                _output.WriteLine($"Operation could not be completed: {exception.Message}");
            }
        }

        _output.WriteLine("Goodbye.");
    }

    private void PrintMainMenu()
    {
        _output.WriteLine();
        _output.WriteLine("=== Water Supply Management System ===");
        _output.WriteLine("1. Manage residents");
        _output.WriteLine("2. Manage water connections");
        _output.WriteLine("3. Record meter reading");
        _output.WriteLine("4. Generate bill");
        _output.WriteLine("5. Record payment");
        _output.WriteLine("6. Service requests");
        _output.WriteLine("7. Search records");
        _output.WriteLine("8. Reports");
        _output.WriteLine("9. Help");
        _output.WriteLine("0. Exit");
    }

    private bool RunResidentMenu()
    {
        var action = _input.ReadInt("1 Add resident, 2 Search residents, 3 Delete resident: ", 1, 3);
        if (action == 1)
        {
            var resident = Require(_residents).Create(_input.ReadRequiredString("Full name: "), _input.ReadRequiredString("Email: "), _input.ReadRequiredString("Phone: "), _input.ReadRequiredString("Address: "), _input.ReadDate("Registration date (yyyy-MM-dd): "));
            _output.WriteLine($"Resident created with ID {resident.Id}.");
        }
        else if (action == 2)
        {
            foreach (var resident in Require(_residents).Search(_input.ReadRequiredString("Search name/email/phone: ")))
                _output.WriteLine($"{resident.Id}: {resident.FullName} | {resident.Phone} | {resident.Email}");
        }
        else
        {
            Require(_residents).Delete(_input.ReadInt("Resident ID: ", 1, int.MaxValue));
            _output.WriteLine("Resident deleted.");
        }

        return true;
    }

    private bool RunConnectionMenu()
    {
        var connection = Require(_connections).Create(_input.ReadInt("Resident ID: ", 1, int.MaxValue), _input.ReadRequiredString("Connection number: "), ConnectionType.Residential, _input.ReadRequiredString("Meter number: "), _input.ReadDate("Connection date (yyyy-MM-dd): "));
        _output.WriteLine($"Connection {connection.ConnectionNumber} created.");
        return true;
    }

    private bool RunReadingMenu()
    {
        var reading = Require(_readings).Record(_input.ReadInt("Connection ID: ", 1, int.MaxValue), _input.ReadDate("Reading date (yyyy-MM-dd): "), _input.ReadDecimal("Previous reading: "), _input.ReadDecimal("Current reading: "));
        _output.WriteLine($"Reading recorded. Consumption: {reading.Consumption:0.##} units.");
        return true;
    }

    private bool RunBillMenu()
    {
        var bill = Require(_billing).GenerateBill(_input.ReadInt("Connection ID: ", 1, int.MaxValue), _input.ReadDate("Period start (yyyy-MM-dd): "), _input.ReadDate("Period end (yyyy-MM-dd): "), _input.ReadDecimal("Units consumed: "), _input.ReadDecimal("Rate per unit: "), _input.ReadDecimal("Fixed charge: "), _input.ReadDecimal("Tax: "), _input.ReadDate("Due date (yyyy-MM-dd): "));
        _output.WriteLine($"Bill {bill.Id} generated. Total: {bill.TotalAmount:0.00}.");
        return true;
    }

    private bool RunPaymentMenu()
    {
        var payment = Require(_billing).RecordPayment(_input.ReadInt("Bill ID: ", 1, int.MaxValue), _input.ReadDecimal("Amount: ", 0.01m), PaymentMethod.Cash, DateTime.Now);
        _output.WriteLine($"Payment {payment.ReferenceNumber} recorded.");
        return true;
    }

    private bool RunRequestMenu()
    {
        var request = Require(_requests).Create(_input.ReadInt("Resident ID: ", 1, int.MaxValue), null, ServiceRequestType.Other, _input.ReadRequiredString("Description: "));
        _output.WriteLine($"Service request {request.Id} created.");
        return true;
    }

    private bool RunSearch()
    {
        var query = _input.ReadRequiredString("Search residents or connections: ");
        _output.WriteLine("Residents:");
        foreach (var resident in Require(_residents).Search(query)) _output.WriteLine($"{resident.Id}: {resident.FullName}");
        _output.WriteLine("Connections:");
        foreach (var connection in Require(_connections).Search(query)) _output.WriteLine($"{connection.Id}: {connection.ConnectionNumber} ({connection.Status})");
        return true;
    }

    private bool RunReports()
    {
        var reportType = _input.ReadInt("1 Outstanding bills, 2 Consumption, 3 Payments: ", 1, 3);
        var reports = Require(_reports);
        if (reportType == 1)
        {
            var report = reports.GetOutstandingBillsReport();
            _output.WriteLine($"Outstanding total: {report.TotalOutstanding:0.00}");
            foreach (var row in report.Rows) _output.WriteLine($"Bill {row.BillId} | Connection {row.WaterConnectionId} | {row.Status} | {row.OutstandingAmount:0.00}");
        }
        else if (reportType == 2)
        {
            var report = reports.GetConsumptionReport(_input.ReadDate("From (yyyy-MM-dd): "), _input.ReadDate("To (yyyy-MM-dd): "));
            _output.WriteLine($"Total consumption: {report.TotalConsumption:0.##}");
            foreach (var row in report.Rows) _output.WriteLine($"{row.ConnectionNumber}: {row.UnitsConsumed:0.##}");
        }
        else
        {
            var report = reports.GetPaymentCollectionReport(_input.ReadDate("From (yyyy-MM-dd): "), _input.ReadDate("To (yyyy-MM-dd): "));
            _output.WriteLine($"Total collected: {report.TotalCollected:0.00}");
            foreach (var row in report.Rows) _output.WriteLine($"Payment {row.PaymentId} / Bill {row.BillId}: {row.Amount:0.00}");
        }

        return true;
    }

    private bool PrintHelp()
    {
        _output.WriteLine("Use the numbered menu to manage water-supply records, search data, and view reports.");
        return true;
    }

    private static T Require<T>(T? service) where T : class => service ?? throw new InvalidOperationException("This menu action is not configured.");
}
