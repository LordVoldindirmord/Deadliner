using System;
using System.Collections.Generic;
using System.Text;

namespace Deadliner.Domain.ViewModel.Tags
{
    /// <summary>
    /// Тег для отображения в списках и дашборде
    /// </summary>
    public class TagViewModel
    {
        /// <summary>
        /// Id тега
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Название
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Цвет в HEX
        /// </summary>
        public string ColorHex { get; set; } = null!;

        /// <summary>
        /// Количество активных задач в теге
        /// </summary>
        public int ActiveTaskCount { get; set; }
    }
}
