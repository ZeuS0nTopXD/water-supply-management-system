using WaterSupply.ConsoleApp.Menu;

namespace WaterSupply.Console.Tests;

public class MenuSmokeTests
{
    [Fact]
    public void Menu_option_zero_exits_after_printing_the_menu()
    {
        var output = new StringWriter();
        var menu = new ConsoleMenu(new InputReader(new StringReader("0\n"), output), output);

        menu.Run();

        Assert.Contains("Water Supply Management System", output.ToString());
        Assert.Contains("Add resident", output.ToString());
        Assert.Contains("Search resident", output.ToString());
        Assert.Contains("Record meter reading", output.ToString());
        Assert.Contains("View bills", output.ToString());
        Assert.Contains("Add service request", output.ToString());
        Assert.Contains("View summary report", output.ToString());
        Assert.Contains("Exit", output.ToString());
        Assert.Contains("Goodbye", output.ToString());
    }
}
