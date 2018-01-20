using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using LUSSIS.Models;
using LUSSIS.Models.WebDTO;
using Microsoft.ApplicationInsights.WindowsServer;

namespace LUSSIS.Repositories
{
    public class DisbursementRepository : Repository<Disbursement, int>
    {
        public Disbursement GetByDateAndDeptCode(DateTime nowDate, string deptCode)
        {
            try
            {
                DateTime updatedDate = nowDate.Subtract(new TimeSpan(1, 0, 0, 0));
                List<Disbursement> disbList = LUSSISContext.Disbursements.Where(x => x.DeptCode == deptCode).ToList();
                return disbList.First(x => x.CollectionDate > updatedDate && x.Status == "in process");
            }
            catch
            {
                return null;
            }
        }

        public CollectionPoint GetCollectionPointByDisbursement(Disbursement disbursement)
        {
            return LUSSISContext.CollectionPoints.First(y => y.CollectionPointId == disbursement.CollectionPointId);
        }

        public CollectionPoint GetCollectionPointByDeptCode(string deptCode)
        {
            Department d = new Department();
            d = LUSSISContext.Departments.First(z => z.DeptCode == deptCode);
            return LUSSISContext.CollectionPoints.First(x => x.CollectionPointId == d.CollectionPointId);
        }

        public List<DisbursementDetail> GetDisbursementDetails(Disbursement disbursement)
        {
            return LUSSISContext.DisbursementDetails.Where(x => x.DisbursementId == disbursement.DisbursementId)
                .ToList();
        }

        public IEnumerable<DisbursementDetail> GetDisbursementDetailsByStatus(string status)
        {
            return LUSSISContext.DisbursementDetails.Where(x => x.Disbursement.Status == status).ToList();
        }

        public IEnumerable<Disbursement> GetDisbursementByStatus(string status)
        {
            return LUSSISContext.Disbursements.Where(x => x.Status == status).ToList();
        }

        public IEnumerable<Disbursement> GetInProcessDisbursements()
        {
            return GetDisbursementByStatus("inprocess");
        }

        public IEnumerable<DisbursementDetail> GetUnfullfilledDisDetailList()
        {
            return LUSSISContext.DisbursementDetails.Where(d => (d.RequestedQty - d.ActualQty) > 0).ToList();
        }

        public void CreateDisbursement(DateTime collectionDate)
        {
            List<Disbursement> disbursements = new List<Disbursement>();

            //group requisition requests by dept and create disbursement list based on it
            List<Requisition> approvedReq = LUSSISContext.Requisitions.Where(r => r.Status == "approved").ToList();

            List<List<Requisition>> reqGroupByDep = approvedReq.GroupBy(r => r.RequisitionEmployee.DeptCode)
                .Select(grp => grp.ToList()).ToList();
            foreach (List<Requisition> reqForOneDep in reqGroupByDep)
            {
                Disbursement d = ConvertReqListForOneDepToDisbursement(reqForOneDep, collectionDate);

                disbursements.Add(d);
                foreach (Requisition req in reqForOneDep)
                {
                    req.Status = "processed";
                }
            }


            //get unfullfilled disbursement
            List<Disbursement> unfullfilledDisList = GetDisbursementByStatus("unfullfilled").ToList();


            foreach (Disbursement ud in unfullfilledDisList)
            {
                //is ud.DeptCode exsits in disbursements's deptCode? if not, create new
                //if (disbursements.First(x => x.DeptCode == ud.DeptCode) == null)
                //{
                //    Disbursement newD = new Disbursement()
                //    {
                //        DeptCode = ud.DeptCode,
                //        Department = ud.Department,
                //        CollectionDate = collectionDate,
                //        CollectionPoint = ud.Department.CollectionPoint,
                //        CollectionPointId = ud.Department.CollectionPointId,
                //    };
                //    disbursements.Add(newD);
                //}
                bool isNew = true;
                foreach (var d in disbursements)
                {
                    if (d.DeptCode == ud.DeptCode)
                    {
                        isNew = false;
                        break;
                    }
                }

                if (isNew)
                {
                    Disbursement newD = new Disbursement()
                    {
                        DeptCode = ud.DeptCode,
                        Department = ud.Department,
                        Status = "inprocess",
                        CollectionDate = collectionDate,
                        CollectionPoint = ud.Department.CollectionPoint,
                        CollectionPointId = ud.Department.CollectionPointId,
                    };
                    disbursements.Add(newD);
                }

                //if exist, add to found department's
                ud.Status = "fullfilled";
            }

            foreach (var d in disbursements)
            {
                Add(d);
            }

            LUSSISContext.SaveChanges();
        }

        private DisbursementDetail ConvertReDetailToDisDetail(RequisitionDetail rd)
        {
            return new DisbursementDetail(rd.ItemNum, rd.Stationery.AverageCost, rd.Quantity, rd.Stationery);
        }

        private Disbursement ConvertReqListForOneDepToDisbursement(List<Requisition> ReqListForOneDep,
            DateTime collectionDate)
        {
            Disbursement d = new Disbursement
            {
                Status = "inprocess",
                CollectionDate = collectionDate,
                Department = ReqListForOneDep.First().RequisitionEmployee.Department,
            };
            d.DeptCode = d.Department.DeptCode;
            d.CollectionPoint = d.Department.CollectionPoint;
            d.CollectionPointId = d.CollectionPoint.CollectionPointId;
            return d;
        }
    }
}