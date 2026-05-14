using Deadliner.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Deadliner.Domain.ViewModel.Tasks
{
    /// <summary>
    /// Модель фильтрации и сортировки — присылается с фронта при запросе списка задач.
    /// Все поля опциональны.
    /// </summary>
    public class TaskFilterViewModel
    {
        /// <summary>
        /// Фильтр по тегу (null — все теги).
        /// </summary>
        public int? TagId { get; set; }

        /// <summary>
        /// Фильтр по приоритету (null — все).
        /// </summary>
        public PriorityStatus? Priority { get; set; }

        /// <summary>
        /// Фильтр по статусу (null — все).
        /// </summary>
        public TasksStatus? Status { get; set; }

        /// <summary>
        /// Поиск по названию (содержит подстроку).
        /// </summary>
        public string? SearchQuery { get; set; }

        /// <summary>
        /// Показать только просроченные.
        /// </summary>
        public bool? OverdueOnly { get; set; }

        /// <summary>
        /// Поле для сортировки: "deadline", "priority", "created_at". Если указан некорректный, то будет created_at.
        /// </summary>
        public string? SortBy { get; set; }

        /// <summary>
        /// Направление сортировки: "asc" или "desc". По умолчанию — "asc".
        /// </summary>
        public string? SortDirection { get; set; }
    }
}
