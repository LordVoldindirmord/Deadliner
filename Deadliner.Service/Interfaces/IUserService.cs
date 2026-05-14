using Deadliner.Domain.Response;
using Deadliner.Domain.ViewModel.Account;

namespace Deadliner.Service.Interfaces
{
    /// <summary>
    /// Интерфейс сервиса для работы с пользователями
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Зарегистрировать нового пользователя
        /// </summary>
        /// <param name="model">Модель регистрации пользователя</param>
        /// <returns>Успешность регистрации нового пользователя</returns>
        Task<BaseResponse<bool>> RegisterAsync(RegisterViewModel model);

        /// <summary>
        /// Войти (возвращает профиль при успехе, null — неверный логин/пароль)
        /// Сначала пытается верифицировать пользователя по Login, при неудаче по Email
        /// </summary>
        /// <param name="model">Модель авторизации пользователя</param>
        /// <returns>ViewModel зарегистрированного пользователя</returns>
        Task<BaseResponse<UserProfileViewModel>> LoginAsync(LoginViewModel model);

        /// <summary>
        /// Получить профиль пользователя по id
        /// </summary>
        /// <param name="userId">Id пользователя</param>
        /// <returns>Модель авторизованного пользователя (null при ошибке получения пользователя)</returns>
        Task<BaseResponse<UserProfileViewModel>> GetByIdAsync(int userId);

        /// <summary>
        /// Полное удаление пользователя без возможности восстановления
        /// </summary>
        /// <param name="userId">Id пользователя для удаления</param>
        /// <returns>Успешность удаления пользователя</returns>
        Task<BaseResponse<bool>> DeleteAsync(int userId);
    }
}
