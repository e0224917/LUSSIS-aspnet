using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web;
using LUSSIS.Constants;
using LUSSIS.Models;
using LUSSIS.Models.WebDTO;
using static LUSSIS.Constants.DisbursementStatus;

namespace LUSSIS.Repositories
{
    //Authors: Tang Xiaowen, May Zin Ko
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
            return LUSSISContext.DisbursementDetails.Where(x => x.Disbursement.Status == status).ToList();
        }

        public IEnumerable<Disbursement> GetDisbursementByStatus(string status)
        {
            return LUSSISContext.Disbursements.Where(x => x.Status == status).ToList();
        }

        public List<DisbursementDetail> GetUnfulfilledDisbursementDetailList()
        {
            return LUSSISContext.DisbursementDetails.Where(d =>
                    d.Disbursement.Status == Unfulfilled && d.RequestedQty - d.ActualQty > 0)
                .Include(d => d.Stationery).ToList();
        }

        /// <summary>
        /// /for supervisoer' dashboard
        /// </summary>
        /// <returns></returns>
        public double GetDisbursementTotalAmount()
        {
            double result = 0;
            var list = GetAll().Where(x => x.Status != InProcess).ToList();
            foreach (Disbursement d in list)
            {
                result += GetAmountByDisbursement(d);
            }

            return result;
        }

        public double GetDisbursementTotalAmountOfDept(string deptCode)
        {
            double result = 0;

            var list = GetAll().Where(x => x.Status != InProcess && x.DeptCode.Equals(deptCode)).ToList();
            foreach (Disbursement d in list)
            {
                result += GetAmountByDisbursement(d);
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

        public double GetAmountByDisbursement(Disbursement d)
        {
            double result = 0;
            var detailList = d.DisbursementDetails.ToList();
            foreach (DisbursementDetail f in detailList)
            {
                int qty = f.ActualQty;
                double unitPrice = f.UnitPrice;
                result += (qty * unitPrice);
            }

            return result;
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


        public double GetDisAmountByDate(String dep, List<int> cat, String from, String to)
        {
            //  DateTime fromDate = DateTime.ParseExact(from, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            // DateTime toDate = DateTime.ParseExact(to, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            DateTime fromDate = DateTime.Parse(from);
            DateTime toDate = DateTime.Parse(to);
            double total = 0;

            foreach (int catId in cat)
            {
                var q = from t1 in LUSSISContext.Disbursements
                    join t2 in LUSSISContext.DisbursementDetails
                        on t1.DisbursementId equals t2.DisbursementId
                    join t3 in LUSSISContext.Stationeries
                        on t2.ItemNum equals t3.ItemNum
                    where t3.CategoryId == catId &&
                          t1.Status != InProcess &&
                          t1.DeptCode == dep
                          && (t1.CollectionDate <= toDate && t1.CollectionDate >= fromDate)
                    select new
                    {
                        price = (int) t2.Stationery.AverageCost,
                        Qty = (double) t2.ActualQty
                    };

                foreach (var a in q)
                {
                    total += a.price * a.Qty;
                }
            }

            return total;
        }


        public void UpdateDisbursementDetail(DisbursementDetail detail)
        {
            LUSSISContext.Entry(detail).State = EntityState.Modified;
            LUSSISContext.SaveChanges();
        }

        public List<RequisitionDetail> GetApprovedRequisitionDetailsByDeptCode(string deptCode)
        {
            return LUSSISContext.RequisitionDetails
                .Where(r => r.Requisition.DeptCode == deptCode
                            && r.Requisition.Status == "approved").ToList();
        }

        public IEnumerable<Requisition> GetApprovedRequisitions()
        {
            return LUSSISContext.Requisitions.Where(r => r.Status == "approved").ToList();
        }

        public void UpdateRequisition(Requisition requisition)
        {
            LUSSISContext.Entry(requisition).State = EntityState.Modified;
            LUSSISContext.SaveChanges();
        }

        public IEnumerable<RetrievalItemDTO> GetRetrievalInProcess()
        {
            var itemsToRetrieve = new List<RetrievalItemDTO>();

            //use disbursement as resource to generate retrieval in process

            var inProcessDisDetailsGroupedByItem = GetDisbursementDetailsByStatus(InProcess)
                .GroupBy(x => x.ItemNum).Select(grp => grp.ToList()).ToList();

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
    }
}