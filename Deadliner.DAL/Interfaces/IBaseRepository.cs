namespace Deadliner.DAL.Interfaces
{
    /// <summary>
    /// Базовый интерфейс ответа из базы данных
    /// </summary>
    public interface IBaseRepository<T> where T : class
    {
        /// <summary>
        /// Получить все данные у сущности
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<T>> GetAll();

        /// <summary>
        /// Удалить объект
        /// </summary>
        /// <param name="entity">Объект для удаления</param>
        Task DeleteAsync(T entity);

        /// <summary>
        /// Получить по id
        /// </summary>
        /// <param name="id">id пользователя</param>
        /// <returns>Пользователь</returns>
        Task<T?> GetByIdAsync(int id);

        /// <summary>
        /// Создать объект в БД
        /// </summary>
        /// <param name="entity"></param>
        Task CreateAsync(T entity);

        /// <summary>
        /// Обновить данные у объекта
        /// </summary>
        /// <param name="entity">Обновленный объект</param>
        Task UpdateAsync(T entity);
    }
}
