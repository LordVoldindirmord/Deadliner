using System;
using System.Collections.Generic;

namespace Deadliner.Domain.Entity;

public partial class User
{
    public int Id { get; set; }

    public string Login { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}