﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VirtoCommerce.PricingModule.Data.Repositories;

#nullable disable

namespace VirtoCommerce.PricingModule.Data.MySql.Migrations
{
    [DbContext(typeof(PricingDbContext))]
    [Migration("20221226150717_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("VirtoCommerce.PricingModule.Data.Model.MergedPriceEntity", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("longtext");

                    b.Property<decimal>("List")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("MinQuantity")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("PricelistId")
                        .HasColumnType("longtext");

                    b.Property<string>("ProductId")
                        .HasColumnType("longtext");

                    b.Property<decimal?>("Sale")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("State")
                        .HasColumnType("int");

                    b.ToView("empty");
                });

            modelBuilder.Entity("VirtoCommerce.PricingModule.Data.Model.PriceEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime?>("EndDate")
                        .HasColumnType("datetime(6)");

                    b.Property<decimal>("List")
                        .HasPrecision(18, 4)
                        .HasColumnType("decimal(18,4)");

                    b.Property<decimal>("MinQuantity")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("ModifiedBy")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("OuterId")
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("PricelistId")
                        .IsRequired()
                        .HasColumnType("varchar(128)");

                    b.Property<string>("ProductId")
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("ProductName")
                        .HasMaxLength(1024)
                        .HasColumnType("varchar(1024)");

                    b.Property<decimal?>("Sale")
                        .HasPrecision(18, 4)
                        .HasColumnType("decimal(18,4)");

                    b.Property<DateTime?>("StartDate")
                        .HasColumnType("datetime(6)");

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
                        .HasColumnType("varchar(128)");

                    b.Property<string>("CatalogId")
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("ConditionExpression")
                        .HasColumnType("longtext");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Description")
                        .HasMaxLength(512)
                        .HasColumnType("varchar(512)");

                    b.Property<DateTime?>("EndDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("ModifiedBy")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("OuterId")
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("PredicateVisualTreeSerialized")
                        .HasColumnType("longtext");

                    b.Property<string>("PricelistId")
                        .IsRequired()
                        .HasColumnType("varchar(128)");

                    b.Property<int>("Priority")
                        .HasColumnType("int");

                    b.Property<DateTime?>("StartDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("StoreId")
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.HasKey("Id");

                    b.HasIndex("PricelistId");

                    b.ToTable("PricelistAssignment", (string)null);
                });

            modelBuilder.Entity("VirtoCommerce.PricingModule.Data.Model.PricelistEntity", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Currency")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<string>("Description")
                        .HasMaxLength(512)
                        .HasColumnType("varchar(512)");

                    b.Property<string>("ModifiedBy")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("OuterId")
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

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
