using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using LUSSIS.Constants;

namespace LUSSIS.Models.WebDTO
{
    //Authors: Douglas Lee Kiat Hui
    public class PurchaseOrderDTO
    {
        public PurchaseOrderDTO()
        {
            PurchaseOrderDetails = new List<PurchaseOrderDetail>(); //use this normally, can access stationery and order details
            PurchaseOrderDetailsDTO = new List<PurchaseOrderDetailDTO>();//tried to use this for validation but it fails
        }
        public int PoNum { get; set; }
        public int SupplierId { get; set; }
        [Display(Name="Created Date")]
        public DateTime CreateDate { get; set; }
        [Display(Name = "Ordered Date")]
        public DateTime? OrderDate { get; set; }
        public string Status { get; set; }
        [Display(Name = "Approved Date")]
        public DateTime? ApprovalDate { get; set; }
        public int OrderEmpNum { get; set; }
        public int? ApprovalEmpNum { get; set; }
        public double GstAmt { get; set; }
        public double TotalPoAmt { get; set; }
        public Employee OrderEmployee { get; set; }
        public Employee ApprovalEmployee { get; set; }
        public Supplier Supplier { get; set; }
        [Required]
        public string SupplierContact { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        [Required]
        public string SupplierAddress
        {
            get => Address1 + Environment.NewLine + Address2 + Environment.NewLine + Address3;
            set
            {
                var addressArr = value.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                Address1 = addressArr[0];
                if (addressArr.Length > 1)
                    Address2 = addressArr[1];
                Address3 = "";
                for (int i = 2; i < addressArr.Length; i++)
                    Address3 += addressArr[i];
            }
        }
        public List<PurchaseOrderDetail> PurchaseOrderDetails { get; set; }
        public List<PurchaseOrderDetailDTO> PurchaseOrderDetailsDTO { get; set; }
        public PurchaseOrderDTO(PurchaseOrder po)
        {
            PoNum = po.PoNum;
            SupplierId = po.SupplierId;
            CreateDate = po.CreateDate;
            OrderDate = po.OrderDate;
            Status = po.Status;
            ApprovalDate = po.ApprovalDate;
            OrderEmpNum = po.OrderEmpNum;
            ApprovalEmpNum = po.ApprovalEmpNum;
            OrderEmployee = po.OrderEmployee;
            ApprovalEmployee = po.ApprovalEmployee;
            Supplier = po.Supplier;
            SupplierContact = po.SupplierContact;
            Address1 = po.Address1;
            Address2 = po.Address2;
            Address3 = po.Address3;
            PurchaseOrderDetails = po.PurchaseOrderDetails.ToList();
        }
        public void CreatePurchaseOrder(out PurchaseOrder purchaseOrder)
        {
            purchaseOrder = new PurchaseOrder
            {
                Status = POStatus.Pending,
                SupplierId = SupplierId,
                SupplierContact = SupplierContact,
                Address1 = Address1,
                Address2 = Address2,
                Address3 = Address3,
                CreateDate = CreateDate,
                OrderEmpNum = OrderEmpNum
            };

            //set PO detail values
            for (int i = PurchaseOrderDetailsDTO.Count - 1; i >= 0; i--)
            {
                var pdetail = PurchaseOrderDetailsDTO.ElementAt(i);
                if (pdetail.OrderQty > 0)
                {
                    PurchaseOrderDetail newPdetail = new PurchaseOrderDetail
                    {
                        ItemNum = pdetail.ItemNum,
                        OrderQty = pdetail.OrderQty,
                        UnitPrice = pdetail.UnitPrice,
                        ReceiveQty = 0
                    };
                    purchaseOrder.PurchaseOrderDetails.Add(newPdetail);
                }
                else if (pdetail.OrderQty < 0)
                    throw new Exception("Purchase Order was not created, ordered quantity cannot be negative");
            }
            if (purchaseOrder.PurchaseOrderDetails.Count == 0)
                throw new Exception("Purchase Order was not created, no items found");
            if (purchaseOrder.PurchaseOrderDetails.Count > purchaseOrder.PurchaseOrderDetails.Select(x => x.ItemNum).Distinct().Count())
                throw new Exception("the same stationery cannot appear in multiple lines of the PO");
        }
    }
}