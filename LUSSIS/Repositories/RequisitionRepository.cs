using System.Collections.Generic;
using System.Linq;
using LUSSIS.Constants;
using LUSSIS.Models;
using LUSSIS.Repositories.Interface;

namespace LUSSIS.Repositories
{
    //Authors: Tang Xiaowen, Cui Runze
    public class RequisitionRepository : Repository<Requisition, int>, IRequisitionRepository
    {
        public List<Requisition> GetAllByDeptCode(string deptCode)
        {
            var list = LUSSISContext.Requisitions.Where(r => r.DeptCode == deptCode).ToList();
            list.Reverse();
            return list;
        }

        /// <summary>
        /// Return results for search box
        /// </summary>
        /// <param name="term"></param>
        /// <param name="deptCode"></param>
        /// <returns></returns>
        public IEnumerable<Requisition> FindRequisitionsByDeptCodeAndText(string term, string deptCode)
        {
            var list = LUSSISContext.Requisitions
                .Where(r => r.DeptCode == deptCode
                            && (r.Status.ToLower().Contains(term)
                                || r.RequisitionEmployee.FirstName.ToLower().Contains(term)
                                || r.RequisitionEmployee.LastName.ToLower().Contains(term)
                                // || r.RequisitionDate.ToString().Contains(term) 
                                || r.RequestRemarks.ToLower().Contains(term))).ToList();
            list.Reverse();
            return list;                    
        }

        public IEnumerable<RequisitionDetail> GetRequisitionDetailsByStatus(string status)
        {
            return LUSSISContext.RequisitionDetails
                .Where(r => r.Requisition.Status == status);
        }

        public IEnumerable<RequisitionDetail> GetApprovedRequisitionDetails()
        {
            return LUSSISContext.RequisitionDetails
                .Where(r => r.Requisition.Status == RequisitionStatus.Approved);
        }

        public List<RequisitionDetail> GetApprovedRequisitionDetailsByDeptCode(string deptCode)
        {
            return LUSSISContext.RequisitionDetails
                .Where(r => r.Requisition.DeptCode == deptCode
                            && r.Requisition.Status == RequisitionStatus.Approved).ToList();
        }

        public IEnumerable<Requisition> GetApprovedRequisitions()
        {
            return LUSSISContext.Requisitions.Where(r => r.Status == RequisitionStatus.Approved);
        }

        public IEnumerable<Requisition> GetRequisitionsByEmpNum(int empNum)
        {
            return LUSSISContext.Requisitions.Where(s => s.RequisitionEmployee.EmpNum == empNum);
        }

        public IEnumerable<RequisitionDetail> GetRequisitionDetailsById(int requisitionId)
        {
            return LUSSISContext.RequisitionDetails.Where(s => s.RequisitionId == requisitionId);
        }

        public Stationery AddRequisitionDetail(RequisitionDetail requisitionDetail)
        {
            LUSSISContext.Set<RequisitionDetail>().Add(requisitionDetail);
            LUSSISContext.SaveChanges();
            return LUSSISContext.Stationeries.Find(requisitionDetail.ItemNum);
        }

        /// <summary>
        /// Return the list of all pending requisitions in a department
        /// </summary>
        /// <param name="deptCode"></param>
        /// <returns></returns>
        public IEnumerable<Requisition> GetPendingListForHead(string deptCode)
        {
            var list = LUSSISContext.Requisitions
                .Where(r => r.DeptCode == deptCode && r.Status == POStatus.Pending).ToList();
            list.Reverse();
            return list;
        }

    }
}