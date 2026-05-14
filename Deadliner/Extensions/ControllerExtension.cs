using Deadliner.Domain.ViewModel.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

namespace Deadliner.ASP.Extensions
{
    public static class ControllerExtension
    {
        /// <summary>
        /// Создать зашифрованную куку
        /// </summary>
        /// <param name="controller">Расширение для контроллеров</param>
        /// <param name="userId">Id пользователя</param>
        /// <param name="userLogin">Login пользователя</param>
        /// <param name="userEmail">Email пользователя</param>
        /// <param name="rememberMe">Запомнить меня? (true - кука останестя на 7 дней)</param>
        public static async Task SignInUserAsync(this Controller controller, 
            int userId, string userLogin, string userEmail, bool rememberMe)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, userLogin),
                new Claim(ClaimTypes.Email, userEmail),
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(7) : null
            };

            await controller.HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }

        /// <summary>
        /// Выход из системы
        /// </summary>
        /// <param name="controller">Расширение для контроллеров</param>
        public static async Task SignOutUserAsync(this Controller controller)
        {
            await controller.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// Получить Id текущего пользователя
        /// </summary>
        /// <param name="controller">Расширение для контроллеров</param>
        /// <returns>Id пользователя</returns>
        public static int GetUserId(this Controller controller)
        {
            return int.Parse(controller.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "-1");
        }

        /// <summary>
        /// Получить Login текущего пользователя
        /// </summary>
        /// <param name="controller">Расширение для контроллеров</param>
        /// <returns>Login пользователя</returns>
        public static string GetUserName(this Controller controller)
        {
            return controller.User.FindFirstValue(ClaimTypes.Name) ?? "Гость";
        }
        
        /// <summary>
        /// Получить Email текущего пользователя
        /// </summary>
        /// <param name="controller">Расширение для контроллеров</param>
        /// <returns>Email пользователя</returns>
        public static string GetUserEmail(this Controller controller)
        {
            return controller.User.FindFirstValue(ClaimTypes.Email) ?? "Гость";
        }

        /// <summary>
        /// Проверить, авторизован ли пользователь
        /// </summary>
        /// <param name="controller">Расширение для контроллеров</param>
        /// <returns>true - авторизован, false - нет</returns>
        public static bool IsAuthenticated(this Controller controller)
        {
            return controller.User.Identity?.IsAuthenticated ?? false;
        }
    }
}
