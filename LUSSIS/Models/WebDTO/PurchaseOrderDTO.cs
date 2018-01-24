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
            this.SupplierContact = po.Supplier.ContactName;
            this.Address1 = po.Supplier.Address1;
            this.Address2 = po.Supplier.Address2;
            this.Address3 = po.Supplier.Address3;
            this.PurchaseOrderDetails = po.PurchaseOrderDetails.ToList();
        }
    }
}