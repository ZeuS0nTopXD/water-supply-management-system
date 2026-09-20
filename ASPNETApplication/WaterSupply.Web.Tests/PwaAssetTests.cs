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
}
