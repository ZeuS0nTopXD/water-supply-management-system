using WaterSupply.ConsoleApp.Menu;
using WaterSupply.ConsoleApp.Seed;

var dependencies = ConsoleSeedData.Create();
var menu = new ConsoleMenu(new InputReader(Console.In, Console.Out), Console.Out, dependencies.Services);
menu.Run();
