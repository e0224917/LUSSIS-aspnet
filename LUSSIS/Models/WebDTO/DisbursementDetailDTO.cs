using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUSSIS.Models.WebDTO
{
    //Authors: Tang Xiaowen
    public class DisbursementDetailDTO
    {
        public Disbursement CurrentDisbursement { get; set; }
        public List<DisbursementDetail> DisDetailList { get; set; }  
    }
}