using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WaterSupply.Web.Data;

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

    [Fact]
    public async Task Login_page_loads_successfully()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/Account/Login");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().Contain("<h1>Login</h1>");
    }

    [Fact]
    public async Task Registration_creates_account_and_resident_record()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var registerPage = await client.GetAsync("/Account/Register");
        registerPage.StatusCode.Should().Be(HttpStatusCode.OK);
        var html = await registerPage.Content.ReadAsStringAsync();
        var token = System.Text.RegularExpressions.Regex.Match(html, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"").Groups[1].Value;
        token.Should().NotBeNullOrWhiteSpace();

        var response = await client.PostAsync("/Account/Register", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["FullName"] = "New Resident",
            ["Email"] = "new.resident@example.com",
            ["Phone"] = "9999999999",
            ["Address"] = "New Street",
            ["Password"] = "Resident123",
            ["ConfirmPassword"] = "Resident123"
        }));

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        new Uri(new Uri("http://localhost"), response.Headers.Location!).AbsolutePath.Should().Be("/Dashboard");

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        (await db.Users.CountAsync(user => user.Email == "new.resident@example.com")).Should().Be(1);
        (await db.Residents.CountAsync(resident => resident.Email == "new.resident@example.com")).Should().Be(1);
    }
}
