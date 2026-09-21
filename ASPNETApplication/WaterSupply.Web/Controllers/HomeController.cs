using Microsoft.AspNetCore.Mvc;

namespace WaterSupply.Web.Controllers;

public sealed class HomeController : Controller
{
    public IActionResult Index() => View();
}
