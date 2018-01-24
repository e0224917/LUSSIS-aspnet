namespace LUSSIS.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Stationery")]
    public partial class Stationery
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Stationery()
        {
            AdjVouchers = new HashSet<AdjVoucher>();
            DisbursementDetails = new HashSet<DisbursementDetail>();
            PurchaseOrderDetails = new HashSet<PurchaseOrderDetail>();
            ReceiveTransDetails = new HashSet<ReceiveTransDetail>();
            RequisitionDetails = new HashSet<RequisitionDetail>();
            StationerySuppliers = new HashSet<StationerySupplier>();
        }

        [Key]
        [StringLength(20)]
        public string ItemNum { get; set; }

        public int? CategoryId { get; set; }

        //[Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        //[Required(ErrorMessage = "ReorderLevel is required")]
        public int? ReorderLevel { get; set; }

        //[Required(ErrorMessage = "ReorderQty is required")]
        public int? ReorderQty { get; set; }

        public double? AverageCost { get; set; }

        //[Required(ErrorMessage = "UnitOfMeasure is required")]
        [StringLength(10)]
        public string UnitOfMeasure { get; set; }

        public int? CurrentQty { get; set; }

        //[Required(ErrorMessage = "BinNum is required")]
        [StringLength(10)]
        public string BinNum { get; set; }

        public int? AvailableQty { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AdjVoucher> AdjVouchers { get; set; }

        public virtual Category Category { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<DisbursementDetail> DisbursementDetails { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PurchaseOrderDetail> PurchaseOrderDetails { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ReceiveTransDetail> ReceiveTransDetails { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RequisitionDetail> RequisitionDetails { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<StationerySupplier> StationerySuppliers { get; set; }
    }
}
