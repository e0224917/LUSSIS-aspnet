using Glimpse.AspNet.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace LUSSIS.Emails
{
    public class EmailHelper
    {
        public string destinationEmail { get; set; }
        public string subject { get; set; }
        public string body { get; set; }

        public EmailHelper()
        {
        }

        public EmailHelper(string destinationEmail, string subject, string body)
        {
            this.destinationEmail = destinationEmail;
            this.subject = subject;
            this.body = body;
        }

        public void SendEmail()
        {
            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
            client.Credentials = new System.Net.NetworkCredential(@"sa45team7@gmail.com", "Password!123");
            client.EnableSsl = true;
            MailMessage mm = new MailMessage("sa45team7@gmail.com", destinationEmail);
            mm.Subject = subject;
            mm.Body = body;
            client.Send(mm);
        }
    }
}