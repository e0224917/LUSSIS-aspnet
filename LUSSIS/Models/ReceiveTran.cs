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
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReceiveId { get; set; }

        [Required]
        public int PoNum { get; set; }

        [Required]
        [Column(TypeName = "date")]
        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime ReceiveDate { get; set; }

        [Required]
        [StringLength(30)]
        public string InvoiceNum { get; set; }

        [Required]
        [StringLength(30)]
        public string DeliveryOrderNum { get; set; }

        public virtual PurchaseOrder PurchaseOrder { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ReceiveTransDetail> ReceiveTransDetails { get; set; }
    }
}
