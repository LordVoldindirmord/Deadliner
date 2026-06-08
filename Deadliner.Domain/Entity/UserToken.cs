using Deadliner.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deadliner.Domain.Entity
{
    public partial class UserToken
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string Token { get; set; } = null!;

        public UserTokenType TokenType { get; set; }

        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
