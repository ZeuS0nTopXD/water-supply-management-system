using System.ComponentModel.DataAnnotations;
using System.Reflection;
using FluentAssertions;
using WaterSupply.Domain.Enums;
using WaterSupply.Web.Models.AccountViewModels;
using WaterSupply.Web.Models.BillingViewModels;
using WaterSupply.Web.Models.ConnectionViewModels;
using WaterSupply.Web.Models.MeterReadingViewModels;
using WaterSupply.Web.Models.ResidentViewModels;
using WaterSupply.Web.Models.ServiceRequestViewModels;

namespace WaterSupply.Web.Tests;

public sealed class FrontendLabelTests
{
    [Theory]
    [InlineData(typeof(LoginViewModel), nameof(LoginViewModel.Email), "Email address")]
    [InlineData(typeof(LoginViewModel), nameof(LoginViewModel.RememberMe), "Remember me")]
    [InlineData(typeof(ChangePasswordViewModel), nameof(ChangePasswordViewModel.ConfirmPassword), "Confirm new password")]
    [InlineData(typeof(ResidentEditViewModel), nameof(ResidentEditViewModel.FullName), "Full name")]
    [InlineData(typeof(ResidentEditViewModel), nameof(ResidentEditViewModel.RegistrationDate), "Registration date")]
    [InlineData(typeof(ResidentEditViewModel), nameof(ResidentEditViewModel.IsActive), "Active resident")]
    [InlineData(typeof(ConnectionEditViewModel), nameof(ConnectionEditViewModel.ResidentId), "Resident")]
    [InlineData(typeof(ConnectionEditViewModel), nameof(ConnectionEditViewModel.ConnectionNumber), "Connection number")]
    [InlineData(typeof(ConnectionEditViewModel), nameof(ConnectionEditViewModel.ConnectionType), "Connection type")]
    [InlineData(typeof(ConnectionEditViewModel), nameof(ConnectionEditViewModel.MeterNumber), "Meter number")]
    [InlineData(typeof(MeterReadingEditViewModel), nameof(MeterReadingEditViewModel.WaterConnectionId), "Water connection")]
    [InlineData(typeof(MeterReadingEditViewModel), nameof(MeterReadingEditViewModel.PreviousReading), "Previous reading")]
    [InlineData(typeof(MeterReadingEditViewModel), nameof(MeterReadingEditViewModel.CurrentReading), "Current reading")]
    [InlineData(typeof(ServiceRequestCreateViewModel), nameof(ServiceRequestCreateViewModel.ResidentId), "Resident")]
    [InlineData(typeof(ServiceRequestCreateViewModel), nameof(ServiceRequestCreateViewModel.WaterConnectionId), "Water connection")]
    [InlineData(typeof(ServiceRequestCreateViewModel), nameof(ServiceRequestCreateViewModel.RequestType), "Request type")]
    [InlineData(typeof(ServiceRequestStatusViewModel), nameof(ServiceRequestStatusViewModel.Status), "Request status")]
    [InlineData(typeof(ServiceRequestStatusViewModel), nameof(ServiceRequestStatusViewModel.StaffNotes), "Staff notes")]
    [InlineData(typeof(BillCreateViewModel), nameof(BillCreateViewModel.WaterConnectionId), "Water connection")]
    [InlineData(typeof(BillCreateViewModel), nameof(BillCreateViewModel.MeterReadingId), "Meter reading")]
    [InlineData(typeof(BillCreateViewModel), nameof(BillCreateViewModel.UnitsConsumed), "Units consumed")]
    [InlineData(typeof(BillCreateViewModel), nameof(BillCreateViewModel.RatePerUnit), "Rate per unit")]
    public void Form_fields_expose_human_facing_labels(Type viewModelType, string propertyName, string expectedLabel)
    {
        var property = viewModelType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

        property.Should().NotBeNull();
        var display = property!.GetCustomAttribute<DisplayAttribute>();
        display.Should().NotBeNull();
        display!.GetName().Should().Be(expectedLabel);
    }

    [Theory]
    [InlineData(typeof(RequestType), nameof(RequestType.NoSupply), "No supply")]
    [InlineData(typeof(RequestStatus), nameof(RequestStatus.InProgress), "In progress")]
    public void Enum_options_expose_human_facing_labels(Type enumType, string memberName, string expectedLabel)
    {
        var member = enumType.GetMember(memberName).Should().ContainSingle().Subject;

        var display = member.GetCustomAttribute<DisplayAttribute>();
        display.Should().NotBeNull();
        display!.GetName().Should().Be(expectedLabel);
    }

    [Fact]
    public void Bill_report_closes_its_table_footer()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "../../../../WaterSupply.Web/Views/Reports/Index.cshtml");
        var source = File.ReadAllText(Path.GetFullPath(path));

        source.Should().Contain("</tfoot>");
    }
}
