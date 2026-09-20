using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WaterSupply.Domain.Entities;
using WaterSupply.Web.Data;

namespace WaterSupply.Web.Tests;

public class DatabaseMappingTests
{
    [Fact]
    public void Sql_model_contains_required_tables_and_relationships()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"mapping-{Guid.NewGuid():N}")
            .Options;
        using var db = new ApplicationDbContext(options);

        db.Model.FindEntityType(typeof(Resident))!.FindPrimaryKey().Should().NotBeNull();
        db.Model.FindEntityType(typeof(WaterConnection))!.GetForeignKeys()
            .Should().Contain(foreignKey => foreignKey.PrincipalEntityType.ClrType == typeof(Resident));
        db.Model.FindEntityType(typeof(Bill))!.GetForeignKeys()
            .Should().Contain(foreignKey => foreignKey.PrincipalEntityType.ClrType == typeof(WaterConnection));
    }
}
