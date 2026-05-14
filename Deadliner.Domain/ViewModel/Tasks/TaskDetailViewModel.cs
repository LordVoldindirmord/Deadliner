using Deadliner.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deadliner.Domain.ViewModel.Tasks
{
    /// <summary>
    /// Детальное представление задачи (для списка и для отдельной страницы)
    /// </summary>
    public class TaskDetailViewModel
    {
        /// <summary>
        /// Id задачи
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Заголовок
        /// </summary>
        public string Title { get; set; } = null!;

        /// <summary>
        /// Описание
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Приоритет
        /// </summary>
        public PriorityStatus Priority { get; set; }

        /// <summary>
        /// Статус
        /// </summary>
        public TasksStatus Status { get; set; }

        /// <summary>
        /// Дедлайн
        /// </summary>
        public DateTime? Deadline { get; set; }

        /// <summary>
        /// Дата выполнения
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Просрочена ли
        /// </summary>
        public bool IsOverdue { get; set; }

        /// <summary>
        /// Прикреплённый тег
        /// </summary>
        public TagInfoViewModel Tag { get; set; } = null!;
    }

    /// <summary>
    /// Краткая информация о теге для шапки списка задач
    /// </summary>
    public class TagInfoViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string ColorHex { get; set; } = null!;
    }
}