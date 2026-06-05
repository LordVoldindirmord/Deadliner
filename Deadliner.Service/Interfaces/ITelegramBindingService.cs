using Deadliner.Domain.DTO;
using Deadliner.Domain.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deadliner.Service.Interfaces
{
    /// <summary>
    /// Сервис для привязки Telegram-аккаунта
    /// </summary>
    public interface ITelegramBindingService
    {
        /// <summary>
        /// Получить статус привязки пользователя
        /// </summary>
        /// <param name="userId">Id пользователя на сайте</param>
        /// <returns>Информация для привязки Telegram</returns>
        Task<BaseResponse<TelegramBindingDto>> GetStatusAsync(int userId);

        /// <summary>
        /// Создать токен и начать процесс привязки
        /// </summary>
        /// <param name="userId">Id пользователя</param>
        /// <returns>Токен для привязки Telegram</returns>
        Task<BaseResponse<TelegramTokenDTO>> StartBindingAsync(int userId);

        /// <summary>
        /// Активировать привязку по токену.
        /// </summary>
        /// <param name="token">Токен из ссылки t.me/bot?start=TOKEN</param>
        /// <param name="chatId">Id чата Telegram</param>
        /// <returns>Успешность операции</returns>
        Task<BaseResponse<bool>> ActivateAsync(string token, long chatId);

        /// <summary>
        /// Отвязать Telegram от аккаунта
        /// </summary>
        /// <param name="userId">Id пользователя</param>
        /// <returns>Успешность операции</returns>
        Task<BaseResponse<bool>> UnbindAsync(int userId);

        /// <summary>
        /// Получить userId по chat_id
        /// </summary>
        /// <param name="chatId">Id чата Telegram</param>
        /// <returns>Id пользователя</returns>
        Task<BaseResponse<int?>> GetUserIdByChatIdAsync(long chatId);
    }
}
