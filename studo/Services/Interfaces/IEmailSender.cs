using System.Threading.Tasks;

namespace studo.Services.Interfaces
{
    public interface IEmailSender
    {
        Task SendEmailConfirmationAsync(string email, string redirectUrl);
        Task SendResetPasswordEmail(string email, string redirectUrl);
    }
}
