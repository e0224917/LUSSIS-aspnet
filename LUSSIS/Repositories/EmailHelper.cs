using Glimpse.AspNet.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace LUSSIS.Repositories
{
    public class EmailHelper
    {
        public EmailHelper(string destinationEmail,string subject,string body)
        {
            SmtpClient client = new SmtpClient("smtp.gmail.com", 25);
            client.Credentials = new System.Net.NetworkCredential(@"sa45team7@gmail.com", "Password!123");
            client.EnableSsl = true;
            MailMessage mm = new MailMessage("sa45team7@gmail.com", "destinationEmail");
            mm.Subject = subject;
            mm.Body = body;
            client.Send(mm);
        }
    }
}