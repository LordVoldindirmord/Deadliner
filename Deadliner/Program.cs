using Deadliner.DAL.Context;
using Deadliner.DAL.Interfaces;
using Deadliner.DAL.Repositories;
using Deadliner.Service.Implementations;
using Deadliner.Service.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace Deadliner
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            #region cookie

            // Тут будет регистрация кук
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
            {
                options.LoginPath = "/User/Login";              // Куда редиректить, если не авторизован
                options.LogoutPath = "/User/Logout";            // Путь для выхода
                options.AccessDeniedPath = "/User/Login";       // Доступ запрещён
                options.Cookie.Name = "Deadliner";              // Имя куки
                options.ExpireTimeSpan = TimeSpan.FromDays(7);  // Время жизни
                options.SlidingExpiration = true;               // Продлевать при активности
            });

            #endregion

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            #region DAL

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found");

            builder.Services.AddDbContext<DeadlinerDbContext>(options => options.UseNpgsql(connectionString));

            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<ITaskRepository, TaskRepository>();
            builder.Services.AddScoped<ITagRepository, TagRepository>();

            #endregion

            #region Service

            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<ITaskService, TaskService>();
            builder.Services.AddScoped<ITagService, TagService>();

            #endregion

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Dashboard}/{action=Dashboard}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
