using System.Globalization;

namespace WaterSupply.ConsoleApp.Menu;

public sealed class InputReader
{
    private readonly TextReader _input;
    private readonly TextWriter _output;

    public InputReader(TextReader input, TextWriter output)
    {
        _input = input;
        _output = output;
    }

    public string ReadRequiredString(string prompt)
    {
        while (true)
        {
            _output.Write(prompt);
            var value = _input.ReadLine();
            if (!string.IsNullOrWhiteSpace(value)) return value.Trim();
            _output.WriteLine("Value is required.");
        }
    }

    public int ReadInt(string prompt, int min, int max)
    {
        while (true)
        {
            _output.Write(prompt);
            var raw = _input.ReadLine();
            if (int.TryParse(raw, out var value) && value >= min && value <= max) return value;
            _output.WriteLine($"Enter a number between {min} and {max}.");
        }
    }

    public decimal ReadDecimal(string prompt, decimal min = 0)
    {
        while (true)
        {
            _output.Write(prompt);
            var raw = _input.ReadLine();
            if (decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var value) && value >= min) return value;
            _output.WriteLine($"Enter a number greater than or equal to {min}.");
        }
    }

    public DateOnly ReadDate(string prompt)
    {
        while (true)
        {
            _output.Write(prompt);
            var raw = _input.ReadLine();
            if (DateOnly.TryParseExact(raw, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var value)) return value;
            _output.WriteLine("Enter a date in yyyy-MM-dd format.");
        }
    }
}
