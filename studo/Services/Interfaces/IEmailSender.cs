using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace studo.Services.Interfaces
{
    public interface IEmailSender
    {
        Task SendEmailConfirmationAsync(string email, string subject, string message);
        Task SendResetPasswordEmail(string email, string resetPasswordToken);
    }
}
