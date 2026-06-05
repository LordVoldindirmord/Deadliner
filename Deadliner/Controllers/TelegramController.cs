using Deadliner.ASP.Extensions;
using Deadliner.Domain.DTO;
using Deadliner.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Deadliner.ASP.Controllers
{
    [Authorize]
    public class TelegramController : Controller
    {
        ITelegramBindingService _telegramService;

        public TelegramController(ITelegramBindingService telegramService)
        {
            _telegramService = telegramService;
        }

        /// <summary>
        /// Страница привязки Telegram
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var response = await _telegramService.GetStatusAsync(this.GetUserId());

            if (!response.IsSuccess || response.Data == null)
            {
                if (response.Data == null)
                    ModelState.AddModelError(string.Empty, "Не удалось загрузить статус привязки");
                return View(new TelegramBindingDto());
            }

            return View(response.Data);
        }

        /// <summary>
        /// Создать привязку (получить ссылку)
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateBind()
        {
            var response = await _telegramService.StartBindingAsync(this.GetUserId());

            if (!response.IsSuccess)
            {
                TempData["Error"] = response.Description ?? "Ошибка при создании привязки";
                return RedirectToAction("Index");
            }

            return View("BindLink", response.Data);
        }

        /// <summary>
        /// Отвязать Telegram аккаунта
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Unbind()
        {
            var response = await _telegramService.UnbindAsync(this.GetUserId());

            if (!response.IsSuccess)
            {
                TempData["Error"] = response.Description ?? "Ошибка при отвязке Telegram аккаунта";
            }

            return RedirectToAction("Index");
        }
    }
}
