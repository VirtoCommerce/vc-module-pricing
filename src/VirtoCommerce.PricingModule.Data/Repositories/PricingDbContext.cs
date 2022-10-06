using EntityFrameworkCore.Triggers;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.PricingModule.Data.Model;

namespace VirtoCommerce.PricingModule.Data.Repositories
{
    public class PricingDbContext : DbContextWithTriggers
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
        }
#pragma warning restore S109
    }
}
