using MailKit.Net.Smtp;
using MimeKit;
using System.Threading.Tasks;

namespace CV_AI.Services
{
    public class EmailService
    {
        private readonly string _emailFrom = "duygacon123456@gmail.com";
        private readonly string _emailPassword = "mmoc bcfa pokv uyjb"; // Dùng App Password của Gmail

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_emailFrom));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;
            email.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = body };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_emailFrom, _emailPassword);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}