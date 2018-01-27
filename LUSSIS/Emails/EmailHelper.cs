using Glimpse.AspNet.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using LUSSIS.Models;

namespace LUSSIS.Emails
{
    public static class EmailHelper
    {
        public static string DestinationEmail { get; set; }
        public static string Subject { get; set; }
        public static string Body { get; set; }

        public static void SendEmail(string destinationEmail, string subject, string body)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new System.Net.NetworkCredential(@"sa45team7@gmail.com", "Password!123"),
                EnableSsl = true
            };

            var mm = new MailMessage("sa45team7@gmail.com", destinationEmail)
            {
                Subject = subject,
                Body = body
            };

            client.Send(mm);
        }

        public static void SendEmail(string subject, string body)
        {
            SendEmail("sa45team7@gmail.com", subject, body);
        }

        public static void SendEmail(LUSSISEmail email)
        {
            SendEmail(email.FromEmail, email.Subject, email.Body);
        }
    }

    public class LUSSISEmail
    {
        public string Subject { get; }
        public string Body { get; }
        public string FromEmail { get; }
        public string ToEmail { get; }

        public class Builder
        {
            public string Subject { get; private set; }
            public string Body { get; private set; }
            public string FromEmail { get; private set; }
            public string ToEmail { get; private set; }
            private Employee FromEmployee { get; set; }

            public Builder()
            {
            }

            public Builder From(string fromEmail)
            {
                FromEmail = fromEmail;
                return this;
            }

            public Builder From(Employee employee)
            {
                FromEmployee = employee;
                FromEmail = employee.EmailAddress;
                return this;
            }

            public Builder To(Employee employee)
            {
                ToEmail = employee.EmailAddress;
                return this;
            }

            public Builder To(string toEmail)
            {
                ToEmail = toEmail;
                return this;
            }

            public Builder ForNewPo(PurchaseOrder purchaseOrder, string fullName)
            {
                Subject = "New Purchase Order";

                var emailForSupervisor = new StringBuilder("New Purchase Order Created");
                emailForSupervisor.AppendLine(
                    "This email is automatically generated and requires no reply to the sender.");
                emailForSupervisor.AppendLine("Purchase Order No " + purchaseOrder.PoNum);
                emailForSupervisor.AppendLine("Created By " + fullName);
                emailForSupervisor.AppendLine("Created On " + purchaseOrder.CreateDate.ToString("dd-MM-yyyy"));

                Body = emailForSupervisor.ToString();
                return this;
            }

            public Builder ForNonPrimaryNewPo(string supplierName, PurchaseOrder purchaseOrder,
                List<Stationery> stationerys)
            {
                Subject = "Purchasing from Non-Primary Supplier";
                //send email if using non=primary supplier
                var emailBody =
                    new StringBuilder("Non-Primary Suppliers in Purchase Order " + purchaseOrder.PoNum);
                emailBody.AppendLine(
                    "This email is automatically generated and requires no reply to the sender.");
                emailBody.AppendLine("Created for Supplier: " + supplierName);
                var index = 0;
                foreach (var stationery in stationerys)
                {
                    index++;
                    emailBody.AppendLine("Index: " + index);
                    emailBody.AppendLine("Stationery: " + stationery.Description);
                    emailBody.AppendLine("Primary Supplier: " + stationery.PrimarySupplier().SupplierName);
                    emailBody.AppendLine();
                }

                Body = emailBody.ToString();
                return this;
            }

            public Builder ForStockAdjustments(string fullName, List<AdjVoucher> adjVouchers)
            {
                Subject = "A new adjustment of stationeries has been made by " + fullName;

                var body = new StringBuilder();
                body.AppendLine(fullName + " has made the following adjustment: ");
                foreach (var voucher in adjVouchers)
                {
                    body.AppendLine("Stationery: " + voucher.Stationery.Description);
                    body.AppendLine("Quantity: " + voucher.Quantity);
                    body.AppendLine();
                }

                body.AppendLine("by " + fullName + "on" + DateTime.Now.ToString("dd-MM-yyyy"));

                Body = body.ToString();
                return this;
            }

            public Builder ForStockAdjustment(string fullName, AdjVoucher adjVoucher)
            {
                Subject = "A new adjustment of stationeries has been made by " + fullName;

                var body = new StringBuilder();
                body.AppendLine(fullName + " has made the following adjustment: ");
                body.AppendLine("Stationery: " + adjVoucher.Stationery.Description);
                body.AppendLine("Quantity: " + adjVoucher.Quantity);
                body.AppendLine();
                body.AppendLine("by " + fullName + "on" + DateTime.Now.ToString("dd-MM-yyyy"));

                Body = body.ToString();
                return this;
            }

            public LUSSISEmail Build()
            {
                return new LUSSISEmail(this);
            }
        }

        private LUSSISEmail(Builder builder)
        {
            FromEmail = builder.FromEmail;
            ToEmail = builder.ToEmail;
            Subject = builder.Subject;
            Body = builder.Body;
        }
    }

    public enum EmailIntention
    {
        NewRequisition,
        ApproveRequisition,
        NewPo
    }
}