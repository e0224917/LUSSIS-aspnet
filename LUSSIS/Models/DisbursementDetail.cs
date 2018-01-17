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
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int DisbursementId { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(20)]
        public string ItemNum { get; set; }

        public double? UnitPrice { get; set; }

        public int? RequestedQty { get; set; }

        public int? ActualQty { get; set; }

        public virtual Disbursement Disbursement { get; set; }

        public virtual Stationery Stationery { get; set; }
    }
}
