using Deadliner.ASP.Extensions;
using Deadliner.Domain.ViewModel.Account;
using Deadliner.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Deadliner.ASP.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Форма для авторизации
        /// </summary>
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View();

            var response = await _userService.LoginAsync(model);

            if (!response.IsSuccess || response.Data == null)
            {
                ModelState.AddModelError(string.Empty, response.Description ?? "Ошибка при авторизации");
                return View(model);
            }

            var userProfile = response.Data;

            // Регистрация кук
            await this.SignInUserAsync(userProfile.Id, userProfile.Login, userProfile.Email, model.RememberMe);

            //return RedirectToAction("MainMenu", "Tag");
            return RedirectToAction("Dashboard", "Dashboard");
        }

        /// <summary>
        /// Форма для регистрации
        /// </summary>
        [HttpGet]
        public IActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registration(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View();

            var response = await _userService.RegisterAsync(model);

            if (!response.IsSuccess || response.Data == false)
            {
                ModelState.AddModelError(string.Empty, response.Description ?? "Ошибка при регистрации");
                return View(model);
            }

            return RedirectToAction("Dashboard", "Dashboard");
        }

        /// <summary>
        /// Выйти из аккаунта
        /// </summary>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await this.SignOutUserAsync();
            return RedirectToAction("Login");
        }
    }
}
