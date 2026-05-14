using Deadliner.ASP.Extensions;
using Deadliner.Domain.ViewModel.Tags;
using Deadliner.Domain.ViewModel.Tasks;
using Deadliner.Service.Implementations;
using Deadliner.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Deadliner.ASP.Controllers
{
    public class TaskController : Controller
    {
        private readonly ITaskService _taskService;
        private readonly ITagService _tagService;

        public TaskController(ITaskService taskService, ITagService tagService)
        {
            _taskService = taskService;
            _tagService = tagService;
        }

        /// <summary>
        /// Список задач с фильтрацией
        /// </summary>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index(TaskFilterViewModel filter)
        {
            var response = await _taskService.GetFilteredAsync(this.GetUserId(), filter);

            if (!response.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, response.Description ?? "Ошибка при получении задач");
                return View(new TaskListViewModel());
            }

            if(response.StatusCode == Domain.Enum.StatusCode.NoContent || response.Data == null)
            {
                return View(new TaskListViewModel());
            }

            return View(response.Data);
        }

        /// <summary>
        /// Детальный просмотри задачи
        /// </summary>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Details(int taskId)
        {
            var response = await _taskService.GetByIdAsync(this.GetUserId(), taskId);

            if (!response.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, response.Description ?? "Ошибка при получении задачи");
                return View(new TaskDetailViewModel());
            }

            return View(response.Data);
        }

        /// <summary>
        /// Создать задачу
        /// </summary>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var tagsResponse = await _tagService.GetByUserIdAsync(this.GetUserId());

            if(!tagsResponse.IsSuccess || tagsResponse.StatusCode == Domain.Enum.StatusCode.NoContent || tagsResponse.Data == null)
            {
                ViewBag.Tags = new List<TagViewModel>();
                return View();
            }

            ViewBag.Tags = tagsResponse.Data;

            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(TaskCreateViewModel model)
        {
            var response = await _taskService.CreateAsync(this.GetUserId(), model);

            if (!response.IsSuccess)
            {
                var tagsResponse = await _tagService.GetByUserIdAsync(this.GetUserId());
                ViewBag.Tags = tagsResponse.IsSuccess ? tagsResponse.Data : new List<TagViewModel>();
                ModelState.AddModelError(string.Empty, response.Description ?? "Ошибка при создании задачи");
                return View(model);
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Редактировать задачу
        /// </summary>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit(int taskId)
        {
            var taskResponse = await _taskService.GetByIdAsync(this.GetUserId(), taskId);

            if (!taskResponse.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, taskResponse.Description ?? "Ошибка при получении задачи");
                return View(new TaskEditViewModel());
            }

            var tagsResponse = await _tagService.GetByUserIdAsync(this.GetUserId());
            ViewBag.Tags = tagsResponse.IsSuccess ? tagsResponse.Data : new List<TagViewModel>();

            var task = taskResponse.Data;
            return View(new TaskEditViewModel
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Priority = task.Priority,
                Status = task.Status,
                Deadline = task.Deadline,
                TagId = task.Tag?.Id ?? 0
            });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Edit(TaskEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var tagsResponse = await _tagService.GetByUserIdAsync(this.GetUserId());
                ViewBag.Tags = tagsResponse.IsSuccess ? tagsResponse.Data : new List<TagViewModel>();
                return View(model);
            }

            var response = await _taskService.UpdateAsync(this.GetUserId(), model);

            if (!response.IsSuccess)
            {
                var tagsResponse = await _tagService.GetByUserIdAsync(this.GetUserId());
                ViewBag.Tags = tagsResponse.IsSuccess ? tagsResponse.Data : new List<TagViewModel>();
                ModelState.AddModelError(string.Empty, response.Description ?? "Ошибка при обновлении задачи");
                return View(model);
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Удалить задачу
        /// </summary>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Delete(int taskId)
        {
            var response = await _taskService.DeleteAsync(this.GetUserId(), taskId);

            if (!response.IsSuccess)
            {
                TempData["Error"] = response.Description ?? "Ошибка при удалении задачи";
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Отметить задачу выполненной
        /// </summary>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Completed(int taskId)
        {
            var response = await _taskService.CompleteAsync(this.GetUserId(), taskId);

            if (!response.IsSuccess)
            {
                TempData["Error"] = response.Description ?? "Ошибка при выполнении задачи";
            }

            return RedirectToAction("Index");
        }
    }
}
