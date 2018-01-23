using Glimpse.AspNet.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace LUSSIS.Emails
{
    public static class EmailHelper
    {
        public static string DestinationEmail { get; set; }
        public static string Subject { get; set; }
        public static string Body { get; set; }
        
        public static void SendEmail(string destinationEmail, string subject, string body)
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