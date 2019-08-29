using Microsoft.Extensions.Options;
using studo.Models.Options;
using studo.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public EmailSender(IOptions<EmailSenderOptions> options)
        {
            this.options = options.Value;
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
            => new HttpClient().GetStringAsync(options.ConfirmEmailTemplateUrl);

        private Task<string> GetResetPasswordTemplateAsync()
            => new HttpClient().GetStringAsync(options.ResetPasswordTemplateUrl);
    }
}
