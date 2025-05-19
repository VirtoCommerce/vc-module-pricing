using System.Reflection;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.Platform.Data.Extensions;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.PricingModule.Data.Model;

namespace VirtoCommerce.PricingModule.Data.Repositories
{
    public class PricingDbContext : DbContextBase
    {
#pragma warning disable S109
        public PricingDbContext(DbContextOptions<PricingDbContext> options)
            : base(options)
        {
        }

        protected PricingDbContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PriceEntity>(builder =>
            {
                builder.ToAuditableEntityTable("Price");
                builder.HasOne(x => x.Pricelist).WithMany(x => x.Prices).IsRequired().HasForeignKey(x => x.PricelistId);
                builder.HasIndex(x => new { x.PricelistId, x.ProductId, x.StartDate, x.EndDate }).HasDatabaseName("IX_PricelistProductDates");
                builder.Property(x => x.MinQuantity).HasPrecision(18, 2);
                builder.HasIndex(x => x.OuterId);
            });

            modelBuilder.Entity<PricelistEntity>(builder =>
            {
                builder.ToAuditableEntityTable("Pricelist");
                builder.HasIndex(x => x.OuterId);
            });

            modelBuilder.Entity<PricelistAssignmentEntity>(builder =>
            {
                builder.ToAuditableEntityTable("PricelistAssignment");
                builder.HasOne(x => x.Pricelist).WithMany(x => x.Assignments).IsRequired().HasForeignKey(x => x.PricelistId);
                builder.HasIndex(x => x.OuterId);
            });

            // ugly hack because EFCore removed ultra useful DbQuery type in 3.0
            modelBuilder.Entity<MergedPriceEntity>(builder =>
            {
                builder.HasNoKey().ToView("empty");
                builder.Property(x => x.List).HasPrecision(18, 2);
                builder.Property(x => x.MinQuantity).HasPrecision(18, 2);
                builder.Property(x => x.Sale).HasPrecision(18, 2);
            });

            // Allows configuration for an entity type for different database types.
            // Applies configuration from all <see cref="IEntityTypeConfiguration{TEntity}" in VirtoCommerce.PricingModule.Data.XXX project. /> 
            switch (Database.ProviderName)
            {
                case "Pomelo.EntityFrameworkCore.MySql":
                    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("VirtoCommerce.PricingModule.Data.MySql"));
                    break;
                case "Npgsql.EntityFrameworkCore.PostgreSQL":
                    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("VirtoCommerce.PricingModule.Data.PostgreSql"));
                    break;
                case "Microsoft.EntityFrameworkCore.SqlServer":
                    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("VirtoCommerce.PricingModule.Data.SqlServer"));
                    break;
            }
        }
#pragma warning restore S109
    }
}
