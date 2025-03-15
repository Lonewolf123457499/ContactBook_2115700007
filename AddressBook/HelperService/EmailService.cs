using System.Net.Mail;
using System.Net;

namespace AddressBook.HelperService
{
    public class EmailService
    {
            private readonly IConfiguration _config;

            public EmailService(IConfiguration config)
            {
                _config = config;
            }

            public async Task SendEmailAsync(string to, string subject, string body)
            {
                using var smtpClient = new SmtpClient(_config["SmtpSettings:Host"])
                {
                    Port = int.Parse(_config["SmtpSettings:Port"]),
                    Credentials = new NetworkCredential(_config["SmtpSettings:UserName"], _config["SmtpSettings:Password"]),
                    EnableSsl = bool.Parse(_config["SmtpSettings:EnableSSL"])
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_config["SmtpSettings:From"]),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(to);
                await smtpClient.SendMailAsync(mailMessage);
            }
        }
    
}
