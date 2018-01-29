using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUSSIS.Models.WebAPI
{
    //Authors: Ton That Minh Nhat
    public class DisbursementDTO
    {
        public int DisbursementId { get; set; }

        public int CollectionPointId { get; set; }

        public string CollectionPoint { get; set; }

        public DateTime CollectionDate { get; set; }

        public string CollectionTime { get; set; }

        public string DepartmentName { get; set; }

        public IEnumerable<RequisitionDetailDTO> DisbursementDetails { get; set; }
    }
}