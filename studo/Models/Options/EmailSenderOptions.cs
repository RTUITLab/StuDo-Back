using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace studo.Models.Options
{
    public class EmailSenderOptions
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string InvitationTemplateUrl { get; set; }
        public string ResetPasswordTemplateUrl { get; set; }
        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }

    }
}
