using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using LUSSIS.Constants;
using LUSSIS.Models;
using LUSSIS.Models.WebDTO;
using static LUSSIS.Constants.DisbursementStatus;

namespace LUSSIS.Repositories
{
    //Authors: Tang Xiaowen, May Zin Ko, Ong Xin Ying, Guo Rui
    public class DisbursementRepository : Repository<Disbursement, int>
    {
        public Disbursement GetByDateAndDeptCode(DateTime nowDate, string deptCode)
        {
            try
            {
                var updatedDate = nowDate.Subtract(new TimeSpan(1, 0, 0, 0));
                var disbursements = LUSSISContext.Disbursements.Where(x => x.DeptCode == deptCode).ToList();
                return disbursements.First(x => x.CollectionDate > updatedDate && x.Status == InProcess);
            }
            catch
            {
                return null;
            }
        }

        public IEnumerable<Disbursement> GetDisbursementsByDeptName(string deptName)
        {
            return LUSSISContext.Disbursements.Where(d => d.Department.DeptName.Contains(deptName));
        }

        public IEnumerable<DisbursementDetail> GetDisbursementDetailsByStatus(string status)
        {
            return LUSSISContext.DisbursementDetails.Where(x => x.Disbursement.Status == status);
        }

        public IEnumerable<Disbursement> GetDisbursementByStatus(string status)
        {
            return LUSSISContext.Disbursements.Where(x => x.Status == status);
        }

        public IEnumerable<DisbursementDetail> GetUnfulfilledDisbursementDetails()
        {
            return LUSSISContext.DisbursementDetails.Where(d =>
                    d.Disbursement.Status == Unfulfilled && d.RequestedQty - d.ActualQty > 0)
                .Include(d => d.Stationery);
        }

        public List<RequisitionDetail> GetApprovedRequisitionDetailsByDeptCode(string deptCode)
        {
            return LUSSISContext.RequisitionDetails
                .Where(r => r.Requisition.DeptCode == deptCode
                            && r.Requisition.Status == RequisitionStatus.Approved).ToList();
        }

        public IEnumerable<Requisition> GetApprovedRequisitions()
        {
            return LUSSISContext.Requisitions.Where(r => r.Status == RequisitionStatus.Approved).ToList();
        }

        public void UpdateRequisition(Requisition requisition)
        {
            LUSSISContext.Entry(requisition).State = EntityState.Modified;
            LUSSISContext.SaveChanges();
        }

        public IEnumerable<Disbursement> GetUnfulfilledDisbursements()
        {
            return GetDisbursementByStatus(Unfulfilled);
        }

        /// <summary>
        /// /for supervisor's dashboard
        /// </summary>
        /// <returns></returns>
        public double GetDisbursementTotalAmount(List<String>fromList)
        {
            double result = 0;
            foreach (String from in fromList)
            {
                DateTime fromDate = DateTime.ParseExact(from, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                List<DisbursementDetail> disList = LUSSISContext.DisbursementDetails.Where(x => x.Disbursement.Status != InProcess
                  && x.Disbursement.CollectionDate.Month == fromDate.Month && x.Disbursement.CollectionDate.Year == fromDate.Year).ToList();
               
                foreach(DisbursementDetail d in disList)
                {
                    result += d.UnitPrice * d.ActualQty;
                }

            }

            return result;
        }

      
        public void Acknowledge(Disbursement disbursement)
        {
            var isFulfilled = disbursement.DisbursementDetails.All(item => item.ActualQty == item.RequestedQty);
            disbursement.Status = isFulfilled ? Fulfilled : Unfulfilled;

            disbursement.AcknowledgeEmpNum = disbursement.Department.RepEmpNum;
            Update(disbursement);
            LUSSISContext.SaveChanges();
        }


        public bool HasInprocessDisbursements()
        {
            return LUSSISContext.Disbursements.Any(d => d.Status == InProcess);
        }


        public DisbursementDetail GetDisbursementDetailByIdAndItem(string id, string itemNum)
        {
            return LUSSISContext.DisbursementDetails.FirstOrDefault(dd =>
                dd.DisbursementId == Convert.ToInt32(id) && dd.ItemNum == itemNum);
        }

        public Disbursement GetUpcomingDisbursement(string deptCode)
        {
            return LUSSISContext.Disbursements
                .FirstOrDefault(d => d.Status == InProcess && d.DeptCode == deptCode);
        }

        public IEnumerable<DisbursementDetail> GetAllDisbursementDetailByItem(string id)
        {
            return LUSSISContext.DisbursementDetails.Where(x => x.ItemNum == id);
        }


        public double GetDisAmountByDep(String dep, List<int> cat, String from)
        {
            DateTime fromDate = DateTime.ParseExact(from, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            
            double total = 0;

            foreach (int catId in cat)
            {
                List<DisbursementDetail> disList = LUSSISContext.DisbursementDetails.Where(x => x.Disbursement.Status != InProcess
                 && x.Disbursement.CollectionDate.Month == fromDate.Month 
                 && x.Disbursement.CollectionDate.Year == fromDate.Year
                 && x.Disbursement.DeptCode==dep
                 && x.Stationery.CategoryId==catId ).ToList();

                foreach(DisbursementDetail d in disList)
                {
                    total += d.UnitPrice * d.ActualQty;
                }

            }

            return total;
        }


        public void UpdateDisbursementDetail(DisbursementDetail detail)
        {
            LUSSISContext.Entry(detail).State = EntityState.Modified;
            LUSSISContext.SaveChanges();
        }

        public IEnumerable<RetrievalItemDTO> GetRetrievalInProcess()
        {
            var itemsToRetrieve = new List<RetrievalItemDTO>();
            
            //group inprocess disbursement by stationery
            List<List<DisbursementDetail>> inProcessDisDetailsGroupedByItem = GetDisbursementDetailsByStatus(InProcess)
                .GroupBy(x => x.ItemNum).Select(grp => grp.ToList()).ToList();

            //list for each stationery forms one RetrievalItemDTO
            foreach (var disDetailForOneItem in inProcessDisDetailsGroupedByItem)
            {
                var stat = disDetailForOneItem.First().Stationery;
                var retrievalItem = new RetrievalItemDTO(stat);
                foreach (var disbursementDetail in disDetailForOneItem)
                {
                    retrievalItem.RequestedQty += disbursementDetail.RequestedQty;
                }
                itemsToRetrieve.Add(retrievalItem);
            }

            return itemsToRetrieve;
        }

        public IEnumerable<DisbursementDetail> GetAllDisbursementDetails()
        {
            return LUSSISContext.DisbursementDetails.ToList();
        }
    }
}