using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUSSIS.Models.WebDTO
{
    /*
    *Same item from requisition detai table become ONE object of this class. 
    *this DTO is used to facilitate retrieval
    */

    public class RetrievalItemDTO
    {
        static public DateTime collectionDate { get; set; }

        public string ItemNum { get; set; }
        public string BinNum { get; set; }
        public string Description { get; set; }
        public string UnitOfMeasure { get; set; }
        //stock qty
        public int? AvailableQty { get; set; }
        //assocaited approved requisition qty
        public int? RequestedQty { get; set; }
        //qty short from unfullfilled disbursement
        public int? RemainingQty { get; set; }
    }
}