using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using LUSSIS.Emails;
using LUSSIS.Models;

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
                return disbList.First(x => x.CollectionDate > updatedDate && x.Status == "inprocess");
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


        

        //Please put this inside collectionRepo
        public IEnumerable<CollectionPoint> GetAllCollectionPoint()
        {
            return LUSSISContext.CollectionPoints.Include(c => c.InChargeEmployee);
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

        public IEnumerable<DisbursementDetail> GetInProcessDisbursementDetails()
        {
            return GetDisbursementDetailsByStatus("inprocess");
        }

        public List<DisbursementDetail> GetUnfulfilledDisbursementDetailList()
        {
            return LUSSISContext.DisbursementDetails.Where(d =>
                d.Disbursement.Status == "unfulfilled" && d.RequestedQty - d.ActualQty > 0)
                .Include(d => d.Stationery).ToList();
        }

        /// <summary>
        /// /for supervisoer' dashboard
        /// </summary>
        /// <returns></returns>
        public double GetDisbursementTotalAmount()
        {
            double result = 0;
            var list = GetAll().Where(x => x.Status != "unprocessed").ToList();
            foreach (Disbursement d in list)
            {
                result += GetAmountByDisbursement(d);
            }
            return result;
        }

        public double GetDisbursementTotalAmountOfDept(string deptCode)
        {
            double result = 0;

            var list = GetAll().Where(x => x.Status != "unprocessed" && x.DeptCode.Equals(deptCode)).ToList();
            foreach (Disbursement d in list)
            {
                result += GetAmountByDisbursement(d);
            }


            return result;
        }

        //签收disbursement
        public void Acknowledge(Disbursement disbursement)
        {
            bool fulFilled = true;
            foreach (var disD in disbursement.DisbursementDetails)
            {
                if (disD.RequestedQty > disD.ActualQty)
                {
                    fulFilled = false;
                    break;
                }
            }
            if (fulFilled)
            {
                disbursement.Status = "fulfilled";
            }
            else
            {
                disbursement.Status = "unfulfilled";
            }

            disbursement.AcknowledgeEmpNum = disbursement.Department.RepEmpNum;
            Update(disbursement);
            LUSSISContext.SaveChanges();
        }

        public bool hasInprocessDisbursements()
        {
            return LUSSISContext.Disbursements.Any(d => d.Status == "inprocess");
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
            return LUSSISContext.DisbursementDetails.FirstOrDefault(dd => (dd.DisbursementId == Convert.ToInt32(id)) && dd.ItemNum == itemNum);
        }

        public Disbursement GetUpcomingDisbursement(string deptCode)
        {
            return LUSSISContext.Disbursements
                .FirstOrDefault(d => d.Status == "inprocess" && d.DeptCode == deptCode);
        }

        public IEnumerable<DisbursementDetail> GetAllDisbursementDetailByItem(string id)
        {
            return LUSSISContext.DisbursementDetails.Where(x => x.ItemNum == id);
        }

        public List<double> GetAmountByDepAndCatList(String depCode,List<String> catId,String from,String to)
        {
            DateTime fromDate = DateTime.ParseExact(from, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime toDate = DateTime.ParseExact(to, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            double total = 0;
           
            List<double> result= new List<double>();
            foreach (String id in catId)
            {
                List<String>itemList= new StationeryRepository().GetItembyCategory(Convert.ToInt32(id));
                //int Id= Convert.ToInt32(id);
                total = 0;
                foreach(String item in itemList)
                {
                    var s = from t1 in LUSSISContext.Disbursements
                            join t2 in LUSSISContext.DisbursementDetails
                            on t1.DisbursementId equals t2.DisbursementId
                            where t2.Stationery.ItemNum == item &&
                            t1.DeptCode == depCode &&
                            (t1.CollectionDate >= fromDate && t1.CollectionDate <= toDate)
                            select new
                            {
                                price = (int)t2.Stationery.AverageCost,
                                Qty = (double)t2.ActualQty
                            };
                    foreach (var a in s)
                    {
                        total += a.Qty;
                    }
                }
               
                result.Add(total);

            }

            return result;
        }

        public List<double> GetAmoutByCatAndDepList(String cat,List<String>depList,String from,String to)
        {
            DateTime fromDate = DateTime.ParseExact(from, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime toDate = DateTime.ParseExact(to, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            double total = 0;
            
            List<double> resultList = new List<double>();
            int catId = Convert.ToInt32(cat);
            foreach (String dep in depList)
            {
                total = 0;
                    var q = from t1 in LUSSISContext.Disbursements
                            join t2 in LUSSISContext.DisbursementDetails
                            on t1.DisbursementId equals t2.DisbursementId
                            join t3 in LUSSISContext.Stationeries
                            on t2.ItemNum equals t3.ItemNum
                            where t3.CategoryId==catId &&
                            t1.DeptCode == dep &&
                            (t1.CollectionDate >= fromDate && t1.CollectionDate <= toDate)
                            select new
                            {
                                price = (int)t2.Stationery.AverageCost,
                                Qty = (double)t2.ActualQty
                            };
                    foreach (var a in q)
                    {
                        total += a.price * a.Qty;
                    }
                resultList.Add(total);
            }
               
            return resultList;
        }
        public List<double> GetMaxCategoryAmountByDep(List<String>catList, List<String> depList,String from,String to)
        {
            DateTime fromDate =  DateTime.ParseExact(from, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime toDate = DateTime.ParseExact(to, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            
            List<double> resultList = new List<double>();
            List<double> findMax=new List<double>();
            foreach (String dep in depList)
            {
                double total = 0;
                foreach (String cat in catList)
                {
                    
                    int catId = Convert.ToInt32(cat);
                   findMax = new List<double>();
                    var q = from t1 in LUSSISContext.Disbursements
                            join t2 in LUSSISContext.DisbursementDetails
                            on t1.DisbursementId equals t2.DisbursementId
                            join t3 in LUSSISContext.Stationeries
                            on t2.ItemNum equals t3.ItemNum
                            where t3.CategoryId == catId &&
                            t1.DeptCode == dep 
                          
                            select new
                            {
                                price = (int)t2.Stationery.AverageCost,
                                Qty = (double)t2.ActualQty
                            };
                    foreach (var a in q)
                    {
                        total += a.Qty;
                    }
                    
                }
               
                resultList.Add(total);
            }
           
            return resultList;
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

    }

    
}