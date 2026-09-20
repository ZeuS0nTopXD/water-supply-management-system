using WaterSupply.ConsoleApp.Menu;
using WaterSupply.ConsoleApp.Seed;

var dependencies = ConsoleSeedData.Create();
var menu = new ConsoleMenu(new InputReader(Console.In, Console.Out), Console.Out, dependencies.Services.Residents, dependencies.Services.Connections, dependencies.Services.Readings, dependencies.Services.Billing, dependencies.Services.Requests, dependencies.Services.Reports);
menu.Run();
