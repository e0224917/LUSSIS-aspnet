using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LUSSIS.Models.WebDTO
{
    public class RetrievalItemsWithDateDTO
    {
        public List<RetrievalItemDTO> retrievalItems { get; set; }


        [DataType(DataType.Date)]
        public DateTime collectionDate { get; set; }    

        public bool hasInprocessDisbursement { get; set; }
    }
}