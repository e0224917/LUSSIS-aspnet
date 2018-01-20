using System.ComponentModel;
using LUSSIS.Validations;

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

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DisbursementId { get; set; }

        [Column(TypeName = "date")]
        [DataType(DataType.Date)]
        [Display(Name="Collection Date")]
        [DisplayFormat(DataFormatString = "{0:dd-MMM-yyyy}", ApplyFormatInEditMode = true)]
        //[CollectionDate] //validation
        public DateTime? CollectionDate { get; set; }

        [Display(Name = "Collection Point Id")]
        public int? CollectionPointId { get; set; }

        [StringLength(20)]
        [Display(Name = "Department Code")]
        public string DeptCode { get; set; }

        public int? AcknowledgeEmpNum { get; set; }

        [StringLength(20)]
        public string Status { get; set; }

        [Display(Name = "Collection Point")]
        public virtual CollectionPoint CollectionPoint { get; set; }

        public virtual Department Department { get; set; }

        public virtual Employee AcknowledgeEmployee { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DisbursementDetail> DisbursementDetails { get; set; }
    }
}
