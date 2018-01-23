using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUSSIS.Models.WebAPI
{
    public class DisbursementDTO
    {
        public int DisbursementId { get; set; }

        public int CollectionPointId { get; set; }

        public String CollectionPoint { get; set; }

        public DateTime CollectionDate { get; set; }

        public IEnumerable<RequisitionDetailDTO> DisbursementDetails { get; set; }
    }
}