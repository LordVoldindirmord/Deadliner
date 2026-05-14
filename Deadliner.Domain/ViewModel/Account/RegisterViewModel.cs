using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Deadliner.Domain.ViewModel.Account
{
    /// <summary>
    /// Модель для регистрации нового пользователя
    /// </summary>
    public class RegisterViewModel
    {
        /// <summary>
        /// Логин (будет отображаться в интерфейсе)
        /// </summary>
        [Required(ErrorMessage = "Login обязателен для заполнения")]
        [StringLength(100, MinimumLength = 3)]
        [Display(Name = "Login")]
        public string Login { get; set; } = null!;

        /// <summary>
        /// Email
        /// </summary>
        [Required(ErrorMessage = "Email обязателен для заполнения")]
        [EmailAddress(ErrorMessage = "Введите корректный Email адрес")]
        [StringLength(255)]
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;

        /// <summary>
        /// Пароль
        /// </summary>
        [Required(ErrorMessage = "Пароль обязателен для заполнения")]
        [MinLength(6, ErrorMessage = "Пароль должен содержать минимум 6 символов")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = null!;

        /// <summary>
        /// Подтверждение пароля
        /// </summary>
        [Required(ErrorMessage = "Подтверждение пароля обязательно")]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение пароля")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
