namespace LUSSIS.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Supplier")]
    public partial class Supplier
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Supplier()
        {
            PurchaseOrders = new HashSet<PurchaseOrder>();
            StationerySuppliers = new HashSet<StationerySupplier>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SupplierId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Supplier Name")]
        public string SupplierName { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Contact Name")]
        public string ContactName { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Phone No")]
        public string TelephoneNum { get; set; }

        [Required]
        [StringLength(30)]
        [Display(Name = "Fax No")]
        public string FaxNum { get; set; }

        [Required]
        public string Address1 { get; set; }

        public string Address2 { get; set; }

        public string Address3 { get; set; }

        [Required]
        [StringLength(30)]
        [Display(Name = "Gst Registration")]
        public string GstRegistration { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<StationerySupplier> StationerySuppliers { get; set; }
    }
}
