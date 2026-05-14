using System;
using System.Collections.Generic;
using System.Text;

namespace Deadliner.Domain.Enum
{
    /// <summary>
    /// Enum приоритета статуса задачи
    /// </summary>
    public enum PriorityStatus
    {
        /// <summary>
        /// Задача не очень важная
        /// </summary>
        Low = 1,

        /// <summary>
        /// Задача средняя по важности
        /// </summary>
        Medium = 2,

        /// <summary>
        /// Задача важная
        /// </summary>
        High = 3,

        /// <summary>
        /// Задача неотложная
        /// </summary>
        Critical = 4
    }
}