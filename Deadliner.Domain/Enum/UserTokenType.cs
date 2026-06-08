using System;
using System.Collections.Generic;
using System.Text;

namespace Deadliner.Domain.Enum
{
    public enum UserTokenType
    {
        /// <summary>
        /// Для подтверждения почты
        /// </summary>
        EmailConfirm,

        /// <summary>
        /// Для смены пароля
        /// </summary>
        PasswordReset,
    }
}
