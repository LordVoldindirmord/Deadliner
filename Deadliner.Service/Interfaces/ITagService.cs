using Deadliner.Domain.Response;
using Deadliner.Domain.ViewModel.Tags;

namespace Deadliner.Service.Interfaces
{
    /// <summary>
    /// Интерфейс сервиса для работы с тегами
    /// </summary>
    public interface ITagService
    {
        /// <summary>
        /// Получить тег по id
        /// </summary>
        /// <param name="userId">Id пользователя</param>
        /// <param name="tagId">Id тега</param>
        /// <returns>Модель тега</returns>
        Task<BaseResponse<TagViewModel>> GetByIdAsync(int userId, int tagId);

        /// <summary>
        /// Получить все теги пользователя
        /// </summary>
        /// <param name="userId">Id пользователя</param>
        /// <returns>Коллекция тегов пользователя</returns>
        Task<BaseResponse<IEnumerable<TagViewModel>>> GetByUserIdAsync(int userId);

        /// <summary>
        /// Создать тег
        /// </summary>
        /// <param name="userId">Id пользователя, который создает тег</param>
        /// <param name="model">Модель тега для создания</param>
        /// <returns>Успешность создания тега</returns>
        Task<BaseResponse<bool>> CreateAsync(int userId, TagCreateViewModel model);

        /// <summary>
        /// Обновить тег
        /// </summary>
        /// <param name="userId">Id пользователя, которому принадлежит данный тег</param>
        /// <param name="model">Модель тега для обновления</param>
        /// <returns>Модель обновленного тега</returns>
        Task<BaseResponse<TagViewModel>> UpdateAsync(int userId, TagEditViewModel model);

        /// <summary>
        /// Удалить тег (и все задачи внутри него)
        /// </summary>
        /// <param name="userId">Id пользователя, которому принадлежит данный тег</param>
        /// <param name="tagId">Id тега для удаления</param>
        Task<BaseResponse<bool>> DeleteAsync(int userId, int tagId);
    }
}
