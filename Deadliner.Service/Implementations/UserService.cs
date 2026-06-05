using Deadliner.DAL.Interfaces;
using Deadliner.Domain.Entity;
using Deadliner.Domain.Response;
using Deadliner.Domain.ViewModel.Account;
using Deadliner.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deadliner.Service.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<BaseResponse<bool>> DeleteAsync(int userId)
        {
            try
            {
                // Проверка существования такого пользователя
                var user = await _userRepository.GetByIdAsync(userId);

                if (user == null)
                    return CreatorResponse.BadRequest<bool>("Пользователь с таким id не найден", false);

                await _userRepository.DeleteAsync(user);

                return CreatorResponse.Ok(true);
            }
            catch (Exception ex)
            {
                return CreatorResponse.InternalError<bool>($"Ошибка: {ex.Message}");
            }
        }

        public async Task<BaseResponse<UserProfileViewModel>> GetByIdAsync(int userId)
        {
            try
            {
                var response = await _userRepository.GetByIdAsync(userId);

                if (response == null)
                    return CreatorResponse.NotFound<UserProfileViewModel>("Пользователя с таким Id не существует");

                // null до сюда не дойдет, на предупреждение не обращать внимания
                return CreatorResponse.Ok(ToProfileViewModel(response));
            }
            catch (Exception ex)
            {
                return CreatorResponse.InternalError<UserProfileViewModel>($"Ошибка: {ex.Message}");
            }
        }

        public async Task<BaseResponse<UserProfileViewModel>> LoginAsync(LoginViewModel model)
        {
            try
            {
                // Проверка по Login
                var userByLogin = await _userRepository.GetByLoginAsync(model.LoginOrEmail);

                if (userByLogin != null && BCrypt.Net.BCrypt.Verify(model.Password, userByLogin.PasswordHash))
                    return CreatorResponse.Ok(ToProfileViewModel(userByLogin), "Вход выполнен успешно");

                // Проверка по Email
                var userByEmail = await _userRepository.GetByEmailAsync(model.LoginOrEmail);

                if (userByEmail != null && BCrypt.Net.BCrypt.Verify(model.Password, userByEmail.PasswordHash))
                    return CreatorResponse.Ok(ToProfileViewModel(userByEmail), "Вход выполнен успешно");

                if (userByLogin == null && userByEmail == null)
                    return CreatorResponse.NotFound<UserProfileViewModel>("Пользователь с таким Email или Login не найден");

                return CreatorResponse.Unauthorized<UserProfileViewModel>("Неверный пароль");
            }
            catch (Exception ex)
            {
                return CreatorResponse.InternalError<UserProfileViewModel>($"Ошибка: {ex.Message}");
            }
        }

        public async Task<BaseResponse<bool>> RegisterAsync(RegisterViewModel model)
        {
            try
            {
                // Проверка существования пользователя с таким Email
                var userByEmail = await _userRepository.ExistByEmailAsync(model.Email);

                if (userByEmail)
                    return CreatorResponse.Conflict<bool>("Пользователь с таким Email уже существует");

                // Проверка существования пользователя с таким Login
                var userByLogin = await _userRepository.ExistByLoginAsync(model.Login);

                if (userByLogin)
                    return CreatorResponse.Conflict<bool>("Пользователь с таким Login уже существует");

                // Все хорошо, регистрируем пользователя
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

                User user = new User
                {
                    Login = model.Login,
                    Email = model.Email,
                    PasswordHash = passwordHash,
                    CreatedAt = DateTime.Now,
                };

                await _userRepository.CreateAsync(user);

                return CreatorResponse.Ok(true);
            }
            catch (Exception ex)
            {
                return CreatorResponse.InternalError<bool>($"Ошибка: {ex.Message}");
            }
        }

        /// <summary>
        /// Вспомогательный метод для перевода из User в ViewModel
        /// </summary>
        /// <param name="user">Сущность (модель БД)</param>
        /// <returns>ViewModel или null в случае, если передается null</returns>
        private static UserProfileViewModel ToProfileViewModel(User user) =>
            new UserProfileViewModel
            {
                Id = user.Id,
                Login = user.Login,
                Email = user.Email,
                CreatedAt = user.CreatedAt,
            };
    }
}