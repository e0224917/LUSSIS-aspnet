namespace LUSSIS.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class ReceiveTran
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ReceiveTran()
        {
            ReceiveTransDetails = new HashSet<ReceiveTransDetail>();
        }

        [Key]
        public int ReceiveId { get; set; }

        public int? PoNum { get; set; }

        [Column(TypeName = "date")]
        public DateTime? ReceiveDate { get; set; }

        [StringLength(30)]
        public string InvoiceNum { get; set; }

        [StringLength(30)]
        public string DeliveryOrderNum { get; set; }

        public virtual PurchaseOrder PurchaseOrder { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ReceiveTransDetail> ReceiveTransDetails { get; set; }
    }
}
