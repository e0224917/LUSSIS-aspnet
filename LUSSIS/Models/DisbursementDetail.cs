namespace LUSSIS.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("DisbursementDetail")]
    public partial class DisbursementDetail
    {
        public DisbursementDetail()
        {

        }

        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int DisbursementId { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(20)]
        public string ItemNum { get; set; }

        [Required]
        public double UnitPrice { get; set; }

        [Required]
        [Display(Name = "Requested Qty")]
        public int RequestedQty { get; set; }

        [Required]
        [Display(Name = "Actual Qty")]
        public int ActualQty { get; set; }

        public virtual Disbursement Disbursement { get; set; }

        public virtual Stationery Stationery { get; set; }

        public DisbursementDetail(RequisitionDetail requisitionDetail)
        {
            ItemNum = requisitionDetail.ItemNum;
            RequestedQty = requisitionDetail.Quantity;
            UnitPrice = requisitionDetail.Stationery.AverageCost;
            //if not enough stock, set actual qty to stock number, else set as requested qty
            ActualQty = requisitionDetail.Stationery.AvailableQty > RequestedQty 
                ? RequestedQty
                : requisitionDetail.Stationery.AvailableQty;
            Stationery = requisitionDetail.Stationery;
        }

        public DisbursementDetail(DisbursementDetail unfufilledDisbursementDetail)
        {
            ItemNum = unfufilledDisbursementDetail.ItemNum;
            //new requested qty equals to the difference between last requested qty and actual qty
            RequestedQty = unfufilledDisbursementDetail.RequestedQty - unfufilledDisbursementDetail.ActualQty;
            UnitPrice = unfufilledDisbursementDetail.UnitPrice;
            ActualQty = RequestedQty;
        }
    }
}
