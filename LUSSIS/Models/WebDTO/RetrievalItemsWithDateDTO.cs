using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LUSSIS.Models.WebDTO
{
    //Authors: Tang Xiaowen
    public class RetrievalItemsWithDateDTO
    {
        public List<RetrievalItemDTO> retrievalItems { get; set; }

        
        public string collectionDate { get; set; }    

        public bool hasInprocessDisbursement { get; set; }
    }
}