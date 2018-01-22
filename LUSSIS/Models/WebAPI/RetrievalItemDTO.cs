using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUSSIS.Models.WebAPI
{
    public class RetrievalItemDTO
    {
        public string ItemNum { get; set; }
        public string BinNum { get; set; }
        public string Description { get; set; }
        public string UnitOfMeasure { get; set; }
        //stock qty
        public int? AvailableQty { get; set; }
        //assocaited approved requisition qty
        public int? RequestedQty { get; set; }
    }
}