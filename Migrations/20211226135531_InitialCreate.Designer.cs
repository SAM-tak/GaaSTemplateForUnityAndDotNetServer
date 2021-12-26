﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using YourProjectName.Data;

#nullable disable

namespace Server.Migrations
{
    [DbContext(typeof(SqliteGameDbContext))]
    [Migration("20211226135531_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.1");

            modelBuilder.Entity("YourProjectName.Models.PlayerAccount", b =>
                {
                    b.Property<long>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("BanDate")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("ExpireDate")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("InactivateDate")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("LastLogin")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Since")
                        .HasColumnType("TEXT");

                    b.Property<int>("Status")
                        .HasColumnType("INTEGER");

                    b.HasKey("ID");

                    b.ToTable("PlayerAccount", (string)null);
                });
#pragma warning restore 612, 618
        }
    }
}
