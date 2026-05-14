using Deadliner.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deadliner.Domain.ViewModel.Dashboard
{
    /// <summary>
    /// Краткая карточка задачи для отображения в дашборде
    /// </summary>
    public class TaskItemViewModel
    {
        /// <summary>
        /// Id задачи
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Заголовок задачи
        /// </summary>
        public string Title { get; set; } = null!;

        /// <summary>
        /// Приоритет
        /// </summary>
        public PriorityStatus Priority { get; set; }

        /// <summary>
        /// Статус
        /// </summary>
        public TasksStatus Status { get; set; }

        /// <summary>
        /// Дедлайн (null — без срока)
        /// </summary>
        public DateTime? Deadline { get; set; }

        /// <summary>
        /// Просрочена ли задача
        /// </summary>
        public bool IsOverdue { get; set; }
    }
}
