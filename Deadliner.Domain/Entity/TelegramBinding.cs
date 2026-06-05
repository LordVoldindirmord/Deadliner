using System;
using System.Collections.Generic;
using System.Text;

namespace Deadliner.Domain.Entity
{
    public partial class TelegramBinding
    {
        public int Id { get; set; }

        public int? UserId { get; set; }

        public long? TelegramChatId { get; set; }

        public string? BindCode { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual User? User { get; set; }
    }
}
