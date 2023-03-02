using System.Threading.Tasks;

namespace app.Infrastructure;

public interface IEmailSender
{
    Task SendEmailAsync(string to, string subject, string body);
}