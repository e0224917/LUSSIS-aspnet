namespace LUSSIS.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("CollectionPoint")]
    public partial class CollectionPoint
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public CollectionPoint()
        {
            Departments = new HashSet<Department>();
            Disbursements = new HashSet<Disbursement>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CollectionPointId { get; set; }

        [StringLength(30)]
        [Display(Name = "Collection Point")]
        public string CollectionName { get; set; }

        [StringLength(20)]
        public string Time { get; set; }

        public int? InChargeEmpNum { get; set; }

        [Display(Name = "Person In Charge")]
        public virtual Employee InChargeEmployee { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Department> Departments { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Disbursement> Disbursements { get; set; }
    }
}
