namespace LUSSIS.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Requisition")]
    public partial class Requisition
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Requisition()
        {
            RequisitionDetails = new HashSet<RequisitionDetail>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RequisitionId { get; set; }
        [Required]
        public int RequisitionEmpNum { get; set; }
        [Required]
        [Column(TypeName = "date")]
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        public DateTime RequisitionDate { get; set; }

        public int? ApprovalEmpNum { get; set; }
        //[Required]
        //public string  DeptCode { get; set; }
        public string ApprovalRemarks { get; set; }

        public string RequestRemarks { get; set; }

        [Column(TypeName = "date")]
        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime? ApprovalDate { get; set; }
        [Required]
        [StringLength(20)]
        public string Status { get; set; }
        [Required]
        public string DeptCode { get; set; }
        public virtual Employee ApprovalEmployee { get; set; }

        public virtual Employee RequisitionEmployee { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RequisitionDetail> RequisitionDetails { get; set; }
    }
}
