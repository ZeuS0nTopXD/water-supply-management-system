using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WaterSupply.Web.Models;

namespace WaterSupply.Web.Controllers;

[Authorize]
public sealed class DashboardController : Controller
{
    [HttpGet]
    public IActionResult Index() => View(new DashboardSummaryViewModel());
}
