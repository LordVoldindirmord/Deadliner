using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Deadliner.Domain.ViewModel.Tags
{
    /// <summary>
    /// Модель для редактирования тега
    /// </summary>
    public class TagEditViewModel
    {
        /// <summary>
        /// Id тега
        /// </summary>
        [Required]
        public int Id { get; set; }

        /// <summary>
        /// Новое название
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Новый цвет
        /// </summary>
        [StringLength(7)]
        public string ColorHex { get; set; } = null!;
    }
}