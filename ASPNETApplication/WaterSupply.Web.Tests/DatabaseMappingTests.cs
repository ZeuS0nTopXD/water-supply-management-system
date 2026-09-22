using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WaterSupply.Domain.Entities;
using WaterSupply.Web.Data;

namespace WaterSupply.Web.Tests;

public class DatabaseMappingTests
{
    [Fact]
    public void Model_contains_exactly_five_business_tables_with_required_relationships()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"mapping-{Guid.NewGuid():N}")
            .Options;
        using var db = new ApplicationDbContext(options);

        var businessTables = db.Model.GetEntityTypes()
            .Select(entity => entity.GetTableName())
            .Where(table => table is not null
                && !table.StartsWith("AspNet", StringComparison.Ordinal)
                && !string.Equals(table, "DataProtectionKey", StringComparison.Ordinal))
            .Distinct()
            .ToArray();

        businessTables.Should().BeEquivalentTo(
            "Residents",
            "WaterConnections",
            "MeterReadings",
            "Bills",
            "ServiceRequests");

        db.Model.FindEntityType(typeof(Resident))!.FindPrimaryKey().Should().NotBeNull();
        db.Model.FindEntityType(typeof(WaterConnection))!.FindPrimaryKey().Should().NotBeNull();
        db.Model.FindEntityType(typeof(MeterReading))!.FindPrimaryKey().Should().NotBeNull();
        db.Model.FindEntityType(typeof(Bill))!.FindPrimaryKey().Should().NotBeNull();
        db.Model.FindEntityType(typeof(ServiceRequest))!.FindPrimaryKey().Should().NotBeNull();
        db.Model.FindEntityType(typeof(WaterConnection))!.GetForeignKeys()
            .Should().Contain(foreignKey => foreignKey.PrincipalEntityType.ClrType == typeof(Resident));
        db.Model.FindEntityType(typeof(MeterReading))!.GetForeignKeys()
            .Should().Contain(foreignKey => foreignKey.PrincipalEntityType.ClrType == typeof(WaterConnection));
        db.Model.FindEntityType(typeof(Bill))!.GetForeignKeys()
            .Should().Contain(foreignKey => foreignKey.PrincipalEntityType.ClrType == typeof(WaterConnection));
        db.Model.FindEntityType(typeof(ServiceRequest))!.GetForeignKeys()
            .Should().Contain(foreignKey => foreignKey.PrincipalEntityType.ClrType == typeof(Resident));
    }
}
