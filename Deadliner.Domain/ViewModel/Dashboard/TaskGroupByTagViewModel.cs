using System;
using System.Collections.Generic;
using System.Text;

namespace Deadliner.Domain.ViewModel.Dashboard
{
    /// <summary>
    /// Группа задач, принадлежащих одному тегу (для дашборда).
    /// Теги без задач не возвращаются.
    /// Задачи без тега попадают в специальную группу с TagName = "Без тега".
    /// </summary>
    public class TaskGroupByTagViewModel
    {
        /// <summary>
        /// Id тега (null для группы "Без тега")
        /// </summary>
        public int? TagId { get; set; }

        /// <summary>
        /// Название тега
        /// </summary>
        public string TagName { get; set; } = null!;

        /// <summary>
        /// Цвет тега в HEX
        /// </summary>
        public string ColorHex { get; set; } = null!;

        /// <summary>
        /// Список ближайших задач в этом теге (количество ограничено limit)
        /// </summary>
        public List<TaskItemViewModel> Tasks { get; set; } = new();
    }
}
