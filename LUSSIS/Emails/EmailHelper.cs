using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using LUSSIS.Models;

namespace LUSSIS.Emails
{
    //Authors: Guo Rui, Ton That Minh Nhat, Tang Xiaowen, Cui Run Ze
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

            destinationEmail = "sa45team7@gmail.com";
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

    //Author: Ton That Minh Nhat
    public class LUSSISEmail
    {
        public string Subject { get; }
        public string Body { get; }
        public string FromEmail { get; }
        public string ToEmail { get; }

        /// <summary>
        /// Builder class to make an email. 
        /// Should include three methods: From(), To() and For...(), 
        /// then Build() to create a LUSSISEmail object.
        /// </summary>
        public class Builder
        {
            public string Subject { get; private set; }
            public string Body { get; private set; }
            public string FromEmail { get; private set; }
            public string ToEmail { get; private set; }

            public Builder From(string fromEmail)
            {
                FromEmail = fromEmail;
                return this;
            }

            public Builder To(string toEmail)
            {
                ToEmail = toEmail;
                return this;
            }

            public Builder ForResetPassword(string fullName, string callbackUrl)
            {
                Subject = "Reset password for " + fullName;
                Body = "Please reset your password by clicking <a href=" + callbackUrl + ">here</a>";
                return this;
            }

            public Builder ForNewPo(PurchaseOrder purchaseOrder, string fullName)
            {
                Subject = "New Purchase Order";

                var body = new StringBuilder();
                body.AppendLine("New Purchase Order Created");
                body.AppendLine(
                    "This email is automatically generated and requires no reply to the sender.");
                body.AppendLine("Purchase Order No " + purchaseOrder.PoNum);
                body.AppendLine("Created By " + fullName);
                body.AppendLine("Created On " + purchaseOrder.CreateDate.ToString("dd-MM-yyyy"));

                Body = body.ToString();
                return this;
            }

            public Builder ForNonPrimaryNewPo(string supplierName, PurchaseOrder purchaseOrder,
                List<Stationery> stationerys)
            {
                Subject = "Purchasing from Secondary Supplier";
                //send email if using non=primary supplier
                var body = new StringBuilder();
                body.AppendLine("Secondary Suppliers in Purchase Order " + purchaseOrder.PoNum);
                body.AppendLine("This email is automatically generated and requires no reply to the sender.");
                body.AppendLine("Created for Supplier: " + supplierName);
                var index = 0;
                foreach (var stationery in stationerys)
                {
                    index++;
                    body.AppendLine("Index: " + index);
                    body.AppendLine("Stationery: " + stationery.Description);
                    body.AppendLine("Primary Supplier: " + stationery.PrimarySupplier().SupplierName);
                    body.AppendLine();
                }

                Body = body.ToString();
                return this;
            }

            public Builder ForNewStockAdjustments(string fullName, List<AdjVoucher> adjVouchers)
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

                body.AppendLine("by " + fullName + " on " + DateTime.Now.ToString("dd-MM-yyyy"));

                Body = body.ToString();
                return this;
            }

            public Builder ForNewStockAdjustment(string fullName, AdjVoucher adjVoucher)
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

            public Builder ForNewRepresentative()
            {
                Subject = "You have been assigned as the department representative.";

                var body = new StringBuilder();
                body.AppendLine("You have been assigned as the department representative.");
                body.AppendLine();
                body.AppendLine("This is effective from " + DateTime.Now.ToString("dd-MM-yyyy") + ".");

                Body = body.ToString();
                return this;
            }

            public Builder ForOldRepresentative()
            {
                Subject = "You are no longer the department representative.";

                var body = new StringBuilder();
                body.AppendLine("You are no longer the department representative.");
                body.AppendLine();
                body.AppendLine("This is effective from " + DateTime.Now.ToString("dd-MM-yyyy") + ".");

                Body = body.ToString();
                return this;
            }

            public Builder ForNewDelegate()
            {
                Subject = "You have been assigned as the delegate.";

                var body = new StringBuilder();
                body.AppendLine("You have been assigned as the delegate.");
                body.AppendLine();
                body.AppendLine("This is effective from " + DateTime.Now.ToString("dd-MM-yyyy") + ".");

                Body = body.ToString();
                return this;
            }

            public Builder ForOldDelegate()
            {
                Subject = "You are no longer the delegate.";

                var body = new StringBuilder();
                body.AppendLine("You are no longer the delegate.");
                body.AppendLine();
                body.AppendLine("This is effective from " + DateTime.Now.ToString("dd-MM-yyyy") + ".");

                Body = body.ToString();
                return this;
            }

            public Builder ForNewRequistion(string fullName, Requisition requisition)
            {
                Subject = "New requisition from " + fullName;

                var body = new StringBuilder();
                body.AppendLine("Description".PadRight(30, ' ') + "\t\t" + "UOM".PadRight(30, ' ') + "\t\t" +
                                "Quantity".PadRight(30, ' '));
                foreach (var detail in requisition.RequisitionDetails)
                {
                    var stationery = detail.Stationery;

                    body.AppendLine(stationery.Description.PadRight(30, ' ') + "\t\t" +
                                    stationery.UnitOfMeasure.PadRight(30, ' ') +
                                    "\t\t" + detail.Quantity.ToString().PadRight(30, ' '));
                }

                Body = body.ToString();
                return this;
            }

            public Builder ForRequisitionApproval(Requisition requisition)
            {
                Subject = "Requistion " + requisition.RequisitionId + " made on " +
                          requisition.RequisitionDate.ToString("dd-MM-yyyy") + " has been " + requisition.Status;

                var body = new StringBuilder();
                body.AppendLine("Your Requisition " + requisition.RequisitionId + " made on " +
                                requisition.RequisitionDate.ToString("dd-MM-yyyy") + " has been " + requisition.Status +
                                " by " +
                                requisition.ApprovalEmployee.FullName);
                body.AppendLine("Requested: ");
                foreach (var detail in requisition.RequisitionDetails)
                {
                    body.AppendLine("Item: " + detail.Stationery.Description);
                    body.AppendLine("Quantity: " + detail.Quantity);
                    body.AppendLine();
                }

                Body = body.ToString();
                return this;
            }

            public Builder ForNewDisbursement(Disbursement disbursement, CollectionPoint collectionPoint)
            {
                Subject = string.Format("Stationery Collection for " + disbursement.Department.DeptName + " on " +
                                        disbursement.CollectionDate.ToShortDateString() +
                                        " at " + collectionPoint.CollectionName);

                var body = new StringBuilder();
                body.AppendLine("We have an upcoming collection for " + disbursement.Department.DeptName);
                body.AppendLine();
                body.AppendLine("Date: \t\t\t" + disbursement.CollectionDate + " " + collectionPoint.Time);
                body.AppendLine("Location: \t" + collectionPoint.CollectionName);
                body.AppendLine(
                    "For more details, please log in LUSSIS to view: https://localhost:44303/Collection/Index");

                Body = body.ToString();
                return this;
            }

            public Builder ForUpdateDisbursement(Disbursement disbursement, CollectionPoint collectionPoint)
            {
                Subject = string.Format("Stationery Collection for " + disbursement.Department.DeptName + " on " +
                                        disbursement.CollectionDate.ToShortDateString() +
                                        " has been updated");

                var body = new StringBuilder();
                body.AppendLine("The upcoming collection for " + disbursement.Department.DeptName +
                                " has been updated as follow: ");
                body.AppendLine();
                body.AppendLine("Date: \t\t\t" + disbursement.CollectionDate + " " + collectionPoint.Time);
                body.AppendLine("Location: \t" + collectionPoint.CollectionName);
                body.AppendLine();
                body.AppendLine(
                    "For more details, please log in LUSSIS to view: https://localhost:44303/Collection/Index");

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
            if (string.IsNullOrEmpty(builder.FromEmail)) throw new MissingFieldException();
            FromEmail = builder.FromEmail;
            if (string.IsNullOrEmpty(builder.ToEmail)) throw new MissingFieldException();
            ToEmail = builder.ToEmail;
            if (string.IsNullOrEmpty(builder.Subject)) throw new MissingFieldException();
            Subject = builder.Subject;
            if (string.IsNullOrEmpty(builder.Body)) throw new MissingFieldException();
            Body = builder.Body;
        }
    }
}