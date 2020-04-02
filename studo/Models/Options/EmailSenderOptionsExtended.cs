using RTUITLab.EmailService.Client;

namespace studo.Models.Options
{
    public class EmailSenderOptionsExtended : EmailSenderOptions
    {
        public string ConfirmEmailTemplateUrl { get; set; }
        public string ResetPasswordTemplateUrl { get; set; }
    }
}
