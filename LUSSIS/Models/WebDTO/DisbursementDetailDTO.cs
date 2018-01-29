using System.Collections.Generic;

namespace LUSSIS.Models.WebDTO
{
    public class DisbursementDetailDTO
    {
        public Disbursement CurrentDisbursement { get; set; }
        public List<DisbursementDetail> DisDetailList { get; set; }  
    }
}