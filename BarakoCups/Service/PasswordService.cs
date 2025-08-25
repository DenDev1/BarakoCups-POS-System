using System.Net.Mail;
using System.Net;

namespace BarakoCups.Service
{
    public class PasswordService
    {
        private readonly string _smtpHost = "smtp.gmail.com";
        private readonly int _smtpPort = 587;
        private readonly string _smtpUser = "dendopulgo123@gmail.com";
        private readonly string _smtpPass = "didegpdzeqmvnztj";

        public async Task SendPasswordResetCodeAsync(string email, string resetCode)
        {
            var fromAddress = new MailAddress(_smtpUser, "dendopulgo123@gmail.com");
            var toAddress = new MailAddress(email);
            var subject = "Password Reset Code";
            var body = $"Your password reset code is: <strong>{resetCode}</strong>";

            var smtp = new SmtpClient
            {
                Host = _smtpHost,
                Port = _smtpPort,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_smtpUser, _smtpPass)
            };

            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
            {
                await smtp.SendMailAsync(message);
            }
        }
    }
}
