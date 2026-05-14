using Deadliner.Domain.Entity;

namespace Deadliner.DAL.Interfaces
{
    /// <summary>
    /// Интерфейс репозитория для работы с пользователями
    /// </summary>
    public interface IUserRepository : IBaseRepository<User>
    {
        /// <summary>
        /// Проверка существования пользователя с таким Email
        /// </summary>
        /// <param name="email">Email для проверки существования в БД</param>
        /// <returns>true - есть пользователь с таким email, false - нет пользователя с таким email</returns>
        Task<bool> ExistByEmailAsync(string email);

        /// <summary>
        /// Проверка существования пользователя с таким Login
        /// </summary>
        /// <param name="login">Login для проверки существования в БД</param>
        /// <returns>true - есть пользователь с таким login, false - нет пользователя с таким login</returns>
        Task<bool> ExistByLoginAsync(string login);

        /// <summary>
        /// Получить пользователя по email
        /// </summary>
        /// <param name="email">email пользователя</param>
        /// <returns>Пользователь</returns>
        Task<User?> GetByEmailAsync(string email);

        /// <summary>
        /// Получить пользователя по login
        /// </summary>
        /// <param name="login">login пользователя</param>
        /// <returns>Пользователь</returns>
        Task<User?> GetByLoginAsync(string login);
    }
}