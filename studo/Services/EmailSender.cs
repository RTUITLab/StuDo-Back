using Microsoft.Extensions.Options;
using studo.Models.Options;
using studo.Services.Interfaces;
using System.Net.Http;
using MailKit.Net.Smtp;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using MimeKit;

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
                .Replace("%url%", $"{HtmlEncoder.Default.Encode(redirectUrl)}");
            await SendEmailAsync(email, "Восстановление пароля", message);
        }

        private async Task SendEmailAsync(string email, string subject, string message)
        {
            MimeMessage mailMessage = new MimeMessage();
            mailMessage.From.Add(new MailboxAddress(options.Email));
            mailMessage.To.Add(new MailboxAddress(email));
            mailMessage.Subject = subject;
            mailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = message
            };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(options.SmtpHost, options.SmtpPort, true);
                await client.AuthenticateAsync(options.Email, options.Password);
                await client.SendAsync(mailMessage);

                await client.DisconnectAsync(true);
            }
        }

        private Task<string> GetConfirmEmailTemplateAsync()
            => httpClient.GetStringAsync(options.ConfirmEmailTemplateUrl);

        private Task<string> GetResetPasswordTemplateAsync()
            => httpClient.GetStringAsync(options.ResetPasswordTemplateUrl);
    }
}
