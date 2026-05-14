using System;
using System.Collections.Generic;
using System.Text;

namespace Deadliner.Domain.Enum
{
    /// <summary>
    /// Enum статуса выполнения задачи
    /// </summary>
    public enum TasksStatus
    {
        /// <summary>
        /// Задача выполняется
        /// </summary>
        Active = 1,

        /// <summary>
        /// Задача выполнена
        /// </summary>
        Completed = 2,

        /// <summary>
        /// Задача отменена
        /// </summary>
        Cancelled = 3
    }
}
