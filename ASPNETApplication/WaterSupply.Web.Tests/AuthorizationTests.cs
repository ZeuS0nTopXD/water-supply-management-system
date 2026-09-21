using System.Net;
using FluentAssertions;

namespace WaterSupply.Web.Tests;

public sealed class AuthorizationTests
{
    [Fact]
    public async Task Anonymous_user_is_redirected_to_login_for_dashboard()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync("/Dashboard");

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location!.AbsolutePath.Should().Contain("/Account/Login");
    }
}
