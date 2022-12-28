using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VirtoCommerce.PricingModule.Data.Model;

namespace VirtoCommerce.PricingModule.Data_MySql
{
    public class PriceEntityConfiguration : IEntityTypeConfiguration<PriceEntity>
    {
        public void Configure(EntityTypeBuilder<PriceEntity> builder)
        {
            builder.Property(x => x.Sale).HasColumnType("decimal").HasPrecision(18, 4);
            builder.Property(x => x.List).HasColumnType("decimal").HasPrecision(18, 4);
        }
    }
}
