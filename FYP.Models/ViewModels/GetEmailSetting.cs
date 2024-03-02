using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fyp.Models.ViewModels
{
    public class GetEmailSetting
    {
        public string SecretKey { get; set; } =default!;    
        public string From { get; set; }
        public string SmtpServer { get; set; }
        public int Port { get; set; }
        public bool EnableSSL { get; set; }
    }
}
