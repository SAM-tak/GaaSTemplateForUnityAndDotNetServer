﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using YourGameServer.Data;

#nullable disable

namespace YourGameServer.Migrations
{
    [DbContext(typeof(SqliteGameDbContext))]
    [Migration("20220106063430_InitialCreation")]
    partial class InitialCreation
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.1");

            modelBuilder.Entity("YourGameServer.Models.PlayerAccount", b =>
                {
                    b.Property<long>("Id")
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

                    b.Property<string>("Luid")
                        .HasMaxLength(16)
                        .HasColumnType("TEXT");

                    b.Property<long>("ProfileId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Since")
                        .HasColumnType("TEXT");

                    b.Property<int>("Status")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("Luid");

                    b.ToTable("PlayerAccounts");
                });

            modelBuilder.Entity("YourGameServer.Models.PlayerDevice", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("DeviceId")
                        .HasColumnType("TEXT");

                    b.Property<int>("DeviceType")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("LastUsed")
                        .HasColumnType("TEXT");

                    b.Property<long>("OwnerId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Since")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId");

                    b.ToTable("PlayerDevices");
                });

            modelBuilder.Entity("YourGameServer.Models.PlayerProfile", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("IconBlobId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Motto")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<long>("OwnerId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId")
                        .IsUnique();

                    b.ToTable("PlayerProfiles");
                });

            modelBuilder.Entity("YourGameServer.Models.PlayerDevice", b =>
                {
                    b.HasOne("YourGameServer.Models.PlayerAccount", "Owner")
                        .WithMany("DeviceList")
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("YourGameServer.Models.PlayerProfile", b =>
                {
                    b.HasOne("YourGameServer.Models.PlayerAccount", "Owner")
                        .WithOne("Profile")
                        .HasForeignKey("YourGameServer.Models.PlayerProfile", "OwnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("YourGameServer.Models.PlayerAccount", b =>
                {
                    b.Navigation("DeviceList");

                    b.Navigation("Profile");
                });
#pragma warning restore 612, 618
        }
    }
}