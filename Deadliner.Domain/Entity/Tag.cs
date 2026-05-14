using System;
using System.Collections.Generic;

namespace Deadliner.Domain.Entity;

public partial class Tag
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string ColorHex { get; set; } = null!;

    public int UserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User User { get; set; } = null!;

    public virtual ICollection<UserTask> UserTasks { get; set; } = new List<UserTask>();
}