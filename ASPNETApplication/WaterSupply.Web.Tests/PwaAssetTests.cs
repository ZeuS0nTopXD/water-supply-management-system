using System.Text.Json;
using FluentAssertions;

namespace WaterSupply.Web.Tests;

public sealed class PwaAssetTests
{
    private static string WebRoot => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../WaterSupply.Web/wwwroot"));

    [Fact]
    public void Manifest_is_installable_and_declares_application_scope()
    {
        var path = Path.Combine(WebRoot, "manifest.webmanifest");
        File.Exists(path).Should().BeTrue();
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        document.RootElement.GetProperty("display").GetString().Should().Be("standalone");
        document.RootElement.GetProperty("start_url").GetString().Should().Be("/Dashboard");
        document.RootElement.GetProperty("icons").GetArrayLength().Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public void Service_worker_contains_install_and_fetch_handlers()
    {
        var path = Path.Combine(WebRoot, "service-worker.js");
        File.Exists(path).Should().BeTrue();
        var source = File.ReadAllText(path);
        source.Should().Contain("addEventListener('install'");
        source.Should().Contain("addEventListener('fetch'");
        source.Should().Contain("offline");
    }

    [Fact]
    public void Layout_registers_notification_support()
    {
        var path = Path.Combine(WebRoot, "../Views/Shared/_Layout.cshtml");
        File.ReadAllText(Path.GetFullPath(path)).Should().Contain("pwa-notifications.js");
        var worker = File.ReadAllText(Path.Combine(WebRoot, "service-worker.js"));
        worker.Should().Contain("notification");
    }

    [Fact]
    public void Dashboard_has_a_print_action_for_presentation_reports()
    {
        var path = Path.Combine(WebRoot, "../Views/Dashboard/Index.cshtml");
        var source = File.ReadAllText(Path.GetFullPath(path));

        source.Should().Contain("window.print()");
        source.Should().Contain("no-print");
    }
}
