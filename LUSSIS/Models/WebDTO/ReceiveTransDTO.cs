using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace LUSSIS.Models.WebDTO
{
    public class ReceiveTransDTO
    {
        public ReceiveTransDTO()
        {
            ReceiveTransDetails = new List<ReceiveTransDetail>();
        }

        public int ReceiveId { get; set; }
        public int PoNum { get; set; }
        public DateTime ReceiveDate { get; set; }
        [Required]
        public string InvoiceNum { get; set; }
        [Required]
        public string DeliveryOrderNum { get; set; }
        public List<ReceiveTransDetail> ReceiveTransDetails { get; set; }
        public ReceiveTran ConvertToReceiveTran()
        {
            ReceiveTran receive = new ReceiveTran();
            receive.ReceiveId = this.ReceiveId;
            receive.PoNum = this.PoNum;
            receive.ReceiveDate = this.ReceiveDate;
            receive.InvoiceNum = this.InvoiceNum;
            receive.DeliveryOrderNum = this.DeliveryOrderNum;
            receive.ReceiveTransDetails = this.ReceiveTransDetails;
            return receive;
        }
    }
}