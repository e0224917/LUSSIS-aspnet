using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.WebPages;
using LUSSIS.Constants;
using LUSSIS.Models;
using LUSSIS.Models.WebDTO;
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

        public IEnumerable<Requisition> FindRequisitionsByDeptCodeAndText(string term, string deptCode)
        {
            return LUSSISContext.Requisitions
                .Where(r => r.DeptCode == deptCode
                            && (r.Status.ToLower().Contains(term)
                                || r.RequisitionEmployee.FirstName.ToLower().Contains(term)
                                || r.RequisitionEmployee.LastName.ToLower().Contains(term)
                                // || r.RequisitionDate.ToString().Contains(term) 
                                || r.RequestRemarks.ToLower().Contains(term)));                             

        }

        public IEnumerable<RequisitionDetail> GetRequisitionDetailsByStatus(string status)
        {
            return LUSSISContext.RequisitionDetails
                .Where(r => r.Requisition.Status == status).ToList();
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
            LUSSISContext.Set<RequisitionDetail>().Include("Stationery");
            var stationery = LUSSISContext.Set<RequisitionDetail>().Add(requisitionDetail).Stationery;
            LUSSISContext.SaveChanges();
            return stationery;
        }

        public IEnumerable<Requisition> GetPendingListForHead(string deptCode)
        {
            var list = LUSSISContext.Requisitions
                .Where(r => r.DeptCode == deptCode && r.Status == POStatus.Pending).ToList();
            list.Reverse();
            return list;
        }

    }


}