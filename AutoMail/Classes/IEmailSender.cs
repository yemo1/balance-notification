using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoMail.Classes
{
    public interface IEmailSender
    {
        void sendEmail(string from,string to,string message);
    }
}
