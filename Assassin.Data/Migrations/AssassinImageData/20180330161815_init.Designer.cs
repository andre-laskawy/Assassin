﻿// <auto-generated />
using Assassin.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;

namespace Assassin.Data.Migrations.AssassinImageData
{
    [DbContext(typeof(AssassinImageDataContext))]
    [Migration("20180330161815_init")]
    partial class init
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.2-rtm-10011");

            modelBuilder.Entity("Assassin.Common.Models.AssassinImage", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreatedDT");

                    b.Property<byte[]>("Image");

                    b.Property<DateTime>("ModifiedDT");

                    b.Property<string>("RelatedTypeDescription");

                    b.Property<Guid>("RelationId");

                    b.Property<bool>("Synced");

                    b.HasKey("Id");

                    b.HasIndex("CreatedDT");

                    b.HasIndex("Id")
                        .IsUnique();

                    b.HasIndex("ModifiedDT");

                    b.HasIndex("RelatedTypeDescription");

                    b.ToTable("AssassinImage");
                });
#pragma warning restore 612, 618
        }
    }
}
