namespace LUSSIS.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("AdjVoucher")]
    public partial class AdjVoucher
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int AdjVoucherId { get; set; }

        [Required]
        [StringLength(20)]
        public string ItemNum { get; set; }

        public int? ApprovalEmpNum { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [StringLength(50)]
        public string Reason { get; set; }

        [Required]
        [Column(TypeName = "date")]
        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime CreateDate { get; set; }

        [Column(TypeName = "date")]
        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime? ApprovalDate { get; set; }

        [Required]
        public int RequestEmpNum { get; set; }

        [Required]
        [StringLength(10)]
        public string Status { get; set; }

        [StringLength(50)]
        public string Remark { get; set; }

        [ForeignKey("ApprovalEmpNum")]
        public virtual Employee ApprovalEmployee { get; set; }

        [ForeignKey("RequestEmpNum")]
        public virtual Employee RequestEmployee { get; set; }

        public virtual Stationery Stationery { get; set; }

    }
}
