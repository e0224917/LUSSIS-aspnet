using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace LUSSIS.Models.WebDTO
{
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
        public string SupplierContact { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string SupplierAddress
        {
            get { return Address1 + Environment.NewLine + Address2 + Environment.NewLine + Address3; }
            set
            {
                string[] addressArr = value.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
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
            this.PoNum = po.PoNum;
            this.SupplierId = po.SupplierId;
            this.CreateDate = po.CreateDate;
            this.OrderDate = po.OrderDate;
            this.Status = po.Status;
            this.ApprovalDate = po.ApprovalDate;
            this.OrderEmpNum = po.OrderEmpNum;
            this.ApprovalEmpNum = po.ApprovalEmpNum;
            this.OrderEmployee = po.OrderEmployee;
            this.ApprovalEmployee = po.ApprovalEmployee;
            this.Supplier = po.Supplier;
            this.SupplierContact = po.SupplierContact;
            this.Address1 = po.Address1;
            this.Address2 = po.Address2;
            this.Address3 = po.Address3;
            this.PurchaseOrderDetails = po.PurchaseOrderDetails.ToList();
        }
        public void CreatePurchaseOrder(out PurchaseOrder purchaseOrder)
        {
            purchaseOrder = new PurchaseOrder();
            purchaseOrder.Status = "pending";
            purchaseOrder.SupplierId = this.SupplierId;
            purchaseOrder.SupplierContact = this.SupplierContact;
            purchaseOrder.Address1 = this.Address1;
            purchaseOrder.Address2 = this.Address2;
            purchaseOrder.Address3 = this.Address3;
            purchaseOrder.CreateDate = this.CreateDate;
            purchaseOrder.OrderEmpNum = this.OrderEmpNum;

            //set PO detail values
            for (int i = this.PurchaseOrderDetailsDTO.Count - 1; i >= 0; i--)
            {
                PurchaseOrderDetailDTO pdetail = this.PurchaseOrderDetailsDTO.ElementAt(i);
                if (pdetail.OrderQty > 0)
                {
                    PurchaseOrderDetail newPdetail = new PurchaseOrderDetail();
                    newPdetail.ItemNum = pdetail.ItemNum;
                    newPdetail.OrderQty = pdetail.OrderQty;
                    newPdetail.UnitPrice = pdetail.UnitPrice;
                    newPdetail.ReceiveQty = 0;
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