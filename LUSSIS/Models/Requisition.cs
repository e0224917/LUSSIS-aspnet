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

        public int RequisitionId { get; set; }

        public int? RequisitionEmpNum { get; set; }

        [Column(TypeName = "date")]
        public DateTime? RequisitionDate { get; set; }

        public int? ApprovalEmpNum { get; set; }

        public string ApprovalRemarks { get; set; }

        public string RequestRemarks { get; set; }

        [Column(TypeName = "date")]
        public DateTime? ApprovalDate { get; set; }

        [StringLength(20)]
        public string Status { get; set; }
        //ApproveEmp
        public virtual Employee Employee { get; set; }
        //RequisitionEmp
        public virtual Employee Employee1 { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RequisitionDetail> RequisitionDetails { get; set; }
    }
}
