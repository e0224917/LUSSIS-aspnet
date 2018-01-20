using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace LUSSIS.Models.WebDTO
{
    public class PurchaseOrderDTO
    {
        public PurchaseOrderDTO(){
            PurchaseOrderDetails = new List<PurchaseOrderDetail>(); //use this normally, can access stationery and order details
            PurchaseOrderDetailsDTO = new List<PurchaseOrderDetailDTO>();//tried to use this for validation but it fails
        }
    public int PoNum { get; set; }
    public int? SupplierId { get; set; }
    public DateTime? CreateDate { get; set; }
    public DateTime? OrderDate { get; set; }
    public string Status { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public int? OrderEmpNum { get; set; }
    public int? ApprovalEmpNum { get; set; }
    public double GstAmt { get; set; }
    public double TotalPoAmt { get; set; }
    public Employee OrderEmployee { get; set; }
    public Employee ApprovalEmployee { get; set; }
    public Supplier Supplier { get; set; }
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
        this.PurchaseOrderDetails = po.PurchaseOrderDetails.ToList();
    }
}
}