using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace LUSSIS.Models.WebDTO
{
    [NotMapped]
    public class AdjustmentVoucherDTO
    {

        [StringLength(20)]
        public string ItemNum { get; set; }


        public int Quantity { get; set; }


        [StringLength(50)]
        public string Reason { get; set; }



        public virtual Stationery Stationery { get; set; }


        [Required(ErrorMessage ="This field is required")]
        public int? Sign{ get; set; }
    }
}