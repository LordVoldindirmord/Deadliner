using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Deadliner.Domain.ViewModel.Tags
{
    /// <summary>
    /// Модель для создания нового тега
    /// </summary>
    public class TagCreateViewModel
    {
        /// <summary>
        /// Название тега
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Цвет в HEX (например, #FF5733). По умолчанию серый.
        /// </summary>
        [StringLength(7)]
        public string ColorHex { get; set; } = "#6C757D";
    }
}
