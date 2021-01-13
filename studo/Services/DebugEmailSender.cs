using Microsoft.Extensions.Logging;
using RTUITLab.EmailService.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace studo.Services
{
    public class DebugEmailSender : IEmailSender
    {
        private readonly ILogger<DebugEmailSender> logger;

        public DebugEmailSender(ILogger<DebugEmailSender> logger)
        {
            this.logger = logger;
        }
        public Task SendEmailAsync(string email, string subject, string body)
        {
            logger.LogInformation($@"Sending email: 
email - {email}
subject - {subject}
body - {body}");
            return Task.CompletedTask;
        }
    }
}
