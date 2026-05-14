using System;
using System.Collections.Generic;
using System.Text;

namespace Deadliner.Domain.ViewModel.Tasks
{
    /// <summary>
    /// Список задач
    /// </summary>
    public class TaskListViewModel
    {
        /// <summary>
        /// Список задач
        /// </summary>
        public List<TaskDetailViewModel> Tasks { get; set; } = new();

        /// <summary>
        /// Общее количество задач (с учётом фильтров)
        /// </summary>
        public int TotalCount { get; set; }
    }
}