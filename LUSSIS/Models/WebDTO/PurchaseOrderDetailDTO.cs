using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace LUSSIS.Models.WebDTO
{
    //Authors: Douglas Lee Kiat Hui, Guo Rui
    public class PurchaseOrderDetailDTO
    {
        public int PoNum { get; set; }
        public string ItemNum { get; set; }
        [Range(0,999,ErrorMessage ="cannot be negative")]
        public int OrderQty { get; set; }
        public double UnitPrice { get; set; }
        public int ReceiveQty { get; set; }
        public int ReorderQty { get; set; }
    }
}