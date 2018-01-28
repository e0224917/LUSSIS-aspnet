﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using CrystalDecisions.CrystalReports.Engine;
using LUSSIS.Models;
using LUSSIS.Models.WebDTO;
using LUSSIS.Repositories;
using PagedList;

namespace LUSSIS.Controllers
{
    [Authorize(Roles = "clerk, supervisor")]
    public class PurchaseOrdersController : Controller
    {
        private readonly PORepository _poRepo = new PORepository();
        private readonly StationeryRepository _stationeryRepo = new StationeryRepository();
        private readonly SupplierRepository _supplierRepo = new SupplierRepository();
        private const double GstRate = 0.07;

        // GET: PurchaseOrders
        public ActionResult Index(int? page = 1)
        {
            var purchaseOrders = _poRepo.GetAll();
            ViewBag.page = page;
            return View(purchaseOrders.ToList().OrderByDescending(x => x.CreateDate)
                .ToPagedList(pageNumber: Convert.ToInt32(page), pageSize: 15));
        }

        // GET: PurchaseOrders/Details/10005
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var purchaseOrder = _poRepo.GetById(Convert.ToInt32(id));
            if (purchaseOrder == null)
            {
                return HttpNotFound();
            }

            var po = new PurchaseOrderDTO(purchaseOrder);

            //ViewBag.PurchaseOrder = po;
            return View(po);
        }


        //GET: PurchaseOrders/Summary
        [Authorize(Roles = "clerk")]
        public ActionResult Summary()
        {
            ViewBag.OutstandingStationeryList = _stationeryRepo.GetOutstandingStationeryByAllSupplier();
            ViewBag.PendingApprovalPOList = _poRepo.GetPOByStatus("pending");
            ViewBag.OrderedPOList = _poRepo.GetPOByStatus("ordered");
            ViewBag.ApprovedPOList = _poRepo.GetPOByStatus("approved");
            return View();
        }


        // GET: PurchaseOrders/Create or PurchaseOrders/Create?supplierId=1
        [Authorize(Roles = "clerk")]
        public ActionResult Create(int? supplierId, string error = null)
        {
            //catch error from redirect
            ViewBag.Error = error;

            var po = new PurchaseOrderDTO(); //view model

            if (supplierId == null) //select supplier if non-chosen yet
            {
                var emptySupplier = new Supplier
                {
                    SupplierId = -1,
                    SupplierName = "Select a Supplier"
                };

                ViewBag.Suppliers = _supplierRepo.GetAll().Concat(new [] {emptySupplier});
                ViewBag.Supplier = emptySupplier;

                var emptyPo = new PurchaseOrderDTO {SupplierId = -1};
                return View(emptyPo);
            }

            //get supplier
            var supplier = _supplierRepo.GetById(Convert.ToInt32(supplierId));
            po.Supplier = supplier;
            po.SupplierId = supplier.SupplierId;
            po.CreateDate = DateTime.Today;
            po.SupplierAddress = supplier.Address1 + Environment.NewLine + supplier.Address2 + Environment.NewLine +
                                 supplier.Address3;
            po.SupplierContact = supplier.ContactName;

            //set empty Stationery template for dropdown
            var emptyStationery = new Stationery
            {
                ItemNum = "select a stationery",
                Description = "select a stationery",
                UnitOfMeasure = "-",
                AverageCost = 0.00
            };

            //get list of recommended for purchase stationery and put in purchase order details
            var a = _stationeryRepo.GetOutstandingStationeryByAllSupplier();
            foreach (KeyValuePair<Supplier, List<Stationery>> kvp in a)
            {
                if (kvp.Key.SupplierId == supplier.SupplierId)
                {
                    foreach (Stationery stationery in kvp.Value)
                    {
                        if (stationery.CurrentQty < stationery.ReorderLevel &&
                            stationery.PrimarySupplier().SupplierId == supplierId)
                        {
                            PurchaseOrderDetailDTO pdetails = new PurchaseOrderDetailDTO();
                            pdetails.OrderQty =
                                Math.Max(Convert.ToInt32(stationery.ReorderLevel - stationery.CurrentQty),
                                    Convert.ToInt32(stationery.ReorderQty));
                            pdetails.UnitPrice = stationery.UnitPrice(Convert.ToInt32(supplierId));
                            pdetails.ItemNum = stationery.ItemNum;
                            po.PurchaseOrderDetailsDTO.Add(pdetails);
                        }
                    }

                    break;
                }
            }

            //no of purchase detail lines to show
            var countOfLines = Math.Max(po.PurchaseOrderDetailsDTO.Count, 1);
            //no ofstationery that belong to supplier
            var countOfStationery = Math.Max(po.PurchaseOrderDetailsDTO.Count, 0);


            //create empty puchase details so user can add up to 100 line items per PO
            for (int i = countOfLines; i < 100; i++)
            {
                var pdetails = new PurchaseOrderDetailDTO
                {
                    OrderQty = 0,
                    UnitPrice = emptyStationery.AverageCost,
                    ItemNum = emptyStationery.ItemNum
                };
                po.PurchaseOrderDetailsDTO.Add(pdetails);
            }

            //fill ViewBag to populate stationery dropdown lists
            var stationerySupplier = new StationerySupplier
            {
                ItemNum = emptyStationery.ItemNum,
                Price = emptyStationery.AverageCost,
                Stationery = emptyStationery
            };

            var sslist = new List<StationerySupplier> {stationerySupplier};
            sslist.AddRange(_stationeryRepo.GetStationerySupplierBySupplierId(supplierId).ToList());
            ViewBag.Stationery = sslist;
            ViewBag.Suppliers = _supplierRepo.GetAll();
            ViewBag.Supplier = supplier;
            ViewBag.countOfStationery = countOfStationery;
            ViewBag.countOfLines = countOfLines;
            ViewBag.GST_RATE = GstRate;

            return View(po);
        }


        // POST: PurchaseOrders/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "clerk")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PurchaseOrderDTO purchaseOrderDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    throw new Exception("IT Error: please contact your administrator");

                //fill default values
                var empNum = Convert.ToInt32(Request.Cookies["Employee"]?["EmpNum"]);
                var fullName = Request.Cookies["Employee"]?["Name"];
                purchaseOrderDto.OrderEmpNum = empNum;
                if (purchaseOrderDto.CreateDate == new DateTime())
                    purchaseOrderDto.CreateDate = DateTime.Today;

                //create PO
                purchaseOrderDto.CreatePurchaseOrder(out var purchaseOrder);

                //save to database
                _poRepo.Add(purchaseOrder);

                //send email to supervisor
                var emailForSupervisor = new StringBuilder("New Purchase Order Created");
                emailForSupervisor.AppendLine(
                    "This email is automatically generated and requires no reply to the sender.");
                emailForSupervisor.AppendLine("Purchase Order No " + purchaseOrder.PoNum);
                emailForSupervisor.AppendLine("Created By " + fullName);
                emailForSupervisor.AppendLine("Created On " + purchaseOrder.CreateDate.ToString("dd-MM-yyyy"));
                var subject = "New Purchase Order";
                Emails.EmailHelper.SendEmail(subject, emailForSupervisor.ToString());

                //send email if using non=primary supplier
                var emailBody =
                    new StringBuilder("Non-Primary Suppliers in Purchase Order " + purchaseOrder.PoNum);
                emailForSupervisor.AppendLine(
                    "This email is automatically generated and requires no reply to the sender.");
                emailBody.AppendLine("Created for Supplier: " +
                                     _supplierRepo.GetById(purchaseOrder.SupplierId).SupplierName);
                int index = 0;
                foreach (var orderDetail in purchaseOrder.PurchaseOrderDetails)
                {
                    var s = _stationeryRepo.GetById(orderDetail.ItemNum);
                    if (s.PrimarySupplier().SupplierId != purchaseOrder.SupplierId)
                    {
                        index++;
                        emailBody.AppendLine("Index: " + index);
                        emailBody.AppendLine("Stationery: " + s.Description);
                        emailBody.AppendLine("Primary Supplier: " + s.PrimarySupplier().SupplierName);
                        emailBody.AppendLine();
                    }
                }

                if (index > 0)
                {
                    subject = "Purchasing from Non-Primary Supplier";
                    Emails.EmailHelper.SendEmail(subject, emailBody.ToString());
                }

                return RedirectToAction("Summary");
            }
            catch (Exception e)
            {
                return RedirectToAction("Create",
                    new {supplierId = purchaseOrderDto.SupplierId.ToString(), error = e.Message});
            }
        }


        //GET: PurchaseOrders/Receive?p=10001
        [Authorize(Roles = "clerk")]
        [HttpGet]
        public ActionResult Receive(int? p = null, string error = null)
        {
            //catch error from redirect
            ViewBag.Error = error;

            var receive = new ReceiveTransDTO(); //model to bind data

            if (p == null)
            {
                ViewBag.OrderedPO = _poRepo.GetPOByStatus("ordered");
                return View();
            }

            //populate PO and ReceiveTrans if PO number is given
            var po = new PurchaseOrderDTO(_poRepo.GetById(Convert.ToInt32(p)));
            if (po.Status != "ordered")
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            for (int i = 0; i < po.PurchaseOrderDetails.Count; i++)
            {
                var rdetail = new ReceiveTransDetail
                {
                    ItemNum = po.PurchaseOrderDetails.Skip(i).First().ItemNum,
                    Quantity = 0
                };
                receive.ReceiveTransDetails.Add(rdetail);
            }

            receive.ReceiveDate = DateTime.Today;

            ViewBag.PurchaseOrder = po;
            return View(receive);
        }

        //POST: PurchaseOrders/Receive
        [Authorize(Roles = "clerk")]
        [HttpPost]
        public ActionResult Receive(ReceiveTransDTO receiveModel)
        {
            try
            {
                if (receiveModel.InvoiceNum == null || receiveModel.DeliveryOrderNum == null)
                    throw new Exception("Delivery Order Number and Invoice Number are required fields");

                if (!ModelState.IsValid)
                    throw new Exception("IT Error: please contact your administrator");

                //set date if null
                var receive = receiveModel.ConvertToReceiveTran();
                if (receive.ReceiveDate == new DateTime()) receive.ReceiveDate = DateTime.Today;

                //check validity
                ValidateReceiveTrans(receive);

                //create receive trans, update PO and stationery
                CreateReceiveTrans(receive);

                return RedirectToAction("Summary");
            }
            catch (Exception e)
            {
                return RedirectToAction("Receive", new {p = receiveModel.PoNum.ToString(), error = e.Message});
            }
        }

        public ActionResult PrintPo(int id, double? orderDate)
        {

            var ds = new DataSet();
            var rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports/PoCrystalReport.rpt")));
            if (orderDate != null)
            {
                var date = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                    .AddMilliseconds(Convert.ToDouble(orderDate)).ToLocalTime();
                ds.Tables.Add(GetPo(id, date));
            }
            else
            {
                ds.Tables.Add(GetPo(id));
            }

            rd.SetDataSource(ds);
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            var stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            stream.Seek(0, SeekOrigin.Begin);
            return File(stream, "application/pdf");
        }


        //GET: PurchaseOrders/Order?p=10001
        [HttpGet]
        public async Task<ActionResult> Order(int? p = null, string error = null)
        {
            //catch error from redirect
            ViewBag.Error = error;

            if (p == null)
            {
                ViewBag.ApprovedPO = _poRepo.GetPOByStatus("approved");
                return View();
            }

            //populate PO DTO if PO number is given
            var purchaseOrder = await _poRepo.GetByIdAsync(Convert.ToInt32(p));
            if (purchaseOrder.Status != "approved")
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var po = new PurchaseOrderDTO(purchaseOrder) {OrderDate = DateTime.Today};

            return View(po);
        }

        [Authorize(Roles = "clerk")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Order(PurchaseOrderDTO po)
        {
            try
            {
                if (!ModelState.IsValid)
                    throw new Exception("IT Error: please contact your administrator");
                var purchaseorder = _poRepo.GetById(po.PoNum);
                purchaseorder.Status = "ordered";
                purchaseorder.OrderDate = po.OrderDate;
                if (po.OrderDate < po.CreateDate)
                    throw new Exception("Record not saved, ordered date cannot be before created date");
                _poRepo.Update(purchaseorder);
                return RedirectToAction("Summary");
            }
            catch (Exception e)
            {
                return RedirectToAction("Order", new {p = po.PoNum.ToString(), error = e.Message});
            }
        }

        [Authorize(Roles = "supervisor")]
        public ActionResult PendingPO()
        {
            return View(_poRepo.GetPendingApprovalPODTO());
        }

        [Authorize(Roles = "supervisor")]
        [HttpGet]
        public ActionResult ApproveRejectPO(string list, string status)
        {
            ViewBag.checkList = list;
            ViewBag.status = status;
            return PartialView("_ApproveRejectPO");
        }

        [Authorize(Roles = "supervisor")]
        [HttpPost]
        public ActionResult ApproveRejectPO(string checkList, string status, string a)
        {
            var list = checkList.Split(',');
            var idList = new int[list.Length];
            for (int i = 0; i < idList.Length; i++)
            {
                idList[i] = int.Parse(list[i]);
            }

            foreach (var id in idList)
            {
                _poRepo.UpDatePOStatus(id, status);
            }

            return PartialView("_ApproveRejectPO");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _poRepo.Dispose();
                _stationeryRepo.Dispose();
                _supplierRepo.Dispose();
            }

            base.Dispose(disposing);
        }

        public void ValidateReceiveTrans(ReceiveTran receive)
        {
            var po = _poRepo.GetById(receive.PoNum);
            int? totalQty = 0;
            foreach (var receiveTransDetail in receive.ReceiveTransDetails)
            {
                totalQty += receiveTransDetail.Quantity;
                if (receiveTransDetail.Quantity < 0)
                    throw new Exception("Record not saved, received quantity cannot be negative");
                if (receiveTransDetail.Quantity > po.PurchaseOrderDetails
                        .Where(x => x.ItemNum == receiveTransDetail.ItemNum)
                        .Select(x => x.OrderQty - x.ReceiveQty).First())
                    throw new Exception("Record not saved, received quantity cannot exceed ordered qty");
            }
            if (totalQty == 0)
                throw new Exception("Record not saved, not receipt of goods found");
        }

        public void CreateReceiveTrans(ReceiveTran receive)
        {
            var po = _poRepo.GetById(Convert.ToInt32(receive.PoNum));
            var fulfilled = true;
            for (var i = po.PurchaseOrderDetails.Count - 1; i >= 0; i--)
            {
                var receiveQty = Convert.ToInt32(receive.ReceiveTransDetails.ElementAt(i).Quantity);
                if (receiveQty > 0)
                {
                    //update po received qty
                    po.PurchaseOrderDetails.ElementAt(i).ReceiveQty += receiveQty;
                    if (po.PurchaseOrderDetails.ElementAt(i).ReceiveQty < po.PurchaseOrderDetails.ElementAt(i).OrderQty)
                        fulfilled = false;

                    //get GST rate
                    var gstRate = po.GST / po.PurchaseOrderDetails.Sum(x => x.OrderQty * x.UnitPrice);
                    //update stationery
                    var stationery = _stationeryRepo.GetById(po.PurchaseOrderDetails.ElementAt(i).Stationery.ItemNum);
                    stationery.AverageCost = ((stationery.AverageCost * stationery.CurrentQty)
                                              + (receiveQty * po.PurchaseOrderDetails.ElementAt(i).UnitPrice) * (1 + gstRate))
                                             / (stationery.CurrentQty + receiveQty);
                    stationery.CurrentQty += receiveQty;
                    stationery.AvailableQty += receiveQty;
                    _stationeryRepo.Update(stationery);   //persist stationery data here
                }
                else if (receiveQty == 0)
                    //keep only the receive transactions details with non-zero quantity
                    receive.ReceiveTransDetails.Remove(receive.ReceiveTransDetails.ElementAt(i));
            }

            //update purchase order and create receive trans
            if (fulfilled) po.Status = "fulfilled";
            po.ReceiveTrans.Add(receive);
            _poRepo.Update(po);
        }

        private static DataTable GetPo(int id, DateTime? orderDate = null)
        {
            var orderdatequery = "p.orderdate";
            //handle orderdate
            if (orderDate >= new DateTime(1971, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToLocalTime())
                orderdatequery = "'" + Convert.ToDateTime(orderDate).ToString("yyyy/MM/dd") + "'as orderdate";
            var connString = System.Configuration.ConfigurationManager.ConnectionStrings["LUSSISContext"]
                .ConnectionString;
            var table = new DataTable();
            //get sql
            using (var sqlConn = new SqlConnection(connString))
            {
                var sqlQuery =
                    "select p.ponum,"
                    + orderdatequery +
                    ",p.approvaldate,s.suppliername,p.suppliercontact, p.address1,p.address2,p.address3 " +
                    ",st.description,q.orderqty,q.unitprice,st.unitofmeasure, e.Title+' '+e.firstname+' '+e.lastname as orderby, f.Title+' '+f.firstname+' '+f.lastname as approvedby  " +
                    "from purchaseorder p " +
                    "inner join supplier s on p.supplierid=s.supplierid " +
                    "inner join purchaseorderdetail q on p.ponum=q.ponum " +
                    "inner join stationery st on st.itemnum=q.itemnum " +
                    "inner join employee e on e.empnum=p.orderempnum " +
                    "inner join employee f on f.empnum=p.approvalempnum " +
                    "where p.ponum=@id";
                using (var cmd = new SqlCommand(sqlQuery, sqlConn))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int);
                    cmd.Parameters["@id"].Value = id;
                    var da = new SqlDataAdapter(cmd);
                    da.Fill(table);
                }
            }

            table.TableName = "PurchaseOrder";
            return table;
        }
    }
}


public static class StationeryExtension
{
    public static double UnitPrice(this Stationery s, int supplierId)
    {
        //return null;
        return (from ss in s.StationerySuppliers where ss.SupplierId == supplierId select ss.Price).FirstOrDefault();
    }

    public static double LinePrice(this Stationery s, int supplierId, int? qty)
    {
        return (from ss in s.StationerySuppliers where ss.SupplierId == supplierId select Convert.ToDouble(ss.Price * qty)).FirstOrDefault();
    }

    public static Supplier PrimarySupplier(this Stationery s)
    {
        return (from ss in s.StationerySuppliers where ss.Rank == 1 select ss.Supplier).FirstOrDefault();
    }
}