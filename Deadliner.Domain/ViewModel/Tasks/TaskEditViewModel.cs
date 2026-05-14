using Deadliner.Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Deadliner.Domain.ViewModel.Tasks
{
    /// <summary>
    /// Модель для редактирования существующей задачи
    /// </summary>
    public class TaskEditViewModel
    {
        /// <summary>
        /// Id задачи
        /// </summary>
        [Required]
        public int Id { get; set; }

        /// <summary>
        /// Заголовок
        /// </summary>
        [Required]
        [StringLength(500)]
        public string Title { get; set; } = null!;

        /// <summary>
        /// Описание
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Приоритет
        /// </summary>
        [Required]
        public PriorityStatus Priority { get; set; }

        /// <summary>
        /// Дедлайн
        /// </summary>
        public DateTime? Deadline { get; set; }

        /// <summary>
        /// Статус (можно изменить)
        /// </summary>
        [Required]
        public TasksStatus Status { get; set; }

        /// <summary>
        /// Id тега
        /// </summary>
        public int TagId { get; set; }
    }
}
