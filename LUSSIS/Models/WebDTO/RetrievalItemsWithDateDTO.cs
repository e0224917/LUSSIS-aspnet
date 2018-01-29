using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using PagedList;

namespace LUSSIS.Models.WebDTO
{
    public class RetrievalItemsWithDateDTO
    {
        public IPagedList<RetrievalItemDTO> retrievalItems { get; set; }
        
        public string collectionDate { get; set; }    

        public bool hasInprocessDisbursement { get; set; }
    }
}