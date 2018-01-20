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
            return LUSSISContext.DisbursementDetails.Where(x => x.DisbursementId == disbursement.DisbursementId).ToList();
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
            foreach (Disbursement disbursement in disbursements)
            {
                disbursement.DisbursementDetails = new List<DisbursementDetail>();
            }
            
            //group requisition requests by dept and create disbursement list based on it
            List<Requisition> approvedReq = LUSSISContext.Requisitions.Where(r => r.Status == "approved").ToList();

            List<List<Requisition>> reqGroupByDep = approvedReq.GroupBy(r => r.RequisitionEmployee.DeptCode).Select(grp => grp.ToList()).ToList();
            foreach (List<Requisition> reqForOneDep in reqGroupByDep)
            {
                Disbursement d = ConvertReqListForOneDepToDisbursement(reqForOneDep, collectionDate);
                int disID = d.DisbursementId;
                disbursements.Add(d);

                //(1)将reqForOneDep中所有req的detail ToList (reqdetofRFOP)
                List<RequisitionDetail> reqDetailListForOneDep = new List<RequisitionDetail>();


                foreach (Requisition req in reqForOneDep)
                {
                    req.Status = "processed";
                    //插入req的detail到deqdetofRFOP
                    List<RequisitionDetail> tempReqDList = new RequisitionRepository().GetRequisitionDetail(req.RequisitionId).ToList();
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
                //取出ud的unfullfilled detail list
                List<DisbursementDetail> unfDisDList = LUSSISContext.DisbursementDetails.Where(x => x.DisbursementId == ud.DisbursementId && (x.RequestedQty - x.ActualQty) > 0).ToList();
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
                    
                    foreach(DisbursementDetail unfdd in unfDisDList)
                    {
                        DisbursementDetail tempDisD = new DisbursementDetail();
                        tempDisD.ItemNum = unfdd.ItemNum;
                        tempDisD.UnitPrice = unfdd.UnitPrice;
                        tempDisD.RequestedQty = unfdd.RequestedQty - unfdd.ActualQty;
                        newD.DisbursementDetails.Add(tempDisD);
                    }
                }
                //if exist, add to found department's

                //取出unfullfilled disDetail List
                //List<DisbursementDetail> unfDisDList = GetUnfullfilledDisDetailList().ToList();
                //整合disbursement detail list
                //AddUnfDisDtoDisD(unfDisDList, disDetails, disbursements);
                ud.Status = "fullfilled";
            }

            foreach (var d in disbursements)
            {
                Add(d);
                //foreach (var dd in d.DisbursementDetails)
                //{
                //    //Debug.WriteLine("Disbursement Detail: " + dd.ItemNum);
                //    LUSSISContext.DisbursementDetails.Add(dd);
                //}
            }
            //Debug.WriteLine("Disbursement generated, count: " + disbursements.Count);
            
            //foreach (var dd in disDetails)
            //{
            //    Debug.WriteLine("Disbursement Detail: " + dd.ItemNum);
            //    LUSSISContext.DisbursementDetails.Add(dd);
            //}

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
                        disD.RequestedQty = disD.RequestedQty + rd.Quantity;
                        break;
                    }
                }

                if (isNew)
                {
                    DisbursementDetail newdisD = new DisbursementDetail()
                    {
                        ItemNum = rd.ItemNum,
                        RequestedQty = rd.Quantity,
                        UnitPrice = rd.Stationery.AverageCost
                    };
                    disDetails.Add(newdisD);
                }
            }
        }

        //将没有fullfill的disdetail整理插入至新disbursement
        /*
        public void AddUnfDisDtoDisD(List<DisbursementDetail> unfDisDList, List<DisbursementDetail> disDetails, List<Disbursement> disbursements)
        {
            foreach(DisbursementDetail unfDisD in unfDisDList)
            {
                bool isNew = true;
                foreach (var disD in disDetails)
                {
                    if (unfDisD.Disbursement.DeptCode == disD.Disbursement.DeptCode && unfDisD.ItemNum == disD.ItemNum)
                    {
                        isNew = false;
                        disD.RequestedQty = disD.RequestedQty + unfDisD.RequestedQty - unfDisD.ActualQty;
                        break;
                    }
                }

                if (isNew)
                {
                    string s = unfDisD.Disbursement.DeptCode;
                    Disbursement tempDis = new Disbursement();
                    foreach (var d in disbursements)
                    {
                        if (d.DeptCode == s)
                        {
                            tempDis = d;
                            break;
                        }
                    }
                        DisbursementDetail newdisD = new DisbursementDetail()
                    {
                        DisbursementId = tempDis.DisbursementId,
                        ItemNum = unfDisD.ItemNum,
                        RequestedQty = unfDisD.RequestedQty - unfDisD.ActualQty,
                        UnitPrice = unfDisD.Stationery.AverageCost
                        };
                    disDetails.Add(newdisD);
                }
            }
        }
        */

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
                        disD.RequestedQty = disD.RequestedQty + unfDisD.RequestedQty - unfDisD.ActualQty;
                        break;
                    }
                }

                if (isNew)
                {
                    DisbursementDetail newdisD = new DisbursementDetail()
                    {
                        ItemNum = unfDisD.ItemNum,
                        RequestedQty = unfDisD.RequestedQty - unfDisD.ActualQty,
                        UnitPrice = unfDisD.Stationery.AverageCost
                    };
                    disDetails.Add(newdisD);
                }
            }
        }
    }
}