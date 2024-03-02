namespace FypWeb.Areas.Admin.Repository.Interface
{
    public interface IEmailSender
    {
        Task <bool> SendEmailAsync(string email, string subject, string message);
    }
}
