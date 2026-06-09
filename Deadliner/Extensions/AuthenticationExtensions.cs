using Microsoft.AspNetCore.Authentication.Cookies;

namespace Deadliner.ASP.Extensions
{
    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddDeadlinerAuthentication(this IServiceCollection services)
        {
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/User/Login";              // Куда редиректить, если не авторизован
                    options.LogoutPath = "/User/Logout";            // Путь для выхода
                    options.AccessDeniedPath = "/User/Login";       // Доступ запрещён
                    options.Cookie.Name = "Deadliner";              // Имя куки
                    options.ExpireTimeSpan = TimeSpan.FromDays(7);  // Время жизни
                    options.SlidingExpiration = true;               // Продлевать при активности
                });

            return services;
        }
    }
}
