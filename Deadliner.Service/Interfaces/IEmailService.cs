using System;
using System.Collections.Generic;
using System.Text;

namespace Deadliner.Service.Interfaces
{
    public interface IEmailService
    {
        /// <summary>
        /// Отправить email-письмо
        /// </summary>
        /// <param name="to">Кому</param>
        /// <param name="subject">Тема письма</param>
        /// <param name="body">HTML-содержимое письма</param>
        Task SendEmailAsync(string to, string subject, string body);
    }
}
