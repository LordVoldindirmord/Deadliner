using Deadliner.ASP.Extensions;
using Deadliner.Domain.ViewModel.Dashboard;
using Deadliner.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Deadliner.ASP.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ITaskService _taskService;

        public DashboardController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Dashboard(int limit = 10)
        {
            var response = await _taskService.GetDashboardAsync(this.GetUserId(), limit);

            if (!response.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, response.Description ?? "Ошибка при получении дашборда");
                return View(new DashboardViewModel());
            }

            if (response.StatusCode == Domain.Enum.StatusCode.NoContent || response.Data == null)
            {
                return View(new DashboardViewModel());
            }

            return View(response.Data);
        }
    }
}
