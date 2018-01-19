using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using LUSSIS.Models;
using LUSSIS.Repositories;

namespace LUSSIS.Controllers
{
    public class SummaryViewModel
    {
        public Dictionary<Supplier, List<Stationery>> outstandingStationeryList { get; set; }
        public List<PurchaseOrder> pendingApprovalPOList { get; set; }
        public List<PurchaseOrder> approvedPOList { get; set; }
    }

    public class POReceiveViewModel
    {
        public ReceiveTransViewModel ReceiveTrans { get; set; }
        public PurchaseOrder PO { get; set; }
    }
    public class ReceiveTransViewModel
    {
        public ReceiveTransViewModel()
        {
            ReceiveTransDetails = new List<ReceiveTransDetail>();
        }

        public int ReceiveId { get; set; }
        public int? PoNum { get; set; }
        public DateTime? ReceiveDate { get; set; }
        public string InvoiceNum { get; set; }
        public string DeliveryOrderNum { get; set; }
        public List<ReceiveTransDetail> ReceiveTransDetails { get; set; }
        public ReceiveTran ConvertToReceiveTran()
        {
            ReceiveTran receive = new ReceiveTran();
            receive.ReceiveId = this.ReceiveId;
            receive.PoNum = this.PoNum;
            receive.ReceiveDate = this.ReceiveDate;
            receive.InvoiceNum = this.InvoiceNum;
            receive.DeliveryOrderNum = this.DeliveryOrderNum;
            receive.ReceiveTransDetails=this.ReceiveTransDetails;
            return receive;
        }
    }

    public class PurchaseOrderViewModel
    {
        public PurchaseOrderViewModel()
        {
            PurchaseOrderDetails = new List<PurchaseOrderDetail>();
            ReceiveTrans = new List<ReceiveTran>();
        }
        public int PoNum { get; set; }
        public int? SupplierId { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? OrderDate { get; set; }
        public string Status { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public int? OrderEmpNum { get; set; }
        public int? ApprovalEmpNum { get; set; }
        public double ShippingFee { get; set; }
        public double GST { get; set; }
        public double TotalAmt { get; set; }
        public Employee OrderEmployee { get; set; }
        public Employee ApprovalEmployee { get; set; }
        public Supplier Supplier { get; set; }
        public List<PurchaseOrderDetail> PurchaseOrderDetails { get; set; }
        public List<ReceiveTran> ReceiveTrans { get; set; }
        public PurchaseOrder ConvertToPurchaseOrder()
        {
            PurchaseOrder po = new PurchaseOrder();
            po.PoNum = this.PoNum;
            po.SupplierId = this.SupplierId;
            po.CreateDate = this.CreateDate;
            po.OrderDate = this.OrderDate;
            po.Status = this.Status;
            po.ApprovalDate = this.ApprovalDate;
            po.OrderEmpNum = this.OrderEmpNum;
            po.ApprovalEmpNum = this.ApprovalEmpNum;
            po.OrderEmployee = this.OrderEmployee;
            po.ApprovalEmployee = this.ApprovalEmployee;
            po.Supplier = this.Supplier;
            po.PurchaseOrderDetails = this.PurchaseOrderDetails;
            po.ReceiveTrans = this.ReceiveTrans;
            return po;
        }
        public PurchaseOrderViewModel(PurchaseOrder po)
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
            this.ReceiveTrans = po.ReceiveTrans.ToList();
        }

    }

    public class PurchaseOrdersController : Controller
    {
        private PORepository pr = new PORepository();
        private StationeryRepository sr = new StationeryRepository();
        private EmployeeRepository er = new EmployeeRepository();
        private SupplierRepository sur = new SupplierRepository();

        // GET: PurchaseOrders
        public ActionResult Index()
        {
            var purchaseOrders = pr.GetAll();
            return View(purchaseOrders.ToList().OrderByDescending(x=>x.CreateDate));
        }

        // GET: PurchaseOrders/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PurchaseOrder purchaseOrder = pr.GetById(Convert.ToInt32(id));
            if (purchaseOrder == null)
            {
                return HttpNotFound();
            }
            return View(new PurchaseOrderViewModel(purchaseOrder));
        }

        // GET: PurchaseOrders/Create?supplierId=5
        public ActionResult Create(int? supplierId)
        {
            Supplier supplier;
            PurchaseOrder po = new PurchaseOrder();
            List<Stationery> stationeries;  //for dropdown list
            int countOfLines = 1;

            if (supplierId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            supplier = sur.GetById(Convert.ToInt32(supplierId));
            po.Supplier = supplier;
            po.SupplierId = supplier.SupplierId;
            po.CreateDate = DateTime.Today;

            //create list of purchase details for outstanding items
            stationeries = sr.GetStationeryBySupplierId(supplierId).ToList();
            foreach (Stationery stationery in stationeries)
            {
                if (stationery.CurrentQty < stationery.ReorderLevel)
                {
                    PurchaseOrderDetail pdetails = new PurchaseOrderDetail();
                    pdetails.Stationery = stationery;
                    pdetails.OrderQty = stationery.ReorderLevel - stationery.CurrentQty;
                    pdetails.UnitPrice = stationery.UnitPrice(Convert.ToInt32(supplierId));
                    pdetails.PurchaseOrder = po;
                    po.PurchaseOrderDetails.Add(pdetails);
                }
            }
            countOfLines = po.PurchaseOrderDetails.Count;

            //create empty puchase details to populate newly created items
            for(int i = countOfLines; i < 100; i++)
            {
                PurchaseOrderDetail pdetails = new PurchaseOrderDetail();
                pdetails.PurchaseOrder = po;
                po.PurchaseOrderDetails.Add(pdetails);
            }

            //fill ViewBag to populate stationery dropdown lists
            ViewBag.Stationery = sr.GetStationerySupplierBySupplierId(supplierId).ToList();
            ViewBag.Suppliers = sur.GetAll();
            ViewBag.Supplier = supplier;
            ViewBag.countOfLines = countOfLines;
            


            return View(new PurchaseOrderViewModel(po));
        }



        // POST: PurchaseOrders/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PurchaseOrderViewModel purchaseOrderViewModel) //[Bind(Include = "PoNum,SupplierId,CreateDate,OrderDate,Status,ApprovalDate,OrderEmpNum,ApprovalEmpNum")] 
        {
            PurchaseOrder purchaseOrder = purchaseOrderViewModel.ConvertToPurchaseOrder();
            if (ModelState.IsValid)
            {
                //validation
                if (purchaseOrder.CreateDate == null) purchaseOrder.CreateDate = DateTime.Today;
                purchaseOrder.OrderEmpNum = er.GetCurrentUser().EmpNum;
                purchaseOrder.Status = "pending";

                //persist data
                pr.Add(purchaseOrder);
                for (int i = purchaseOrder.PurchaseOrderDetails.Count - 1; i >= 0; i--)
                {
                    if (purchaseOrder.PurchaseOrderDetails.Skip(i).First().OrderQty < 0)
                    {
                        purchaseOrder.PurchaseOrderDetails.Remove(purchaseOrder.PurchaseOrderDetails.Skip(i).First());
                    }
                }
                return RedirectToAction("Summary");
            }

            return View(purchaseOrder);
        }

        //GET: PurchaseOrders/Receive?p=100001
        [HttpGet]
        public ActionResult Receive(int? p = null)
        {
            PurchaseOrder po = null;
            POReceiveViewModel pOReceive = new POReceiveViewModel();
            ReceiveTransViewModel receive = new ReceiveTransViewModel();


            if (p == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            //populate PO and ReceiveTrans if PO number is given

            po = pr.GetById(Convert.ToInt32(p));
            pOReceive.PO = po;
            for (int i = 0; i < po.PurchaseOrderDetails.Count; i++)
            {
                ReceiveTransDetail rdetail = new ReceiveTransDetail();
                rdetail.ItemNum = po.PurchaseOrderDetails.Skip(i).First().ItemNum;
                rdetail.Quantity = 0;
                receive.ReceiveTransDetails.Add(rdetail);
            }


            ViewBag.PurchaseOrder = po;
            pOReceive.ReceiveTrans = receive;
            return View(pOReceive);
        }

        //POST: PurchaseOrders/Receive
        [HttpPost]
        public ActionResult Receive(POReceiveViewModel poReceive)
        {
            PurchaseOrder po = pr.GetById(Convert.ToInt32(poReceive.PO.PoNum));
            ReceiveTran receive = poReceive.ReceiveTrans.ConvertToReceiveTran();
            bool fulfilled = true;

            //update received quantity
            for (int i = po.PurchaseOrderDetails.Count - 1; i >= 0; i--)
            {
                int arrQty = Convert.ToInt32(receive.ReceiveTransDetails.Skip(i).First().Quantity);
                if (arrQty > 0)
                {
                    po.PurchaseOrderDetails.Skip(i).First().ReceiveQty += arrQty;
                    if (po.PurchaseOrderDetails.Skip(i).First().ReceiveQty < po.PurchaseOrderDetails.Skip(i).First().OrderQty)
                        fulfilled = false;
                }
                else if (arrQty == 0)
                {
                    receive.ReceiveTransDetails.Remove(receive.ReceiveTransDetails.Skip(i).First());
                }
            }
            if (fulfilled) po.Status = "fulfilled";
            pr.Update(poReceive.PO);

            //update stationery master
            foreach (ReceiveTransDetail rd in receive.ReceiveTransDetails)
            {
                Stationery s = rd.Stationery;
                s.AverageCost = (s.AverageCost * s.CurrentQty) + (rd.Quantity);
                s.CurrentQty += rd.Quantity;
                sr.Update(s);
            }


            return RedirectToAction("Summary");
        }

        //GET: PurchaseOrders/Summary
        public ActionResult Summary()
        {
            SummaryViewModel model = new SummaryViewModel();
            model.outstandingStationeryList = sr.GetOutstandingStationeryByAllSupplier();
            model.pendingApprovalPOList = pr.GetPendingApprovalPO();
            model.approvedPOList = pr.GetApprovedPO();
            return View(model);
        }

        /*
        * Auto generated
        * 
    // GET: PurchaseOrders/Edit/5
    public ActionResult Edit(int? id)
    {
    if (id == null)
    {
        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
    }
    PurchaseOrder purchaseOrder = db.PurchaseOrders.Find(id);
    if (purchaseOrder == null)
    {
        return HttpNotFound();
    }
    ViewBag.OrderEmpNum = new SelectList(db.Employees, "EmpNum", "Title", purchaseOrder.OrderEmpNum);
    ViewBag.ApprovalEmpNum = new SelectList(db.Employees, "EmpNum", "Title", purchaseOrder.ApprovalEmpNum);
    ViewBag.SupplierId = new SelectList(db.Suppliers, "SupplierId", "SupplierName", purchaseOrder.SupplierId);
    return View(purchaseOrder);
    }

    // POST: PurchaseOrders/Edit/5
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Edit([Bind(Include = "PoNum,SupplierId,CreateDate,OrderDate,Status,ApprovalDate,OrderEmpNum,ApprovalEmpNum")] PurchaseOrder purchaseOrder)
    {
    if (ModelState.IsValid)
    {
        db.Entry(purchaseOrder).State = EntityState.Modified;
        db.SaveChanges();
        return RedirectToAction("Index");
    }
    ViewBag.OrderEmpNum = new SelectList(db.Employees, "EmpNum", "Title", purchaseOrder.OrderEmpNum);
    ViewBag.ApprovalEmpNum = new SelectList(db.Employees, "EmpNum", "Title", purchaseOrder.ApprovalEmpNum);
    ViewBag.SupplierId = new SelectList(db.Suppliers, "SupplierId", "SupplierName", purchaseOrder.SupplierId);
    return View(purchaseOrder);
    }
    */
    }
}



public static class StationeryExtension
{
    public static double? UnitPrice(this Stationery s, int supplierId)
    {
        foreach (StationerySupplier ss in s.StationerySuppliers)
        {
            if (ss.SupplierId == supplierId) return ss.Price;
        }
        return null;
    }

    public static double LinePrice(this Stationery s, int supplierId, int? qty)
    {
        foreach (StationerySupplier ss in s.StationerySuppliers)
        {
            if (ss.SupplierId == supplierId) return Convert.ToDouble(ss.Price * qty);
        }
        return 0;
    }

    public static Supplier PrimarySupplier(this Stationery s)
    {
        foreach (StationerySupplier ss in s.StationerySuppliers)
        {
            if (ss.Rank == 1) return ss.Supplier;
        }
        return null;
    }
}

