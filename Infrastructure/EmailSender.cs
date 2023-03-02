using Microsoft.Extensions.Logging;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Net.Mail;
using System.Net;

namespace app.Infrastructure;

public class EmailSender : IEmailSender
{

    private readonly IConfiguration _configuration;

    private readonly ILogger _logger;

    public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }
    
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        _logger.LogInformation("Host: " + _configuration["Mail:Host"]);
        _logger.LogInformation("Port: " + _configuration["Mail:Port"]);
        _logger.LogInformation("Username: " + _configuration["Mail:Username"]);
        _logger.LogInformation("Password: " + _configuration["Mail:Password"]);
        _logger.LogInformation("FromAddress: " + _configuration["Mail:FromAddress"]);

        var emailClient = new SmtpClient(_configuration["Mail:Host"], int.Parse(_configuration["Mail:Port"]));
        // emailClient.Credentials = new NetworkCredential(_configuration["Mail:Username"], _configuration["Mail:Password"]);
        emailClient.EnableSsl = true;
        var message = new MailMessage
        {
            From = new MailAddress(_configuration["Mail:FromAddress"]),
            Subject = subject,
            Body = body
        };
        message.To.Add(new MailAddress(to));
        await emailClient.SendMailAsync(message);
    }
}