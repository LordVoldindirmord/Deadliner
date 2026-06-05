using Deadliner.Domain.Entity;

namespace Deadliner.DAL.Interfaces
{
    /// <summary>
    /// Репозиторий для работы с привязками Telegram-аккаунтов
    /// </summary>
    public interface ITelegramBindingRepository : IBaseRepository<TelegramBinding>
    {
        /// <summary>
        /// Найти привязку по Telegram chat_id
        /// </summary>
        /// <param name="chatId">Id чата в Telegram</param>
        /// <returns>Данные привязки с Telegram</returns>
        Task<TelegramBinding?> GetByChatIdAsync(long chatId);

        /// <summary>
        /// Найти привязку по токену
        /// </summary>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Данные привязки с Telegram</returns>
        Task<TelegramBinding?> GetByTokenAsync(string token);

        /// <summary>
        /// Найти привязку по пользователю сайта
        /// </summary>
        /// <param name="userId">Id пользователя</param>
        /// <returns>Данные привязки с Telegram</returns>
        Task<TelegramBinding?> GetByUserIdAsync(int userId);
    }
}