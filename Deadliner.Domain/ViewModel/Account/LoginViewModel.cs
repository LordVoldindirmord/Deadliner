using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Deadliner.Domain.ViewModel.Account
{
    /// <summary>
    /// Модель для входа пользователя
    /// </summary>
    public class LoginViewModel
    {
        /// <summary>
        /// Логин или Email (пользователь может ввести любое)
        /// </summary>
        [Required(ErrorMessage = "Login или Email обязателен для заполнения")]
        [EmailAddress(ErrorMessage = "Введите корректный Email адрес")]
        [Display(Name = "Email или Login")]
        public string LoginOrEmail { get; set; } = null!;

        /// <summary>
        /// Пароль
        /// </summary>
        [Required(ErrorMessage = "Пароль обязателен для заполнения")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = null!;

        /// <summary>
        /// Запомнить пользователя (для долгоживущей сессии)
        /// </summary>
        [Display(Name = "Запомнить меня")]
        public bool RememberMe { get; set; }
    }
}
