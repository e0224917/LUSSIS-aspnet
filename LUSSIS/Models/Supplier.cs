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

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SupplierId { get; set; }

        [StringLength(50)]
        public string SupplierName { get; set; }

        [StringLength(20)]
        public string ContactName { get; set; }

        [StringLength(20)]
        public string TelephoneNum { get; set; }

        [StringLength(30)]
        public string FaxNum { get; set; }

        public string Address { get; set; }

        [StringLength(30)]
        public string GstRegistration { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<StationerySupplier> StationerySuppliers { get; set; }
    }
}
