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

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> DownloadPdf(DateTime fromDate, DateTime toDate)
        {
            var response = await _taskService.GeneratePdfAsync(this.GetUserId(), fromDate, toDate);

            if (!response.IsSuccess || response.Data == null)
            {
                TempData["Error"] = response.Description ?? "Ошибка при создании PDF";
                return RedirectToAction("Dashboard");
            }

            return File(response.Data, "application/pdf",
                $"Deadliner_Planner_{fromDate:yyyy-MM-dd}_{toDate:yyyy-MM-dd}.pdf");
        }
    }
}
