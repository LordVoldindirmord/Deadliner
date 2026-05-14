using Deadliner.Domain.Entity;
using Deadliner.Domain.Enum;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace Deadliner.DAL.Context;

public partial class DeadlinerDbContext : DbContext
{
    public DeadlinerDbContext(DbContextOptions<DeadlinerDbContext> options) : base(options) { }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserTask> UserTasks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tags_pkey");

            entity.ToTable("tags");

            entity.HasIndex(e => e.UserId, "idx_tags_user_id");

            entity.HasIndex(e => new { e.Name, e.UserId }, "uq_tags_name_user").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ColorHex)
                .HasMaxLength(7)
                .HasDefaultValueSql("'#6C757D'::bpchar")
                .IsFixedLength()
                .HasColumnName("color_hex");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Tags)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("tags_user_id_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "idx_users_email");

            entity.HasIndex(e => e.Id, "idx_users_id");

            entity.HasIndex(e => e.Login, "idx_users_login");

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.HasIndex(e => e.Login, "users_login_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Login)
                .HasMaxLength(100)
                .HasColumnName("login");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
        });

        modelBuilder.Entity<UserTask>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_tasks_pkey");

            entity.ToTable("user_tasks");

            entity.HasIndex(e => e.Deadline, "idx_tasks_deadline");

            entity.HasIndex(e => e.Status, "idx_tasks_status");

            entity.HasIndex(e => e.TagId, "idx_tasks_tag_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CompletedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("completed_at");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Deadline)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("deadline");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Priority)
                .HasMaxLength(20)
                .HasDefaultValueSql("'medium'::character varying")
                .HasColumnName("priority")
                .HasConversion( // enum не сгенерировались, делаем ручками
                e => ConvertPriorityToString(e), // enum -> БД
                e => ConvertStringToPriority(e)); // БД -> enum

            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValueSql("'active'::character varying")
                .HasColumnName("status")
                .HasConversion( // enum не сгенерировался, тоже для конвертации
                e => ConvertStatusToString(e), // enum -> БД
                e => ConvertStringToStatus(e)); // БД -> enum

            entity.Property(e => e.TagId).HasColumnName("tag_id");
            entity.Property(e => e.Title)
                .HasMaxLength(500)
                .HasColumnName("title");

            entity.HasOne(d => d.Tag).WithMany(p => p.UserTasks)
                .HasForeignKey(d => d.TagId)
                .HasConstraintName("user_tasks_tag_id_fkey");
        });
    }

    // Вспомогательные методы для конвертации enum
    private static string ConvertPriorityToString(PriorityStatus priority)
    {
        return priority switch
        {
            PriorityStatus.Low => "low",
            PriorityStatus.Medium => "medium",
            PriorityStatus.High => "high",
            PriorityStatus.Critical => "critical",
            _ => throw new ArgumentOutOfRangeException(nameof(priority), priority, "Неизвестный приоритет")
        };
    }

    private static PriorityStatus ConvertStringToPriority(string value)
    {
        return value switch
        {
            "low" => PriorityStatus.Low,
            "medium" => PriorityStatus.Medium,
            "high" => PriorityStatus.High,
            "critical" => PriorityStatus.Critical,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Неизвестный приоритет в БД")
        };
    }

    private static string ConvertStatusToString(TasksStatus status)
    {
        return status switch
        {
            TasksStatus.Active => "active",
            TasksStatus.Completed => "completed",
            TasksStatus.Cancelled => "cancelled",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Неизвестный статус")
        };
    }

    private static TasksStatus ConvertStringToStatus(string value)
    {
        return value switch
        {
            "active" => TasksStatus.Active,
            "completed" => TasksStatus.Completed,
            "cancelled" => TasksStatus.Cancelled,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Неизвестный статус в БД")
        };
    }
}
