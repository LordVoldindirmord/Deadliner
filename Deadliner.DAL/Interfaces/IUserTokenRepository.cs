using Deadliner.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deadliner.DAL.Interfaces
{
    public interface IUserTokenRepository : IBaseRepository<UserToken>
    {
        /// <summary>
        /// Найти токен по его значению (используется при подтверждении email и сбросе пароля)
        /// </summary>
        /// <param name="token">Значение токена</param>
        /// <returns>Токен или null, если он не найден по такому значению</returns>
        Task<UserToken?> GetByTokenAsync(string token);
    }
}
