namespace LUSSIS.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Disbursement")]
    public partial class Disbursement
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Disbursement()
        {
            DisbursementDetails = new HashSet<DisbursementDetail>();
        }

        public int DisbursementId { get; set; }

        [Column(TypeName = "date")]
        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime? CollectionDate { get; set; }

        public int? CollectionPointId { get; set; }

        [StringLength(20)]
        public string DeptCode { get; set; }

        public int? AcknowledgeEmpNum { get; set; }

        [StringLength(20)]
        public string Status { get; set; }

        public virtual CollectionPoint CollectionPoint { get; set; }

        public virtual Department Department { get; set; }

        public virtual Employee AcknowledgeEmployee { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DisbursementDetail> DisbursementDetails { get; set; }
    }
}
