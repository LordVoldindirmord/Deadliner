using Deadliner.Domain.Entity;

namespace Deadliner.DAL.Interfaces
{
    /// <summary>
    /// Интерфейс репозитория для работы с тегами
    /// </summary>
    public interface ITagRepository : IBaseRepository<Tag>
    {
        /// <summary>
        /// Получить все теги пользователя
        /// </summary>
        /// <param name="userId">Id пользователя</param>
        Task<IEnumerable<Tag>> GetByUserIdAsync(int userId);

        /// <summary>
        /// Проверить, существует ли тег с таким названием у пользователя
        /// </summary>
        /// <param name="name">Название тега</param>
        /// <param name="userId">Id пользователя</param>
        /// /// <returns>true - тег с таким названием у пользователя есть, false - нет</returns>
        Task<bool> ExistsByNameAsync(string name, int userId);

        /// <summary>
        /// Получить все теги пользователя вместе с количеством задач в них
        /// </summary>
        /// <param name="userId">Id пользователя</param>
        /// <returns>Tuple(тег, количество задач)</returns>
        Task<IEnumerable<(Tag tag, int taskCount)>> GetByUserIdWithTaskCountAsync(int userId);
    }
}
