using Microsoft.Extensions.Options;
using studo.Models.Options;
using studo.Services.Interfaces;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace studo.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailSenderOptions options;
        private readonly HttpClient httpClient;

        public EmailSender(IOptions<EmailSenderOptions> options, IHttpClientFactory httpClientFactory)
        {
            this.options = options.Value;
            this.httpClient = httpClientFactory.CreateClient();
        }

        public async Task SendEmailConfirmationAsync(string email, string redirectUrl)
        {
            var message = (await GetConfirmEmailTemplateAsync())
                .Replace("%url%", $"{HtmlEncoder.Default.Encode(redirectUrl)}");
            await SendEmailAsync(email, "Подтвердите почту", message);
        }

        public async Task SendResetPasswordEmail(string email, string redirectUrl)
        {
            var message = (await GetResetPasswordTemplateAsync())
                .Replace("%url%", $"{ HtmlEncoder.Default.Encode(redirectUrl)}");
            await SendEmailAsync(email, "Восстановление пароля", message);
        }

        private async Task SendEmailAsync(string email, string subject, string message)
        {
            MailMessage mailMessage = new MailMessage
            {
                From = new MailAddress(options.Email),
                IsBodyHtml = true
            };
            mailMessage.To.Add(new MailAddress(email));
            mailMessage.Subject = subject;
            mailMessage.Body = message;

            SmtpClient smtpClient = new SmtpClient
            {
                Host = options.SmtpHost,
                Port = options.SmtpPort,
                EnableSsl = true,
                Credentials = new NetworkCredential(options.Email, options.Password)
            };

            await smtpClient.SendMailAsync(mailMessage);
        }

        private Task<string> GetConfirmEmailTemplateAsync()
            => httpClient.GetStringAsync(options.ConfirmEmailTemplateUrl);

        private Task<string> GetResetPasswordTemplateAsync()
            => httpClient.GetStringAsync(options.ResetPasswordTemplateUrl);
    }
}
