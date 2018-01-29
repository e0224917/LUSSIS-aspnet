using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUSSIS.Models.WebDTO
{
    //Authors: Douglas Lee Kiat Hui
    public class StationeryTransactionDTO
    {
        public DateTime Date { get; set; }
        public string Transtype { get; set; }
        public string Remarks { get; set; }
        public int Qty { get; set; }
    }
}