using System;
using System.Collections.Generic;
using System.Text;

namespace Deadliner.Domain.DTO
{
    /// <summary>
    /// Информация для привязки Telegram
    /// </summary>
    public class TelegramBindingDto
    {
        /// <summary>
        /// Привязан ли Telegram
        /// </summary>
        public bool IsBound { get; set; }

        /// <summary>
        /// Дата привязки (если привязан)
        /// </summary>
        public DateTime? BindedAt { get; set; }
    }
}
