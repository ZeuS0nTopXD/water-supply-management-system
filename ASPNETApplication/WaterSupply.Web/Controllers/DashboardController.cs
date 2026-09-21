using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WaterSupply.Web.Services;

namespace WaterSupply.Web.Controllers;

[Authorize]
public sealed class DashboardController(DashboardQueryService dashboard) : Controller
{
    public async Task<IActionResult> Index(DateOnly? month = null)
    {
        var selectedMonth = month ?? DateOnly.FromDateTime(DateTime.Today);
        ViewBag.SelectedMonth = selectedMonth;
        return View(await dashboard.GetSummaryAsync(selectedMonth));
    }
}
