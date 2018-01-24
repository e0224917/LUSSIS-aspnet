namespace LUSSIS.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("StationerySupplier")]
    public partial class StationerySupplier
    {
        [Key]
        [Column(Order = 0)]
        [StringLength(20)]
        public string ItemNum { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SupplierId { get; set; }

        [Required]
        public double Price { get; set; }

        [Required]
        public int Rank { get; set; }

        public virtual Stationery Stationery { get; set; }

        public virtual Supplier Supplier { get; set; }
    }
}
