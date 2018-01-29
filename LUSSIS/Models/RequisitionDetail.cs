namespace LUSSIS.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("RequisitionDetail")]
    public partial class RequisitionDetail
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int RequisitionId { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(20)]
        public string ItemNum { get; set; }

        [Required]
        public int Quantity { get; set; }

        public virtual Requisition Requisition { get; set; }

        public virtual Stationery Stationery { get; set; }
    }
}
