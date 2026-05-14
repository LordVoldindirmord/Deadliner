using Deadliner.Domain.Entity;
using Deadliner.Domain.Enum;
using System;
using System.Collections.Generic;

namespace Deadliner.Domain.Entity;

public partial class UserTask
{
    public int Id { get; set; }

    public int TagId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public PriorityStatus Priority { get; set; }

    public TasksStatus Status { get; set; }

    public DateTime? Deadline { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Tag Tag { get; set; } = null!;
}