namespace WaterSupply.ConsoleApp.Menu;

public sealed class InputValidationException : Exception
{
    public InputValidationException(string message) : base(message)
    {
    }
}
