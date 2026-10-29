using CinemaGo.Application.Services.Interfaces;
using CinemaGo.Application.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace CinemaGo.Application.Services.Implementations
{
    public class SmtpEmailService : IEmailService
    {
        private readonly SmtpSettings _smtp;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IOptions<EmailSettings> settings, ILogger<SmtpEmailService> logger)
        {
            _smtp = settings.Value.Smtp;
            _logger = logger;
        }

        public async Task SendAsync(string to, string subject, string body, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(_smtp.Host) ||
                string.IsNullOrWhiteSpace(_smtp.UserName) || _smtp.UserName.StartsWith("[ENTER") ||
                string.IsNullOrWhiteSpace(_smtp.Password) || _smtp.Password.StartsWith("[ENTER"))
                throw new InvalidOperationException(
                    "SMTP is not configured. Fill Email:Smtp in appsettings.Local.json or use Email:Provider = Simulated.");

            var fromAddress = string.IsNullOrWhiteSpace(_smtp.FromAddress) ? _smtp.UserName : _smtp.FromAddress;
        
            using var mailMessage = new MailMessage
            {
                From = new MailAddress(fromAddress, _smtp.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(to);

            using var client = new SmtpClient(_smtp.Host, _smtp.Port)
            {
                Credentials = new NetworkCredential(_smtp.UserName, _smtp.Password),
                EnableSsl = true 
            };

            using (ct.Register(() => client.SendAsyncCancel()))
            {
                await client.SendMailAsync(mailMessage);
            }
            _logger.LogInformation("E-mail '{Subject}' sent to {To}.", subject, to);
        }
    }
}
