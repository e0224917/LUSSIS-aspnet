using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using PagedList;

namespace LUSSIS.Models.WebDTO
{
    //Authors: Tang Xiaowen
    public class RetrievalItemsWithDateDTO
    {
        public IPagedList<RetrievalItemDTO> RetrievalItems { get; set; }
        
        public string CollectionDate { get; set; }    

        public bool HasInprocessDisbursement { get; set; }
    }
}