using System;
using System.Collections.Generic;
using System.Text;

namespace Deadliner.Domain.ViewModel.Dashboard
{
    /// <summary>
    /// Общий дашборд — возвращается одним запросом
    /// </summary>
    public class DashboardViewModel
    {
        /// <summary>
        /// Количество активных задач (status = 'active', deadline не просрочен)
        /// </summary>
        public int ActiveCount { get; set; }

        /// <summary>
        /// Количество просроченных задач (status = 'active' и deadline < now())
        /// </summary>
        public int OverdueCount { get; set; }

        /// <summary>
        /// Количество выполненных задач (status = 'completed')
        /// </summary>
        public int CompletedCount { get; set; }

        /// <summary>
        /// Количество отмененных задач (status = 'cancelled')
        /// </summary>
        public int CancelledCount { get; set; }

        /// <summary>
        /// Ближайшие задачи, сгруппированные по тегам.
        /// Количество задач внутри каждого тега контролируется параметром limit.
        /// </summary>
        public List<TaskGroupByTagViewModel> TaskGroups { get; set; } = new();
    }
}
