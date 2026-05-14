using Deadliner.Domain.Response;
using Deadliner.Domain.ViewModel.Dashboard;
using Deadliner.Domain.ViewModel.Tasks;

namespace Deadliner.Service.Interfaces
{
    /// <summary>
    /// Интерфейс сервиса для работы с задачами
    /// </summary>
    public interface ITaskService
    {
        /// <summary>
        /// Получить дашборд пользователя (счётчики + ближайшие задачи по тегам)
        /// </summary>
        /// <param name="userId">Id пользователя</param>
        /// <param name="limit">Максимальное количество для получения</param>
        /// <returns>Модель дашборда пользователя</returns>
        Task<BaseResponse<DashboardViewModel>> GetDashboardAsync(int userId, int limit);

        /// <summary>
        /// Получить список задач с фильтрацией и сортировкой
        /// </summary>
        /// <param name="userId">Id пользователя</param>
        /// <param name="filter">Фильтр</param>
        /// <returns>Модель списка задач</returns>
        Task<BaseResponse<TaskListViewModel>> GetFilteredAsync(int userId, TaskFilterViewModel filter);

        /// <summary>
        /// Получить детальную информацию о задаче
        /// </summary>
        /// <param name="userId">Id пользователя</param>
        /// <param name="taskId">Id Задачи</param>
        /// <returns>Модель задачи</returns>
        Task<BaseResponse<TaskDetailViewModel>> GetByIdAsync(int userId, int taskId);

        /// <summary>
        /// Создать задачу
        /// </summary>
        /// <param name="userId">Id пользователя</param>
        /// <param name="model">Модель задачи</param>
        /// <returns>Модель созданной задачи</returns>
        Task<BaseResponse<TaskDetailViewModel>> CreateAsync(int userId, TaskCreateViewModel model);

        /// <summary>
        /// Обновить задачу
        /// </summary>
        /// <param name="userId">Id пользователя</param>
        /// <param name="model">Задача для обновления</param>
        /// <returns>Обновленная задача</returns>
        Task<BaseResponse<TaskDetailViewModel>> UpdateAsync(int userId, TaskEditViewModel model);

        /// <summary>
        /// Отметить задачу как выполненную
        /// </summary>
        /// <param name="userId">Id пользователя</param>
        /// <param name="taskId">Id задачи</param>
        /// <returns></returns>
        Task<BaseResponse<bool>> CompleteAsync(int userId, int taskId);

        /// <summary>
        /// Удалить задачу
        /// </summary>
        /// <param name="userId">Id пользователя</param>
        /// <param name="taskId">Id задачи для удаления</param>
        /// <returns></returns>
        Task<BaseResponse<bool>> DeleteAsync(int userId, int taskId);
    }
}