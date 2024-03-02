/*using Fyp.Models.ViewModels;
using FypWeb.Areas.Admin.Repository.Interface;
using System.Net.Mail;

namespace FypWeb.Areas.Admin.Repository.Service
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration configuration;
        public EmailSender(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public async Task<bool> EmailSendAsync(string email, string subject, string message)
        {
            bool status = false;
            try
            {
                GetEmailSetting getEmailSetting = new GetEmailSetting()
                {
                    SecretKey = configuration.GetValue<string>("AppSetting:SecretKey"),
                    From = configuration.GetValue<string>("AppSetting:EmailSettings:From"),
                    SmtpServer = configuration.GetValue<string>("AppSetting:EmailSettings:SmtpServer"),
                    Port = configuration.GetValue<int>("AppSetting:EmailSettings:Port"),
                    EnableSSL = configuration.GetValue<bool>("AppSetting:EmailSettings:EnableSSL"),
                };
                MailMessage mailMessage = new MailMessage()
                {
                   From=new MailAddress(getEmailSetting.From),
                   Subject=subject,
                   Body=message,
                  
                };
                mailMessage.To.Add(email);
                SmtpClient smtpClient = new SmtpClient(getEmailSetting.SmtpServer)
                {
                    Port = getEmailSetting.Port,
                    Credentials = new System.Net.NetworkCredential(getEmailSetting.From, getEmailSetting.SecretKey),
                    EnableSsl = getEmailSetting.EnableSSL
                };
                await smtpClient.SendMailAsync(mailMessage);
                status = true;


            }
            catch (Exception ex)
            {
                status = false;
            }
            return status;

        }
    }
}
*/