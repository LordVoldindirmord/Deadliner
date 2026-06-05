using System;
using System.Collections.Generic;
using System.Text;

namespace Deadliner.Domain.DTO
{
    /// <summary>
    /// Токен для привязки Telegram
    /// </summary>
    public class TelegramTokenDTO
    {
        /// <summary>
        /// Токен
        /// </summary>
        public string Token { get; set; } = null!;

        /// <summary>
        /// Ссылка для пользователя (t.me/bot?start=TOKEN)
        /// </summary>
        public string DeepLink { get; set; } = null!;

        /// <summary>
        /// Срок действия токена (то есть до какого времени он будет действителен)
        /// </summary>
        public DateTime ExpiresAt { get; set; }
    }
}
