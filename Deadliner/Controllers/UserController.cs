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
                if (response.StatusCode == Domain.Enum.StatusCode.Conflict
                    && response.Description?.Contains("Email") == true)
                {
                    ViewBag.NeedConfirmation = true;
                    ViewBag.ConfirmationEmail = model.LoginOrEmail;
                    return View(model);  // Возвращаем без ModelState.AddModelError
                }

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

            TempData["Message"] = response.Description ?? "Проверьте почту для подтверждения";
            return RedirectToAction("Login");
        }

        /// <summary>
        /// Подтверждение email по токену из письма
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string token)
        {
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login");

            var response = await _userService.ConfirmEmailAsync(token);

            if (!response.IsSuccess)
            {
                TempData["Error"] = response.Description ?? "Ошибка при подтверждении email";
                return RedirectToAction("Login");
            }

            TempData["Message"] = response.Description ?? "Email подтверждён";
            return RedirectToAction("Login");
        }

        /// <summary>
        /// Форма "Забыли пароль"
        /// </summary>
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError(string.Empty, "Введите Email");
                return View();
            }

            var response = await _userService.ForgotPasswordAsync(email);

            if (!response.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, response.Description ?? "Ошибка");
                return View();
            }

            TempData["Message"] = response.Description ?? "Проверьте почту";
            return RedirectToAction("Login");
        }

        /// <summary>
        /// Форма сброса пароля (переход по ссылке из письма)
        /// </summary>
        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login");

            return View(new ResetPasswordViewModel { Token = token });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var response = await _userService.ResetPasswordAsync(model.Token, model.NewPassword);

            if (!response.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, response.Description ?? "Ошибка");
                return View(model);
            }

            TempData["Message"] = response.Description ?? "Пароль изменён";
            return RedirectToAction("Login");
        }

        /// <summary>
        /// Повторно отправить письмо для подтверждения email
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ResendConfirmation(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Введите Email или Login";
                return RedirectToAction("Login");
            }

            var response = await _userService.ResendConfirmationAsync(email);

            if (!response.IsSuccess)
                TempData["Error"] = response.Description ?? "Ошибка при отправке";
            else
                TempData["Message"] = response.Description ?? "Проверьте почту";

            return RedirectToAction("Login");
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
