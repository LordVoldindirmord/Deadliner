using Deadliner.ASP.Extensions;
using Deadliner.Domain.ViewModel.Tags;
using Deadliner.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Deadliner.ASP.Controllers
{
    public class TagController : Controller
    {
        private readonly ITagService _tagService;

        public TagController(ITagService tagService)
        {
            _tagService = tagService;
        }

        /// <summary>
        /// Получить все теги пользователя
        /// </summary>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var response = await _tagService.GetByUserIdAsync(this.GetUserId());

            if (!response.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, response.Description ?? "Ошибка при получении тегов");
                return View(new List<TagViewModel>());
            }

            if (response.StatusCode == Domain.Enum.StatusCode.NoContent)
            {
                return View(new List<TagViewModel>());
            }

            return View(response.Data);
        }

        /// <summary>
        /// Форма для создания нового тега
        /// </summary>
        [Authorize]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(TagCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var response = await _tagService.CreateAsync(this.GetUserId(), model);

            if(!response.IsSuccess || !response.Data)
            {
                ModelState.AddModelError(string.Empty, response.Description ?? "Ошибка при создании тега");
                return View(model);
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Форма для редактирования тега
        /// </summary>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(int tagId)
        {
            var response = await _tagService.GetByIdAsync(this.GetUserId(), tagId);

            if (!response.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, response.Description ?? "Ошибка при получении тега");
                return View(new TagEditViewModel());
            }

            var tag = response.Data;

            return View(new TagEditViewModel()
            {
                Id = tagId,
                Name = tag?.Name ?? "Без имени",
                ColorHex = tag?.ColorHex,
            });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Edit(TagEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var response = await _tagService.UpdateAsync(this.GetUserId(), model);

            if (!response.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, response.Description ?? "Ошибка при создании тега");
                return View();
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Удаление тега
        /// </summary>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Delete(int tagId)
        {
            var response = await _tagService.DeleteAsync(this.GetUserId(), tagId);

            if (!response.IsSuccess)
            {
                TempData["Error"] = response.Description ?? "Ошибка при удалении тега";
            }

            return RedirectToAction("Index");
        }
    }
}
