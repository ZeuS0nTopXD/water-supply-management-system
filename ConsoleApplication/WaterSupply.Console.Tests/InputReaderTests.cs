using WaterSupply.ConsoleApp.Menu;

namespace WaterSupply.Console.Tests;

public class InputReaderTests
{
    [Fact]
    public void ReadInt_rejects_non_numeric_input_without_terminating_the_process()
    {
        var reader = new InputReader(new StringReader("x\n3\n"), TextWriter.Null);

        Assert.Equal(3, reader.ReadInt("Choice", 0, 5));
    }
}
