﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using VirtoCommerce.PricingModule.Data.Repositories;

#nullable disable

namespace VirtoCommerce.PricingModule.Data.PostgreSql.Migrations
{
    [DbContext(typeof(PricingDbContext))]
    partial class PricingDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("VirtoCommerce.PricingModule.Data.Model.MergedPriceEntity", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<decimal>("List")
                        .HasPrecision(18, 2)
                        .HasColumnType("numeric(18,2)");

                    b.Property<decimal>("MinQuantity")
                        .HasPrecision(18, 2)
                        .HasColumnType("numeric(18,2)");

                    b.Property<string>("PricelistId")
                        .HasColumnType("text");

                    b.Property<string>("ProductId")
                        .HasColumnType("text");

                    b.Property<decimal?>("Sale")
                        .HasPrecision(18, 2)
                        .HasColumnType("numeric(18,2)");

                    b.Property<int>("State")
                        .HasColumnType("integer");

                    b.ToView("empty");
                });

            modelBuilder.Entity("VirtoCommerce.PricingModule.Data.Model.PriceEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("EndDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<decimal>("List")
                        .HasColumnType("Money");

                    b.Property<decimal>("MinQuantity")
                        .HasPrecision(18, 2)
                        .HasColumnType("numeric(18,2)");

                    b.Property<string>("ModifiedBy")
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("OuterId")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("PricelistId")
                        .IsRequired()
                        .HasColumnType("character varying(128)");

                    b.Property<string>("ProductId")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("ProductName")
                        .HasMaxLength(1024)
                        .HasColumnType("character varying(1024)");

                    b.Property<decimal?>("Sale")
                        .HasColumnType("Money");

                    b.Property<DateTime?>("StartDate")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("PricelistId", "ProductId", "StartDate", "EndDate")
                        .HasDatabaseName("IX_PricelistProductDates");

                    b.ToTable("Price", (string)null);
                });

            modelBuilder.Entity("VirtoCommerce.PricingModule.Data.Model.PricelistAssignmentEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("CatalogId")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("ConditionExpression")
                        .HasColumnType("text");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .HasMaxLength(512)
                        .HasColumnType("character varying(512)");

                    b.Property<DateTime?>("EndDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("ModifiedBy")
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("OuterId")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("PredicateVisualTreeSerialized")
                        .HasColumnType("text");

                    b.Property<string>("PricelistId")
                        .IsRequired()
                        .HasColumnType("character varying(128)");

                    b.Property<int>("Priority")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("StartDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("StoreId")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.HasKey("Id");

                    b.HasIndex("PricelistId");

                    b.ToTable("PricelistAssignment", (string)null);
                });

            modelBuilder.Entity("VirtoCommerce.PricingModule.Data.Model.PricelistEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Currency")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<string>("Description")
                        .HasMaxLength(512)
                        .HasColumnType("character varying(512)");

                    b.Property<string>("ModifiedBy")
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("OuterId")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.HasKey("Id");

                    b.HasIndex("OuterId");

                    b.ToTable("Pricelist", (string)null);
                });

            modelBuilder.Entity("VirtoCommerce.PricingModule.Data.Model.PriceEntity", b =>
                {
                    b.HasOne("VirtoCommerce.PricingModule.Data.Model.PricelistEntity", "Pricelist")
                        .WithMany("Prices")
                        .HasForeignKey("PricelistId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Pricelist");
                });

            modelBuilder.Entity("VirtoCommerce.PricingModule.Data.Model.PricelistAssignmentEntity", b =>
                {
                    b.HasOne("VirtoCommerce.PricingModule.Data.Model.PricelistEntity", "Pricelist")
                        .WithMany("Assignments")
                        .HasForeignKey("PricelistId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Pricelist");
                });

            modelBuilder.Entity("VirtoCommerce.PricingModule.Data.Model.PricelistEntity", b =>
                {
                    b.Navigation("Assignments");

                    b.Navigation("Prices");
                });
#pragma warning restore 612, 618
        }
    }
}
