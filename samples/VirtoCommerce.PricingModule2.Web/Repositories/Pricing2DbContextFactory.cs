using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using VirtoCommerce.Platform.Data.PostgreSql.Extensions;

namespace VirtoCommerce.PricingModule2.Web.Repositories;

public class Pricing2DbContextFactory : IDesignTimeDbContextFactory<Pricing2DbContext>
{
    public Pricing2DbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<Pricing2DbContext>();

        var connectionString = "Host=localhost;Port=5432;Database=VirtoCommerce3;Persist Security Info=True;User ID=virto;Password=virto;Timeout=30;TrustServerCertificate=True;";

        optionsBuilder.UsePostgreSqlDatabase(connectionString, typeof(Pricing2DbContextFactory), Module.Configuration);

        return new Pricing2DbContext(optionsBuilder.Options);
    }
}
