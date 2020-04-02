using Microsoft.Extensions.Options;
using studo.Models.Options;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace studo.Services
{
    public class EmailSender : Interfaces.IEmailSender
    {
        private readonly EmailSenderOptionsExtended options;
        private readonly HttpClient httpClient;
        private readonly RTUITLab.EmailService.Client.IEmailSender sender;

        public EmailSender(IOptions<EmailSenderOptionsExtended> options, IHttpClientFactory httpClientFactory, RTUITLab.EmailService.Client.IEmailSender sender)
        {
            this.options = options.Value;
            this.httpClient = httpClientFactory.CreateClient();
            this.sender = sender;
        }

        public async Task SendEmailConfirmationAsync(string email, string redirectUrl)
        {
            var message = (await GetConfirmEmailTemplateAsync())
                .Replace("%url%", $"{HtmlEncoder.Default.Encode(redirectUrl)}");
            await sender.SendEmailAsync(email, "Подтвердите почту", message);
        }

        public async Task SendResetPasswordEmail(string email, string redirectUrl)
        {
            var message = (await GetResetPasswordTemplateAsync())
                .Replace("%url%", $"{HtmlEncoder.Default.Encode(redirectUrl)}");
            await sender.SendEmailAsync(email, "Восстановление пароля", message);
        }

        private Task<string> GetConfirmEmailTemplateAsync()
            => httpClient.GetStringAsync(options.ConfirmEmailTemplateUrl);

        private Task<string> GetResetPasswordTemplateAsync()
            => httpClient.GetStringAsync(options.ResetPasswordTemplateUrl);
    }
}
