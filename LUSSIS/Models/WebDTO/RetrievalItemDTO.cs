using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LUSSIS.Models.WebDTO
{
    /*
    *Same item from requisition detai table become ONE object of this class. 
    *this DTO is used to facilitate retrieval
    */
    public class RetrievalListDTO
    {
        public List<RetrievalItemDTO> List { get; set; }

        public RetrievalListDTO()
        {
            List = new List<RetrievalItemDTO>();
        }

        public void Add(RetrievalItemDTO item)
        {
            if (List.Count > 0)
            {
                for (int i = 0; i < List.Count; i++)
                {
                    if (item.ItemNum == List[i].ItemNum)
                    {
                        List[i].RequestedQty += item.RequestedQty;
                    }
                    else
                    {
                        List.Add(item);
                    }
                }
            }
            else
            {
                List.Add(item);
            }
        }

        public void AddRange(RetrievalListDTO listDto)
        {
            foreach (var item in listDto.List)
            {
                Add(item);
            }
        }
        
    }

    public class RetrievalItemDTO
    {
        public string ItemNum { get; set; }
        public string BinNum { get; set; }
        public string Description { get; set; }
        public string UnitOfMeasure { get; set; }
        //stock qty
        public int AvailableQty { get; set; }
        //assocaited approved requisition qty
        public int RequestedQty { get; set; }
        //qty short from unfullfilled disbursement
        public int RemainingQty { get; set; }

        public RetrievalItemDTO(Stationery stationery)
        {
            ItemNum = stationery.ItemNum;
            BinNum = stationery.BinNum;
            Description = stationery.Description;
            UnitOfMeasure = stationery.UnitOfMeasure;
            AvailableQty = stationery.AvailableQty;
            RequestedQty = 0;
            RemainingQty = stationery.AvailableQty;
        }

        public RetrievalItemDTO(List<RequisitionDetail> requisitionDetails)
        {
            var stationery = requisitionDetails.First().Stationery;

            ItemNum = stationery.ItemNum;
            BinNum = stationery.BinNum;
            Description = stationery.Description;
            UnitOfMeasure = stationery.UnitOfMeasure;
            AvailableQty = stationery.AvailableQty;
            RequestedQty = 0;
            RemainingQty = stationery.AvailableQty;

            //Calculate the quantity
            foreach (var requisitionDetail in requisitionDetails)
            {
                RequestedQty += requisitionDetail.Quantity;
            }
        }

        public RetrievalItemDTO(List<DisbursementDetail> disbursementDetails)
        {
            var stationery = disbursementDetails.First().Stationery;

            ItemNum = stationery.ItemNum;
            BinNum = stationery.BinNum;
            Description = stationery.Description;
            UnitOfMeasure = stationery.UnitOfMeasure;
            AvailableQty = stationery.AvailableQty;
            RequestedQty = 0;
            RemainingQty = stationery.AvailableQty;

            //Calculate the quantity
            foreach (var disbursementDetail in disbursementDetails)
            {
                RequestedQty += disbursementDetail.RequestedQty - disbursementDetail.ActualQty;
            }
        }
    }
}