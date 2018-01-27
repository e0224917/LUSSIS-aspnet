using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Entity;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using LUSSIS.Emails;
using LUSSIS.Models;
using LUSSIS.Models.WebDTO;
using Microsoft.ApplicationInsights.WindowsServer;

namespace LUSSIS.Repositories
{
    public class DisbursementRepository : Repository<Disbursement, int>
    {

        StationeryRepository statRepo = new StationeryRepository();

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

        public CollectionPoint GetCollectionPointByDeptCode(string deptCode)
        {
            Department d = LUSSISContext.Departments.First(z => z.DeptCode == deptCode);
            return LUSSISContext.CollectionPoints.First(x => x.CollectionPointId == d.CollectionPointId);
        }

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

        public IEnumerable<Disbursement> GetUnfulfilledDisbursements()
        {
            return GetDisbursementByStatus("unfulfilled");
        }

        public IEnumerable<DisbursementDetail> GetInProcessDisbursementDetails()
        {
            return GetDisbursementDetailsByStatus("inprocess");
        }

        public IEnumerable<DisbursementDetail> GetUnfulfilledDisDetailList()
        {
            return LUSSISContext.DisbursementDetails.Where(d =>
                (d.Disbursement.Status == "unfulfilled") && ((d.RequestedQty - d.ActualQty) > 0)).ToList();
        }

        public void CreateDisbursement(DateTime collectionDate)
        {
            RequisitionRepository reqRepo = new RequisitionRepository();
            List<Disbursement> disbursements = new List<Disbursement>();
            foreach (Disbursement disbursement in disbursements)
            {
                disbursement.DisbursementDetails = new List<DisbursementDetail>();
            }

            //group requisition requests by dept and create disbursement list based on it
            List<Requisition> approvedReq = LUSSISContext.Requisitions.Where(r => r.Status == "approved").ToList();

            List<List<Requisition>> reqGroupByDep = approvedReq.GroupBy(r => r.RequisitionEmployee.DeptCode)
                .Select(grp => grp.ToList()).ToList();
            foreach (List<Requisition> reqForOneDep in reqGroupByDep)
            {
                Disbursement d = ConvertReqListForOneDepToDisbursement(reqForOneDep, collectionDate);

                disbursements.Add(d);

                //(1)将reqForOneDep中所有req的detail ToList (reqdetofRFOP)
                List<RequisitionDetail> reqDetailListForOneDep = new List<RequisitionDetail>();


                foreach (Requisition req in reqForOneDep)
                {
                    req.Status = "processed";
                    //插入req的detail到deqdetofRFOP
                    List<RequisitionDetail> tempReqDList = reqRepo.GetRequisitionDetail(req.RequisitionId).ToList();
                    foreach (RequisitionDetail reqD in tempReqDList)
                    {
                        reqDetailListForOneDep.Add(reqD);
                    }
                }

                //(1)完成
                //(2)将reqdetofRFOP迁移整理到disdetails
                AddReqDtoDisD(reqDetailListForOneDep, d.DisbursementDetails);
                //(2)完成
            }

            List<Disbursement> unfulfilledDisList = GetDisbursementByStatus("unfulfilled").ToList();


            foreach (Disbursement ud in unfulfilledDisList)
            {
                List<DisbursementDetail> unfDisDList = LUSSISContext.DisbursementDetails
                    .Where(x => x.DisbursementId == ud.DisbursementId && (x.RequestedQty - x.ActualQty) > 0).ToList();
                bool isNew = true;
                foreach (var d in disbursements)
                {
                    if (d.DeptCode == ud.DeptCode)
                    {
                        isNew = false;
                        //detail整合
                        AddUnfDisDtoDisD(unfDisDList, d.DisbursementDetails);
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

                    //ud detail直接迁移
                    foreach (DisbursementDetail unfdd in unfDisDList)
                    {
                        DisbursementDetail tempDisD = new DisbursementDetail();
                        tempDisD.ItemNum = unfdd.ItemNum;
                        tempDisD.UnitPrice = unfdd.UnitPrice;
                        tempDisD.RequestedQty = unfdd.RequestedQty - unfdd.ActualQty;
                        tempDisD.ActualQty = unfdd.Stationery.AvailableQty > tempDisD.RequestedQty
                            ? tempDisD.RequestedQty
                            : unfdd.Stationery.AvailableQty;
                        newD.DisbursementDetails.Add(tempDisD);
                        //更改unfdd request qty 
                        unfdd.RequestedQty = unfdd.ActualQty;
                    }
                }

                ud.Status = "fulfilled";
            }

            foreach (var d in disbursements)
            {
                Add(d);
            }

            foreach (var unfd in unfulfilledDisList)
            {
                unfd.Status = "fulfilled";
                Update(unfd);
            }

            LUSSISContext.SaveChanges();

            SendEmailNotifyCollection(disbursements);
        }
        
        public Disbursement UpdateAndNotify(Disbursement disbursement)
        {
            Update(disbursement);
            SendEmailNotifyCollectionUpdate(disbursement);
            return disbursement;
        }

        private void SendEmailNotifyCollection(List<Disbursement> disbursements)
        {
            string subject, body, destinationEmail;
            
            foreach (var dis in disbursements)
            {
                subject = String.Format("Stationery Collection for " + dis.Department.DeptName + " on " +
                                        ((DateTime)dis.CollectionDate).ToShortDateString() +
                                        " at " + dis.CollectionPoint.CollectionName);
                body = String.Format("We have an upcoming collection for " + dis.Department.DeptName
                                                                           + "\n\nDate: \t\t\t" + dis.CollectionDate +
                                                                           " " + dis.CollectionPoint.Time
                                                                           + "\nLocation: \t" +
                                                                           dis.CollectionPoint.CollectionName
                                                                           + "\n\nFor more details, please log in LUSSIS to view: https://localhost:44303/Collection/Index");

                destinationEmail = "sa45team7@gmail.com";
                EmailHelper.SendEmail(destinationEmail, subject, body);
            }
        }

        private void SendEmailNotifyCollectionUpdate(Disbursement dis)
        {
            string subject, body, destinationEmail;
           
            subject = String.Format("Stationery Collection for " + dis.Department.DeptName + " on " +
                                    ((DateTime)dis.CollectionDate).ToShortDateString() +
                                    " has been updated");
            body = String.Format("The upcoming collection for " + dis.Department.DeptName + " has been updated as follow: "
                                 + "\n\nDate: \t\t\t" + dis.CollectionDate + " " + dis.CollectionPoint.Time
                                 + "\nLocation: \t" + dis.CollectionPoint.CollectionName
                                 + "\n\nFor more details, please log in LUSSIS to view: https://localhost:44303/Collection/Index");

            destinationEmail = "sa45team7@gmail.com";
            EmailHelper.SendEmail(destinationEmail, subject, body);
        }

        //helper
        private DisbursementDetail ConvertReDetailToDisDetail(RequisitionDetail rd)
        {
            return new DisbursementDetail(rd.ItemNum, rd.Stationery.AverageCost, rd.Quantity, rd.Stationery);
        }

        //helper
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

        //将reqD整理转入disD
        public void AddReqDtoDisD(List<RequisitionDetail> reqDetailListOfOneDept, ICollection<DisbursementDetail> disDetails)
        {
            foreach (RequisitionDetail rd in reqDetailListOfOneDept)
            {
                bool isNew = true;
                foreach (var disD in disDetails)
                {
                    if (rd.ItemNum == disD.ItemNum)
                    {
                        isNew = false;
                        disD.RequestedQty += rd.Quantity;
                        disD.ActualQty = rd.Stationery.AvailableQty > disD.RequestedQty ? disD.RequestedQty : rd.Stationery.AvailableQty;
                        break;
                    }
                }
                if (isNew)
                {
                    DisbursementDetail newdisD = new DisbursementDetail()
                    {
                        ItemNum = rd.ItemNum,
                        RequestedQty = rd.Quantity,
                        UnitPrice = rd.Stationery.AverageCost,
                        ActualQty = rd.Stationery.AvailableQty > rd.Quantity ? rd.Quantity : rd.Stationery.AvailableQty,
                    };
                    disDetails.Add(newdisD);
                }
            }
        }

        public void AddUnfDisDtoDisD(List<DisbursementDetail> unfDisDList, ICollection<DisbursementDetail> disDetails)
        {
            foreach (DisbursementDetail unfDisD in unfDisDList)
            {
                bool isNew = true;
                foreach (var disD in disDetails)
                {
                    if (unfDisD.ItemNum == disD.ItemNum)
                    {
                        isNew = false;
                        disD.RequestedQty += unfDisD.RequestedQty - unfDisD.ActualQty;
                        disD.ActualQty = unfDisD.Stationery.AvailableQty > disD.RequestedQty ? disD.RequestedQty : unfDisD.Stationery.AvailableQty;
                        break;
                    }
                }
                if (isNew)
                {
                    DisbursementDetail newdisD = new DisbursementDetail()
                    {
                        ItemNum = unfDisD.ItemNum,
                        RequestedQty = unfDisD.RequestedQty - unfDisD.ActualQty,
                        UnitPrice = unfDisD.Stationery.AverageCost,
                        ActualQty = unfDisD.Stationery.AvailableQty > unfDisD.RequestedQty - unfDisD.ActualQty ? unfDisD.RequestedQty - unfDisD.ActualQty : unfDisD.Stationery.AvailableQty,
                    };
                    disDetails.Add(newdisD);
                }
                //更改unfDisD RequestedQty
                unfDisD.RequestedQty = unfDisD.ActualQty;
            }
        }

        /// <summary>
        /// /for supervisoer' dashboard
        /// </summary>
        /// <returns></returns>
        public double GetDisbursementTotalAmount()
        {
            double result = 0;
            List<Disbursement> list = new List<Disbursement>();
            List<DisbursementDetail> detailList = new List<DisbursementDetail>();
            list = GetAll().Where(x => x.Status != "unprocessed").ToList();
            foreach (Disbursement d in list)
            {
                result += GetAmountByDisbursement(d);
            }
            return result;
        }
        public double GetDisbursementByDepCode(String depcode)
        {
            double result = 0;
            List<Disbursement> list = new List<Disbursement>();
            List<DisbursementDetail> detailList = new List<DisbursementDetail>();
            list = GetAll().Where(x => x.Status != "unprocessed" && x.DeptCode.Equals(depcode)).ToList();
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
            List<DisbursementDetail> detailList = d.DisbursementDetails.ToList();
            foreach (DisbursementDetail f in detailList)
            {

                int qty = (int)f.ActualQty;
                double unit_price = (double)f.UnitPrice;
                result += (qty * unit_price);
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
        public List<double> GetAmountByDepAndCatList(String depCode,List<String> catId,String from,String to)
        {
            DateTime fromDate = DateTime.ParseExact(from, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            DateTime toDate = DateTime.ParseExact(to, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            double total = 0;
           
            List<double> result= new List<double>();
            foreach (String id in catId)
            {
                List<String>itemList=statRepo.GetItembyCategory(Convert.ToInt32(id));
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
       


     }

    
}