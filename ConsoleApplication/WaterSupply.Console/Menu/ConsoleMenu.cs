using WaterSupply.Application.Abstractions;
using WaterSupply.Domain.Enums;
using WaterSupply.Domain.Exceptions;
using WaterSupply.ConsoleApp.Seed;

namespace WaterSupply.ConsoleApp.Menu;

public sealed class ConsoleMenu
{
    private readonly InputReader _input;
    private readonly TextWriter _output;
    private readonly ConsoleMenuServices? _services;

    public ConsoleMenu(InputReader input, TextWriter output, ConsoleMenuServices? services = null)
    {
        _input = input;
        _output = output;
        _services = services;
    }

    public void Run()
    {
        var running = true;
        while (running)
        {
            PrintMainMenu();
            try
            {
                var choice = _input.ReadInt("Choose an option: ", 0, 8);
                running = choice switch
                {
                    0 => false,
                    1 => AddResident(),
                    2 => ListResidents(),
                    3 => SearchResident(),
                    4 => AddConnection(),
                    5 => RecordReading(),
                    6 => ViewBills(),
                    7 => AddServiceRequest(),
                    8 => ViewSummaryReport(),
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
        }

        _output.WriteLine("Goodbye.");
    }

    private void PrintMainMenu()
    {
        _output.WriteLine();
        _output.WriteLine("=== Water Supply Management System ===");
        _output.WriteLine("1. Add resident");
        _output.WriteLine("2. List residents");
        _output.WriteLine("3. Search resident");
        _output.WriteLine("4. Add water connection");
        _output.WriteLine("5. Record meter reading");
        _output.WriteLine("6. View bills");
        _output.WriteLine("7. Add service request");
        _output.WriteLine("8. View summary report");
        _output.WriteLine("0. Exit");
    }

    private bool AddResident()
    {
        var resident = Services.Residents.Create(
            _input.ReadRequiredString("Full name: "),
            _input.ReadRequiredString("Email: "),
            _input.ReadRequiredString("Phone: "),
            _input.ReadRequiredString("Address: "),
            _input.ReadDate("Registration date (yyyy-MM-dd): "));
        _output.WriteLine($"Resident created with ID {resident.ResidentId}.");
        return true;
    }

    private bool ListResidents()
    {
        foreach (var resident in Services.Residents.Search(null))
        {
            _output.WriteLine($"{resident.ResidentId}: {resident.FullName} | {resident.Phone} | {resident.Email}");
        }

        return true;
    }

    private bool SearchResident()
    {
        var search = _input.ReadRequiredString("Search name/email/phone: ");
        foreach (var resident in Services.Residents.Search(search))
        {
            _output.WriteLine($"{resident.ResidentId}: {resident.FullName} | {resident.Phone} | {resident.Email}");
        }

        return true;
    }

    private bool AddConnection()
    {
        var connection = Services.Connections.Create(
            _input.ReadInt("Resident ID: ", 1, int.MaxValue),
            _input.ReadRequiredString("Connection number: "),
            ConnectionType.Residential,
            _input.ReadRequiredString("Meter number: "),
            _input.ReadDate("Connection date (yyyy-MM-dd): "));
        _output.WriteLine($"Connection {connection.ConnectionNumber} created.");
        return true;
    }

    private bool RecordReading()
    {
        var connectionId = _input.ReadInt("Connection ID: ", 1, int.MaxValue);
        var date = _input.ReadDate("Reading date (yyyy-MM-dd): ");
        var reading = Services.Readings.Record(connectionId, date, _input.ReadDecimal("Previous reading: "), _input.ReadDecimal("Current reading: "));
        var bill = Services.Billing.Generate(connectionId, reading.MeterReadingId, date, (int)reading.Consumption, 5m);
        _output.WriteLine($"Reading recorded. Consumption: {reading.Consumption:0.##} units.");
        _output.WriteLine($"Bill {bill.BillId} generated. Total: {bill.TotalAmount:0.00}.");
        return true;
    }

    private bool ViewBills()
    {
        foreach (var bill in Services.Billing.GetAll())
        {
            _output.WriteLine($"Bill {bill.BillId} | Connection {bill.WaterConnectionId} | {bill.Status} | {bill.TotalAmount:0.00}");
        }

        return true;
    }

    private bool AddServiceRequest()
    {
        var request = Services.Requests.Create(
            _input.ReadInt("Resident ID: ", 1, int.MaxValue),
            null,
            RequestType.Other,
            _input.ReadRequiredString("Description: "));
        _output.WriteLine($"Service request {request.ServiceRequestId} created.");
        return true;
    }

    private bool ViewSummaryReport()
    {
        var report = Services.Reports.GetSummary();
        _output.WriteLine("Summary Report");
        _output.WriteLine($"Residents: {report.Residents}");
        _output.WriteLine($"Active connections: {report.ActiveConnections}");
        _output.WriteLine($"Total consumption: {report.TotalConsumption:0.##}");
        _output.WriteLine($"Total bill amount: {report.TotalBillAmount:0.00}");
        _output.WriteLine($"Open requests: {report.OpenRequests}");
        return true;
    }

    private ConsoleMenuServices Services => _services ?? throw new InvalidOperationException("Console services are not configured.");
}
