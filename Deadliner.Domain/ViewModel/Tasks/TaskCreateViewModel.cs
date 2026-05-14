using Deadliner.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Deadliner.Domain.ViewModel.Tasks
{
    /// <summary>
    /// Модель для создания новой задачи
    /// </summary>
    public class TaskCreateViewModel
    {
        /// <summary>
        /// Заголовок (обязательно)
        /// </summary>
        [Required]
        [StringLength(500)]
        public string Title { get; set; } = null!;

        /// <summary>
        /// Описание (опционально)
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Приоритет
        /// </summary>
        [Required]
        public PriorityStatus Priority { get; set; } = PriorityStatus.Medium;

        /// <summary>
        /// Дедлайн (опционально)
        /// </summary>
        public DateTime? Deadline { get; set; }

        /// <summary>
        /// Id тега, к которому прикрепить задачу (null — без тега)
        /// </summary>
        public int TagId { get; set; }
    }
}
