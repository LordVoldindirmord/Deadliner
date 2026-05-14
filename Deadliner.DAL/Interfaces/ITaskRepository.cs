using Deadliner.Domain.Entity;
using Deadliner.Domain.Enum;

namespace Deadliner.DAL.Interfaces
{
    /// <summary>
    /// Интерфейс репозитория для работы с задачами
    /// </summary>
    public interface ITaskRepository : IBaseRepository<UserTask>
    {
        /// <summary>
        /// Получить все задачи тега
        /// </summary>
        /// <param name="tagId">Id тега</param>
        /// <returns>Список задач пользователя</returns>
        Task<IEnumerable<UserTask>> GetByTagIdAsync(int tagId);

        /// <summary>
        /// Получить все задачи пользователя (по всем его тегам)
        /// </summary>
        /// <param name="userId">Id пользователя</param>
        /// <returns>Список задач тега</returns>
        Task<IEnumerable<UserTask>> GetByUserIdAsync(int userId);

        /// <summary>
        /// Получить задачи пользователя по статусу
        /// </summary>
        /// <param name="userId">Id пользователя</param>
        /// <param name="status">Статус</param>
        /// <returns>Список задач пользователя по статусу</returns>
        Task<IEnumerable<UserTask>> GetByStatusAsync(int userId, TasksStatus status);

        /// <summary>
        /// Получить просроченные задачи пользователя
        /// </summary>
        /// <param name="userId">Id пользователя</param>
        /// <returns>Список просроченных задач тега</returns>
        Task<IEnumerable<UserTask>> GetOverdueAsync(int userId);

        /// <summary>
        /// Проверить, существует ли задача с таким названием в теге
        /// </summary>
        /// <param name="title">Название задачи</param>
        /// <param name="tagId">Id тега</param>
        /// <returns>true - существует, false - нет</returns>
        Task<bool> ExistsByTitleAsync(string title, int tagId);
    }
}
