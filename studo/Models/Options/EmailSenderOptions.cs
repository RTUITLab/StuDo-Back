namespace studo.Models.Options
{
    public class EmailSenderOptions
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmEmailTemplateUrl { get; set; }
        public string ResetPasswordTemplateUrl { get; set; }
        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
    }
}
