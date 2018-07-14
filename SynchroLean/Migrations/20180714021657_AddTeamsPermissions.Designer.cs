﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SynchroLean.Persistence;

namespace SynchroLean.Migrations
{
    [DbContext(typeof(SynchroLeanDbContext))]
    [Migration("20180714021657_AddTeamsPermissions")]
    partial class AddTeamsPermissions
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.1-rtm-30846");

            modelBuilder.Entity("SynchroLean.Core.Models.AddUserRequest", b =>
                {
                    b.Property<int>("AddUserRequestId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("DestinationTeamId");

                    b.Property<int>("InviteeOwnerId");

                    b.Property<int?>("InviterOwnerId");

                    b.Property<bool>("IsAuthorized");

                    b.HasKey("AddUserRequestId");

                    b.HasIndex("DestinationTeamId");

                    b.HasIndex("InviteeOwnerId");

                    b.HasIndex("InviterOwnerId");

                    b.ToTable("AddUserRequests");
                });

            modelBuilder.Entity("SynchroLean.Core.Models.Team", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("OwnerId");

                    b.Property<string>("TeamDescription")
                        .HasMaxLength(250);

                    b.Property<string>("TeamName")
                        .IsRequired()
                        .HasMaxLength(25);

                    b.HasKey("Id");

                    b.ToTable("Teams");
                });

            modelBuilder.Entity("SynchroLean.Core.Models.TeamPermission", b =>
                {
                    b.Property<int>("SubjectTeamId");

                    b.Property<int>("ObjectTeamId");

                    b.HasKey("SubjectTeamId", "ObjectTeamId");

                    b.HasIndex("ObjectTeamId");

                    b.ToTable("TeamPermissions");
                });

            modelBuilder.Entity("SynchroLean.Core.Models.UserAccount", b =>
                {
                    b.Property<int>("OwnerId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.Property<bool>("IsDeleted");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.Property<int>("TeamId");

                    b.HasKey("OwnerId");

                    b.ToTable("UserAccounts");
                });

            modelBuilder.Entity("SynchroLean.Core.Models.UserTask", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CompletionDate");

                    b.Property<DateTime>("CreationDate");

                    b.Property<string>("Description");

                    b.Property<bool>("IsCompleted");

                    b.Property<bool>("IsRecurring");

                    b.Property<bool>("IsRemoved");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.Property<int>("OwnerId");

                    b.Property<byte>("Weekdays");

                    b.HasKey("Id");

                    b.ToTable("UserTasks");
                });

            modelBuilder.Entity("SynchroLean.Core.Models.AddUserRequest", b =>
                {
                    b.HasOne("SynchroLean.Core.Models.Team", "DestinationTeam")
                        .WithMany()
                        .HasForeignKey("DestinationTeamId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("SynchroLean.Core.Models.UserAccount", "Invitee")
                        .WithMany()
                        .HasForeignKey("InviteeOwnerId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("SynchroLean.Core.Models.UserAccount", "Inviter")
                        .WithMany()
                        .HasForeignKey("InviterOwnerId");
                });

            modelBuilder.Entity("SynchroLean.Core.Models.TeamPermission", b =>
                {
                    b.HasOne("SynchroLean.Core.Models.Team", "ObjectTeam")
                        .WithMany()
                        .HasForeignKey("ObjectTeamId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("SynchroLean.Core.Models.Team", "SubjectTeam")
                        .WithMany()
                        .HasForeignKey("SubjectTeamId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
