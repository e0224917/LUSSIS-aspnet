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

        [Range (0,10000, ErrorMessage="Enter number between 0 to 10000")]
        public int Quantity { get; set; }


        [StringLength(50)]
        public string Reason { get; set; }



        public virtual Stationery Stationery { get; set; }

        [Display(Name="Adjustment Type")]
        [Required(ErrorMessage = "This field is required")]
        public int? Sign { get; set; }

        public IList<AdjustmentVoucherDTO> MyList { get; set; }

    }
}