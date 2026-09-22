using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WaterSupply.Web.Services;

namespace WaterSupply.Web.Controllers;

[Authorize]
public sealed class DashboardController(DashboardQueryService dashboard) : Controller
{
    public async Task<IActionResult> Index(string? month = null)
    {
        if (User.IsInRole("Resident"))
        {
            return RedirectToAction(nameof(ResidentPortalController.Index), "ResidentPortal");
        }

        var selectedMonth = ParseMonth(month) ?? DateOnly.FromDateTime(DateTime.Today);
        return View(await dashboard.GetSummaryAsync(selectedMonth));
    }

    private static DateOnly? ParseMonth(string? month)
    {
        return DateOnly.TryParseExact(
            $"{month}-01",
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var parsed)
            ? parsed
            : null;
    }
}
