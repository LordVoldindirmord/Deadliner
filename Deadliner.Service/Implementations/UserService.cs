using Deadliner.DAL.Interfaces;
using Deadliner.Domain.Entity;
using Deadliner.Domain.Enum;
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
        private readonly IUserTokenRepository _userTokenRepository;
        private readonly IEmailService _emailService;

        public UserService(IUserRepository userRepository, IUserTokenRepository userTokenRepository, IEmailService emailService)
        {
            _userRepository = userRepository;
            _userTokenRepository = userTokenRepository;
            _emailService = emailService;
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
                {
                    if (userByLogin.EmailConfirmed)
                        return CreatorResponse.Ok(ToProfileViewModel(userByLogin), "Вход выполнен успешно");
                    else
                        return CreatorResponse.Conflict<UserProfileViewModel>("У вас не активный Email, активируйте его!");
                }

                // Проверка по Email
                var userByEmail = await _userRepository.GetByEmailAsync(model.LoginOrEmail);

                if (userByEmail != null && BCrypt.Net.BCrypt.Verify(model.Password, userByEmail.PasswordHash))
                {
                    if(userByEmail.EmailConfirmed)
                        return CreatorResponse.Ok(ToProfileViewModel(userByEmail), "Вход выполнен успешно");
                    else
                        return CreatorResponse.Conflict<UserProfileViewModel>("У вас не активный Email, активируйте его!");
                }

                if (userByLogin == null && userByEmail == null)
                    return CreatorResponse.NotFound<UserProfileViewModel>("Пользователь с таким Email или Login не найден");

                return CreatorResponse.Unauthorized<UserProfileViewModel>("Неверный пароль");
            }
            catch (Exception ex)
            {
                return CreatorResponse.InternalError<UserProfileViewModel>($"Ошибка: {ex.Message}");
            }
        }

        // Переделать (не давай пользователю зайти до тех пор, пока он не подтвердит Email)
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
                    EmailConfirmed = false,
                };

                await _userRepository.CreateAsync(user);

                // Генерация токена и отправка письма
                var token = Guid.NewGuid().ToString("N");

                var userToken = new UserToken
                {
                    UserId = user.Id,
                    Token = token,
                    TokenType = UserTokenType.EmailConfirm,
                    ExpiresAt = DateTime.Now.AddHours(24),
                    IsUsed = false,
                    CreatedAt = DateTime.Now
                };

                await _userTokenRepository.CreateAsync(userToken);

                var confirmLink = $"https://localhost:7210/User/ConfirmEmail?token={token}";
                var body = $@"
<h2>Подтверждение регистрации</h2>
<p>Для завершения регистрации перейдите по ссылке:</p>
<p><a href='{confirmLink}'>{confirmLink}</a></p>
<p>Ссылка действительна 24 часа.</p>";

                await _emailService.SendEmailAsync(user.Email, "Подтверждение регистрации", body);

                return CreatorResponse.Ok(true, "На ваш email отправлена ссылка для подтверждения.");
            }
            catch (Exception ex)
            {
                return CreatorResponse.InternalError<bool>($"Ошибка: {ex.Message}");
            }
        }

        public async Task<BaseResponse<bool>> ConfirmEmailAsync(string token)
        {
            try
            {
                var userToken = await _userTokenRepository.GetByTokenAsync(token);

                if (userToken == null)
                    return CreatorResponse.NotFound<bool>("Токен не найден");

                if (userToken.TokenType != UserTokenType.EmailConfirm)
                    return CreatorResponse.BadRequest<bool>("Неверный тип токена");

                if (userToken.IsUsed)
                    return CreatorResponse.Conflict<bool>("Токен уже использован");

                if (userToken.ExpiresAt < DateTime.Now)
                    return CreatorResponse.BadRequest<bool>("Срок действия токена истёк");

                userToken.IsUsed = true;
                await _userTokenRepository.UpdateAsync(userToken);

                var user = await _userRepository.GetByIdAsync(userToken.UserId);

                if (user == null)
                    return CreatorResponse.NotFound<bool>("Пользователь не найден");

                user.EmailConfirmed = true;

                await _userRepository.UpdateAsync(user);

                return CreatorResponse.Ok(true, "Email успешно подтверждён. Теперь вы можете войти.");
            }
            catch (Exception ex)
            {
                return CreatorResponse.InternalError<bool>($"Ошибка: {ex.Message}");
            }
        }

        public async Task<BaseResponse<bool>> ForgotPasswordAsync(string email)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(email);

                if (user == null)
                    return CreatorResponse.NotFound<bool>("Пользователь с таким Email не найден");

                var token = Guid.NewGuid().ToString("N");

                var userToken = new UserToken
                {
                    UserId = user.Id,
                    Token = token,
                    TokenType = UserTokenType.PasswordReset,
                    ExpiresAt = DateTime.Now.AddMinutes(15),
                    IsUsed = false,
                    CreatedAt = DateTime.Now
                };

                await _userTokenRepository.CreateAsync(userToken);

                var resetLink = $"https://localhost:7210/User/ResetPassword?token={token}";
                var body = $@"
<h2>Сброс пароля</h2>
<p>Для сброса пароля перейдите по ссылке:</p>
<p><a href='{resetLink}'>{resetLink}</a></p>
<p>Ссылка действительна 15 минут.</p>";

                await _emailService.SendEmailAsync(user.Email, "Сброс пароля", body);

                return CreatorResponse.Ok(true, "Ссылка для сброса пароля отправлена на ваш email.");
            }
            catch (Exception ex)
            {
                return CreatorResponse.InternalError<bool>($"Ошибка: {ex.Message}");
            }
        }

        public async Task<BaseResponse<bool>> ResetPasswordAsync(string token, string newPassword)
        {
            try
            {
                var userToken = await _userTokenRepository.GetByTokenAsync(token);

                if (userToken == null)
                    return CreatorResponse.NotFound<bool>("Токен не найден");

                if (userToken.TokenType != UserTokenType.PasswordReset)
                    return CreatorResponse.BadRequest<bool>("Неверный тип токена");

                if (userToken.IsUsed)
                    return CreatorResponse.Conflict<bool>("Токен уже использован");

                if (userToken.ExpiresAt < DateTime.Now)
                    return CreatorResponse.BadRequest<bool>("Срок действия токена истёк");

                var user = await _userRepository.GetByIdAsync(userToken.UserId);
                if (user == null)
                    return CreatorResponse.NotFound<bool>("Пользователь не найден");

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                userToken.IsUsed = true;

                await _userRepository.UpdateAsync(user);
                await _userTokenRepository.UpdateAsync(userToken);

                return CreatorResponse.Ok(true, "Пароль успешно изменён.");
            }
            catch (Exception ex)
            {
                return CreatorResponse.InternalError<bool>($"Ошибка: {ex.Message}");
            }
        }

        public async Task<BaseResponse<bool>> ResendConfirmationAsync(string loginOrEmail)
        {
            try
            {
                var user = await _userRepository.GetByLoginAsync(loginOrEmail)
                        ?? await _userRepository.GetByEmailAsync(loginOrEmail);

                if (user == null)
                    return CreatorResponse.NotFound<bool>("Пользователь не найден");

                if (user.EmailConfirmed)
                    return CreatorResponse.Conflict<bool>("Email уже подтверждён");

                var token = Guid.NewGuid().ToString("N");
                var userToken = new UserToken
                {
                    UserId = user.Id,
                    Token = token,
                    TokenType = UserTokenType.EmailConfirm,
                    ExpiresAt = DateTime.Now.AddHours(24),
                    IsUsed = false,
                    CreatedAt = DateTime.Now
                };

                await _userTokenRepository.CreateAsync(userToken);

                var confirmLink = $"https://localhost:7210/User/ConfirmEmail?token={token}";
                var body = $@"
<h2>Подтверждение регистрации</h2>
<p>Для завершения регистрации перейдите по ссылке:</p>
<p><a href='{confirmLink}'>{confirmLink}</a></p>
<p>Ссылка действительна 24 часа.</p>";

                await _emailService.SendEmailAsync(user.Email, "Подтверждение регистрации", body);

                return CreatorResponse.Ok(true, "Письмо отправлено повторно.");
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