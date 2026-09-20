using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WaterSupply.Web.Services;

namespace WaterSupply.Web.Controllers;

[Authorize(Roles = "Administrator")]
public sealed class DashboardController : Controller
{
    private readonly DashboardQueryService _dashboardQuery;

    public DashboardController(DashboardQueryService dashboardQuery) => _dashboardQuery = dashboardQuery;

    [HttpGet]
    public async Task<IActionResult> Index() => View(await _dashboardQuery.GetSummaryAsync(DateOnly.FromDateTime(DateTime.Today)));
}
