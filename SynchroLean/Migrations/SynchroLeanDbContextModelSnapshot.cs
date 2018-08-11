﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SynchroLean.Persistence;

namespace SynchroLean.Migrations
{
    [DbContext(typeof(SynchroLeanDbContext))]
    partial class SynchroLeanDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.1-rtm-30846");

            modelBuilder.Entity("SynchroLean.Core.Models.AddUserRequest", b =>
                {
                    b.Property<int>("AddUserRequestId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("DestinationTeamId");

                    b.Property<string>("InviteeEmail")
                        .IsRequired();

                    b.Property<string>("InviterEmail");

                    b.Property<bool>("IsAuthorized");

                    b.HasKey("AddUserRequestId");

                    b.HasIndex("DestinationTeamId");

                    b.HasIndex("InviteeEmail");

                    b.HasIndex("InviterEmail");

                    b.ToTable("AddUserRequests");
                });

            modelBuilder.Entity("SynchroLean.Core.Models.CompletionLogEntry", b =>
                {
                    b.Property<int>("TaskId");

                    b.Property<string>("OwnerEmail");

                    b.Property<DateTime>("EntryTime");

                    b.Property<bool>("IsCompleted");

                    b.Property<int?>("TeamId");

                    b.HasKey("TaskId", "OwnerEmail", "EntryTime");

                    b.HasIndex("OwnerEmail");

                    b.HasIndex("TeamId");

                    b.ToTable("TaskCompletionLog");
                });

            modelBuilder.Entity("SynchroLean.Core.Models.Team", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime?>("Deleted");

                    b.Property<string>("OwnerEmail")
                        .IsRequired();

                    b.Property<string>("TeamDescription")
                        .HasMaxLength(250);

                    b.Property<string>("TeamName")
                        .IsRequired()
                        .HasMaxLength(25);

                    b.HasKey("Id");

                    b.ToTable("Teams");
                });

            modelBuilder.Entity("SynchroLean.Core.Models.TeamMember", b =>
                {
                    b.Property<int>("TeamId");

                    b.Property<string>("MemberEmail");

                    b.HasKey("TeamId", "MemberEmail");

                    b.HasIndex("MemberEmail");

                    b.ToTable("TeamMembers");
                });

            modelBuilder.Entity("SynchroLean.Core.Models.TeamPermission", b =>
                {
                    b.Property<int>("SubjectTeamId");

                    b.Property<int>("ObjectTeamId");

                    b.HasKey("SubjectTeamId", "ObjectTeamId");

                    b.HasIndex("ObjectTeamId");

                    b.ToTable("TeamPermissions");
                });

            modelBuilder.Entity("SynchroLean.Core.Models.Todo", b =>
                {
                    b.Property<int>("TaskId");

                    b.Property<DateTime?>("Completed");

                    b.Property<DateTime>("Expires");

                    b.HasKey("TaskId");

                    b.ToTable("Todos");
                });

            modelBuilder.Entity("SynchroLean.Core.Models.UserAccount", b =>
                {
                    b.Property<string>("Email")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(50);

                    b.Property<DateTime?>("Deleted");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.Property<string>("Password")
                        .IsRequired();

                    b.Property<string>("Salt")
                        .IsRequired();

                    b.HasKey("Email");

                    b.ToTable("UserAccounts");
                });

            modelBuilder.Entity("SynchroLean.Core.Models.UserTask", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreationDate");

                    b.Property<DateTime?>("Deleted");

                    b.Property<string>("Description");

                    b.Property<int>("Frequency");

                    b.Property<bool>("IsRecurring");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255);

                    b.Property<string>("OwnerEmail");

                    b.Property<int?>("TeamId");

                    b.Property<byte>("Weekdays");

                    b.HasKey("Id");

                    b.HasIndex("OwnerEmail");

                    b.HasIndex("TeamId");

                    b.ToTable("UserTasks");
                });

            modelBuilder.Entity("SynchroLean.Core.Models.AddUserRequest", b =>
                {
                    b.HasOne("SynchroLean.Core.Models.Team", "DestinationTeam")
                        .WithMany("Invites")
                        .HasForeignKey("DestinationTeamId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("SynchroLean.Core.Models.UserAccount", "Invitee")
                        .WithMany("IncomingInvites")
                        .HasForeignKey("InviteeEmail")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("SynchroLean.Core.Models.UserAccount", "Inviter")
                        .WithMany("OutgoingInvites")
                        .HasForeignKey("InviterEmail")
                        .OnDelete(DeleteBehavior.SetNull);
                });

            modelBuilder.Entity("SynchroLean.Core.Models.CompletionLogEntry", b =>
                {
                    b.HasOne("SynchroLean.Core.Models.UserAccount", "Owner")
                        .WithMany()
                        .HasForeignKey("OwnerEmail")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("SynchroLean.Core.Models.UserTask", "Task")
                        .WithMany()
                        .HasForeignKey("TaskId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("SynchroLean.Core.Models.Team", "Team")
                        .WithMany("AssociatedLogEntries")
                        .HasForeignKey("TeamId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("SynchroLean.Core.Models.TeamMember", b =>
                {
                    b.HasOne("SynchroLean.Core.Models.UserAccount", "Member")
                        .WithMany("TeamMembershipRelations")
                        .HasForeignKey("MemberEmail")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("SynchroLean.Core.Models.Team", "Team")
                        .WithMany("TeamMembershipRelations")
                        .HasForeignKey("TeamId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("SynchroLean.Core.Models.TeamPermission", b =>
                {
                    b.HasOne("SynchroLean.Core.Models.Team", "ObjectTeam")
                        .WithMany("PermissionsWhereThisIsObject")
                        .HasForeignKey("ObjectTeamId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("SynchroLean.Core.Models.Team", "SubjectTeam")
                        .WithMany("PermissionsWhereThisIsSubject")
                        .HasForeignKey("SubjectTeamId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("SynchroLean.Core.Models.Todo", b =>
                {
                    b.HasOne("SynchroLean.Core.Models.UserTask", "Task")
                        .WithOne("Todo")
                        .HasForeignKey("SynchroLean.Core.Models.Todo", "TaskId")
                        .HasConstraintName("FK_Todo_Tasks_TaskId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("SynchroLean.Core.Models.UserTask", b =>
                {
                    b.HasOne("SynchroLean.Core.Models.UserAccount", "Owner")
                        .WithMany("Tasks")
                        .HasForeignKey("OwnerEmail");

                    b.HasOne("SynchroLean.Core.Models.Team", "Team")
                        .WithMany("AssociatedTasks")
                        .HasForeignKey("TeamId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
