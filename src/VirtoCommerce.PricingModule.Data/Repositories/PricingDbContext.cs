using System.Reflection;
using Microsoft.EntityFrameworkCore;
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
            modelBuilder.Entity<PriceEntity>().ToTable("Price").HasKey(x => x.Id);
            modelBuilder.Entity<PriceEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();
            modelBuilder.Entity<PriceEntity>().HasOne(x => x.Pricelist).WithMany(x => x.Prices).IsRequired().HasForeignKey(x => x.PricelistId);
            modelBuilder.Entity<PriceEntity>().HasIndex(x => new { x.PricelistId, x.ProductId, x.StartDate, x.EndDate }).HasDatabaseName("IX_PricelistProductDates");
            modelBuilder.Entity<PriceEntity>().Property(x => x.MinQuantity).HasPrecision(18, 2);
            modelBuilder.Entity<PriceEntity>();

            modelBuilder.Entity<PricelistEntity>().ToTable("Pricelist").HasKey(x => x.Id);
            modelBuilder.Entity<PricelistEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();
            modelBuilder.Entity<PricelistEntity>().HasIndex(x => x.OuterId);

            modelBuilder.Entity<PricelistAssignmentEntity>().ToTable("PricelistAssignment").HasKey(x => x.Id);
            modelBuilder.Entity<PricelistAssignmentEntity>().HasOne(x => x.Pricelist).WithMany(x => x.Assignments).IsRequired().HasForeignKey(x => x.PricelistId);
            modelBuilder.Entity<PricelistAssignmentEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();

            // ugly hack because EFCore removed ultra useful DbQuery type in 3.0
            modelBuilder.Entity<MergedPriceEntity>().HasNoKey().ToView("empty");
            modelBuilder.Entity<MergedPriceEntity>().Property(x => x.List).HasPrecision(18, 2);
            modelBuilder.Entity<MergedPriceEntity>().Property(x => x.MinQuantity).HasPrecision(18, 2);
            modelBuilder.Entity<MergedPriceEntity>().Property(x => x.Sale).HasPrecision(18, 2);

            base.OnModelCreating(modelBuilder);



            // Allows configuration for an entity type for different database types.
            // Applies configuration from all <see cref="IEntityTypeConfiguration{TEntity}" in VirtoCommerce.PricingModule.Data.XXX project. /> 
            switch (this.Database.ProviderName)
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
