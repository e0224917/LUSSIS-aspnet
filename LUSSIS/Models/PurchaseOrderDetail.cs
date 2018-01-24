namespace LUSSIS.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("PurchaseOrderDetail")]
    public partial class PurchaseOrderDetail
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PoNum { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(20)]
        public string ItemNum { get; set; }

        [Required]
        public int OrderQty { get; set; }

        [Required]
        public double UnitPrice { get; set; }

        [Required]
        public int ReceiveQty { get; set; }

        public virtual PurchaseOrder PurchaseOrder { get; set; }

        public virtual Stationery Stationery { get; set; }
    }
}
