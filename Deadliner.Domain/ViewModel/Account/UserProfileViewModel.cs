using System;
using System.Collections.Generic;
using System.Text;

namespace Deadliner.Domain.ViewModel.Account
{
    /// <summary>
    /// Профиль пользователя (отображение в шапке, настройках)
    /// </summary>
    public class UserProfileViewModel
    {
        /// <summary>
        /// Id пользователя
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Логин (также он nickname)
        /// </summary>
        public string Login { get; set; } = null!;

        /// <summary>
        /// Email
        /// </summary>
        public string Email { get; set; } = null!;

        /// <summary>
        /// Дата регистрации
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
