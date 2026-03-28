using backend.Models;

namespace backend.Services
{
    public interface IEmailService
    {
        public Task SendEmailAsync(User user, string emailSubject, string emailBody);
    }
}
