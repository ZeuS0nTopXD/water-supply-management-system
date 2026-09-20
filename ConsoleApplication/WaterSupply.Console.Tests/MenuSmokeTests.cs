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
        Assert.Contains("Goodbye", output.ToString());
    }
}
