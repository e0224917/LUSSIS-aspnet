using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.WebPages;
using LUSSIS.Models;
using LUSSIS.Models.WebDTO;
using LUSSIS.Repositories.Interface;

namespace LUSSIS.Repositories
{
    //still working on it. --- xiaowen

    public class RequisitionRepository : Repository<Requisition, int>, IRequisitionRepository
    {
        EmployeeRepository er = new EmployeeRepository();
        public IEnumerable<RetrievalItemDTO> GetConsolidatedRequisition()
        {
            List<RetrievalItemDTO> itemsToRetrieve = new List<RetrievalItemDTO>();
            ConsolidateRequisitionQty(itemsToRetrieve, GetRequisitionDetailsByStatus("approved"));
            ConsolidateRemainingQty(itemsToRetrieve, new DisbursementRepository().GetUnfulfilledDisDetailList());
            return itemsToRetrieve;
        }

        public IEnumerable<RetrievalItemDTO> ArrangeRetrievalAndDisbursement(DateTime collectionDate)
        {
            IEnumerable<RetrievalItemDTO> itemsToRetrieve = GetConsolidatedRequisition();

            new DisbursementRepository().CreateDisbursement(collectionDate);
            return itemsToRetrieve;
        }

        /*
         * helper method to consolidate each [approved requisitions for one item] into [one RetrievalItemDTO]
        */
        private void ConsolidateRequisitionQty(List<RetrievalItemDTO> itemsToRetrieve, IEnumerable<RequisitionDetail> requisitionDetailList)
        {
            //group RequisitionDetail list by item: e.g.: List<ReqDetail>-for-pen, List<ReqDetail>-for-Paper, and store these lists in List:
            List<List<RequisitionDetail>> groupedReqListByItem = requisitionDetailList.GroupBy(rd=>rd.ItemNum).Select(grp=>grp.ToList()).ToList();

            //each list merge into ONE RetrievalItemDTO. e.g.: List<ReqDetail>-for-pen to be converted into ONE RetrievalItemDTO. 
            foreach (List<RequisitionDetail> reqListForOneItem in groupedReqListByItem)
            {
                Stationery statItem = reqListForOneItem.First().Stationery;

                //get the DTO for this stationery
                RetrievalItemDTO dto = GetDtoIfSameStatOrAddAndGetNewDto(itemsToRetrieve, statItem);

                foreach (RequisitionDetail rd in reqListForOneItem)
                {
                   dto.RequestedQty += rd.Quantity;
                }
            }
        }
       
        /*
         * helper method to consolidate each [unfullfilled Disbursements for one item] add to / into [one RetrievalItemDTO]
        */
        private void ConsolidateRemainingQty(List<RetrievalItemDTO> itemsToRetrieve, IEnumerable<DisbursementDetail> unfullfilledDisDetailList)
        {
            //group DisbursementDetail list by item: e.g.: List<DisDetail>-for-pen, List<DisDetail>-for-Paper, and store these lists in List:
            List<List<DisbursementDetail>> groupedDisListByItem = unfullfilledDisDetailList.GroupBy(rd => rd.ItemNum).Select(grp => grp.ToList()).ToList();

            //each list merge into ONE RetrievalItemDTO. e.g.: List<DisDetail>-for-pen to be converted into ONE RetrievalItemDTO. 
            foreach (List<DisbursementDetail> disListForOneItem in groupedDisListByItem)
            {
                Stationery statItem = disListForOneItem.First().Stationery;
                RetrievalItemDTO dto = GetDtoIfSameStatOrAddAndGetNewDto(itemsToRetrieve, statItem);

                foreach (DisbursementDetail dd in disListForOneItem)
                {

                    dto.RequestedQty += (dd.RequestedQty - dd.ActualQty);
                }
            }
        }

        
        /*
         * helper method
         * check if a stat's equivlent DTO exist in the target DTO list
         * exist? return that DTO : create new DTO and add it to list then return it
         */
        private RetrievalItemDTO GetDtoIfSameStatOrAddAndGetNewDto(List<RetrievalItemDTO> itemsToRetrieve, Stationery stat)
        {
            //if yes, take the DTO out and return it
            if (itemsToRetrieve.Count > 0)
            {
                foreach (RetrievalItemDTO dto in itemsToRetrieve)
                {
                    if (dto.ItemNum == stat.ItemNum)
                    {
                        return dto;
                    }
                }
            }
            //if not, instantiate a new DTO, add to the list and return it instead
            RetrievalItemDTO newDto = convertStatoDTO(stat);
            itemsToRetrieve.Add(newDto);
            return newDto;
        }


        public IEnumerable<Requisition> GetRequisitionsByStatus(string status)
        {
            return LUSSISContext.Requisitions.Where(r => r.Status == status).ToList();
        }

        public List<Requisition> GetPendingRequisitions()
        {
            string deptCode = er.GetCurrentUser().DeptCode;
            int userEmpNum = er.GetCurrentUser().EmpNum;
            List<Employee> elist = LUSSISContext.Employees.Where(e => e.DeptCode == deptCode && e.EmpNum != userEmpNum).ToList();
            List<Requisition> req = new List<Requisition>();
            foreach (Employee ee in elist)
            {
                int EmpNum = ee.EmpNum;
                List<Requisition> req1 = LUSSISContext.Requisitions.Where(r => r.Status == "pending" && r.RequisitionEmpNum == EmpNum).ToList();
                if (req1 != null)
                {
                    foreach(Requisition ree in req1)
                    {
                        req.Add(ree);
                    }
                }
            }
            return req;
        }

        public List<Requisition> GetAllRequisitionsForCurrentUser()
        {
            string deptCode = er.GetCurrentUser().DeptCode;
            List<Employee> elist = LUSSISContext.Employees.Where(e => e.DeptCode == deptCode).ToList();
            List<Requisition> req = new List<Requisition>();
            foreach (Employee ee in elist)
            {
                int EmpNum = ee.EmpNum;
                List<Requisition> req1 = LUSSISContext.Requisitions.Where(r =>r.RequisitionEmpNum == EmpNum).ToList();
                if (req1 != null)
                {
                    foreach (Requisition ree in req1)
                    {
                        req.Add(ree);
                    }
                }
            }
            return req;
        }

        public List<Requisition> GetAllRequisitionsSearch(string term)
        {
            string deptCode = er.GetCurrentUser().DeptCode;
            List<Employee> elist = LUSSISContext.Employees.Where(e => e.DeptCode == deptCode).ToList();
            List<Requisition> req = new List<Requisition>();
            term = term.ToLower();
            foreach (Employee ee in elist)
            {
                int EmpNum = ee.EmpNum;
                List<Requisition> req1 = LUSSISContext.Requisitions.Where(r => r.RequisitionEmpNum == EmpNum && (r.Status.ToLower().Contains(term) || r.RequisitionEmployee.FirstName.ToLower().Contains(term)|| r.RequisitionEmployee.LastName.ToLower().Contains(term) || r.RequisitionDate.ToString().Contains(term) || r.RequestRemarks.ToLower().Contains(term))).ToList();
                if (req1 != null)
                {
                    foreach (Requisition ree in req1)
                    {
                        req.Add(ree);
                    }
                }
            }
            return req;
        }



        public IEnumerable<RequisitionDetail> GetRequisitionDetailsByStatus(string status)
        {
            return LUSSISContext.RequisitionDetails.Where(r => r.Requisition.Status == status ).ToList();
        }

        /*
         * helper method
         */
        private RetrievalItemDTO convertStatoDTO(Stationery s)
        {
            return new RetrievalItemDTO()
            {
                ItemNum = s.ItemNum,
                BinNum = s.BinNum,
                Description = s.Description,
                UnitOfMeasure = s.UnitOfMeasure,
                AvailableQty = s.AvailableQty,
                RequestedQty = 0,
                RemainingQty = 0
            };
        }
        public IEnumerable<Requisition> GetRequisitionByEmpNum(int EmpNum)
        {
            return LUSSISContext.Requisitions.Where(s => s.RequisitionEmployee.EmpNum == EmpNum);
        }
        public IEnumerable<RequisitionDetail> GetRequisitionDetail(int RequisitionId)
        {
            return LUSSISContext.RequisitionDetails.Where(s => s.RequisitionId == RequisitionId);
        }
        public void AddRequisitionDetail(RequisitionDetail requisitionDetail)
        {
             LUSSISContext.RequisitionDetails.Add(requisitionDetail);
            LUSSISContext.SaveChanges();
        }

        public IEnumerable<Requisition> GetPendingListForHead(string dept)
        {
            return LUSSISContext.Requisitions.Where(r => r.RequisitionEmployee.DeptCode == dept && r.Status == "pending");
        }

        public IEnumerable<RetrievalItemDTO> GetRetrievalInPorcess()
        {
            List<RetrievalItemDTO> itemsToRetrieve = new List<RetrievalItemDTO>();
            
            //use disbursement as resource to generate retrieval in process
            
            var inProcessDisDetailsGroupedByItem = new DisbursementRepository().GetInProcessDisbursementDetails().GroupBy(x=>x.ItemNum).Select(grp=>grp.ToList()).ToList();

            foreach (List<DisbursementDetail> disDetailForOneItem in inProcessDisDetailsGroupedByItem)
            {
                Stationery stat = disDetailForOneItem.First().Stationery;
                RetrievalItemDTO dto = ConvertStationeryToDto(stat);
                foreach (DisbursementDetail dd in disDetailForOneItem)
                {
                    dto.RequestedQty += dd.RequestedQty;
                }
                itemsToRetrieve.Add(dto);
            }

            return itemsToRetrieve;
        }
        /*
         * helper method
         */
        private RetrievalItemDTO ConvertStationeryToDto(Stationery stat)
        {
            return new RetrievalItemDTO()
            {
                BinNum = stat.BinNum,
                ItemNum = stat.ItemNum,
                Description = stat.Description,
                AvailableQty = stat.AvailableQty,
                UnitOfMeasure = stat.UnitOfMeasure,
                RequestedQty = 0,
                RemainingQty = 0,
            };
        }
    }

    
}