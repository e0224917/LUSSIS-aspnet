using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUSSIS.Models.WebAPI
{
    //Authors: Ton That Minh Nhat
    public class DisbursementDTO
    {
        public DisbursementDTO(Disbursement disbursement)
        {
            DisbursementId = disbursement.DisbursementId;
            CollectionDate = disbursement.CollectionDate;
            CollectionPoint = disbursement.CollectionPoint.CollectionName;
            CollectionPointId = (int)disbursement.CollectionPointId;
            CollectionTime = disbursement.CollectionPoint.Time;
            DepartmentName = disbursement.Department.DeptName;
            DisbursementDetails = disbursement.DisbursementDetails.Select(details => new RequisitionDetailDTO()
            {
                ItemNum = details.ItemNum,
                Description = details.Stationery.Description,
                Quantity = details.ActualQty,
                UnitOfMeasure = details.Stationery.UnitOfMeasure
            });
        }

        public int DisbursementId { get; set; }

        public int CollectionPointId { get; set; }

        public string CollectionPoint { get; set; }

        public DateTime CollectionDate { get; set; }

        public string CollectionTime { get; set; }

        public string DepartmentName { get; set; }

        public IEnumerable<RequisitionDetailDTO> DisbursementDetails { get; set; }
    }
}