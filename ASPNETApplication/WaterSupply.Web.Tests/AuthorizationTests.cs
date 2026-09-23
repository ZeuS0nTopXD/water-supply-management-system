using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
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
    public async Task Error_page_is_available_for_production_exception_handling()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/Home/Error");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().Contain("Something went wrong");
    }

    [Fact]
    public async Task Registration_page_uses_readable_field_labels()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/Account/Register");
        var html = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        html.Should().Contain("for=\"FullName\">Full name</label>");
        html.Should().Contain("for=\"ConfirmPassword\">Confirm password</label>");
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
        new Uri(new Uri("http://localhost"), response.Headers.Location!).AbsolutePath.Should().Be("/ResidentPortal");

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var registeredUser = await userManager.FindByEmailAsync("new.resident@example.com");
        registeredUser.Should().NotBeNull();
        (await userManager.IsInRoleAsync(registeredUser!, "Resident")).Should().BeTrue();
        (await db.Users.CountAsync(user => user.Email == "new.resident@example.com")).Should().Be(1);
        (await db.Residents.CountAsync(resident => resident.Email == "new.resident@example.com")).Should().Be(1);
    }

    [Fact]
    public async Task Registration_activates_an_existing_manual_resident_profile_instead_of_creating_a_duplicate()
    {
        using var factory = new TestWebApplicationFactory();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Residents.Add(new WaterSupply.Domain.Entities.Resident(
                0,
                "Manual Resident",
                "manual.login@example.com",
                "9999999999",
                "Manual Street",
                new DateOnly(2026, 1, 1)));
            await db.SaveChangesAsync();
        }

        using var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var registerPage = await client.GetAsync("/Account/Register");
        var html = await registerPage.Content.ReadAsStringAsync();
        var token = System.Text.RegularExpressions.Regex.Match(html, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"").Groups[1].Value;

        var response = await client.PostAsync("/Account/Register", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["FullName"] = "Manual Resident Updated",
            ["Email"] = "manual.login@example.com",
            ["Phone"] = "8888888888",
            ["Address"] = "Updated Street",
            ["Password"] = "Resident123",
            ["ConfirmPassword"] = "Resident123"
        }));

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        using var verifyScope = factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var resident = await verifyDb.Residents.SingleAsync(item => item.Email == "manual.login@example.com");
        resident.IdentityUserId.Should().NotBeNullOrWhiteSpace();
        resident.FullName.Should().Be("Manual Resident Updated");
        (await verifyDb.Residents.CountAsync(item => item.Email == "manual.login@example.com")).Should().Be(1);
    }

    [Fact]
    public async Task Login_with_external_return_url_stays_inside_the_application()
    {
        using var factory = new TestWebApplicationFactory();
        using (var scope = factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var user = new IdentityUser
            {
                UserName = "login.test@example.com",
                Email = "login.test@example.com",
                EmailConfirmed = true
            };
            (await userManager.CreateAsync(user, "Resident123")).Succeeded.Should().BeTrue();
        }

        using var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var loginPage = await client.GetAsync("/Account/Login");
        var html = await loginPage.Content.ReadAsStringAsync();
        var token = System.Text.RegularExpressions.Regex.Match(html, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"").Groups[1].Value;

        var response = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["Email"] = "login.test@example.com",
            ["Password"] = "Resident123",
            ["ReturnUrl"] = "https://example.com/not-local"
        }));

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location!.ToString().Should().Be("/Dashboard");
    }
}
