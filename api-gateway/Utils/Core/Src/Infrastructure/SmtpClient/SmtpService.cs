using System.Net;
using System.Net.Mail;

namespace Application.Core
{
    public record EmailInfo(string To, string Subject, string Body);

    public class SmtpService : IEmailService<EmailInfo>
    {
        public Task SendEmail(EmailInfo emailInfo)
        {
            var from = Environment.GetEnvironmentVariable("EMAIL_FROM")!;
            var password = Environment.GetEnvironmentVariable("EMAIL_PASSWORD")!;
            var smtpHost = Environment.GetEnvironmentVariable("SMTP_HOST")!;

            var smtpClient = new SmtpClient(smtpHost)
            {
                Port = 587,
                Credentials = new NetworkCredential(from, password),
                EnableSsl = true,
            };
            return smtpClient.SendMailAsync(from, emailInfo.To, emailInfo.Subject, emailInfo.Body);
        }
    }
}
