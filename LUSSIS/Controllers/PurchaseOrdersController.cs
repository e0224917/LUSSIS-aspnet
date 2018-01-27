using System;
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
using System.Web.Helpers;
using System.Web.Mvc;

using LUSSIS.Models;
using LUSSIS.Models.WebDTO;
using LUSSIS.Repositories;
using PagedList;

namespace LUSSIS.Controllers
{
    [Authorize(Roles = "clerk, supervisor")]
    public class PurchaseOrdersController : Controller
    {
        private PORepository pr = new PORepository();
        private DisbursementRepository disRepo = new DisbursementRepository();
        private StockAdjustmentRepository stockRepo = new StockAdjustmentRepository();
        private StationeryRepository sr = new StationeryRepository();
        private EmployeeRepository er = new EmployeeRepository();
        private SupplierRepository sur = new SupplierRepository();
        public const double GST_RATE = 0.07;

        // GET: PurchaseOrders
        public ActionResult Index(int? page = 1)
        {
            var purchaseOrders = pr.GetAll();
            int pageSize = 15;
            ViewBag.page = page;
            return View(purchaseOrders.ToList().OrderByDescending(x => x.CreateDate).ToPagedList(Convert.ToInt32(page), pageSize));
        }

        // GET: PurchaseOrders/Details/10005
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
            PurchaseOrderDTO po = new PurchaseOrderDTO(purchaseOrder);

            //ViewBag.PurchaseOrder = po;
            return View(po);
        }


        //GET: PurchaseOrders/Summary
        [Authorize(Roles = "clerk")]
        public ActionResult Summary()
        {
            ViewBag.OutstandingStationeryList = sr.GetOutstandingStationeryByAllSupplier();
            ViewBag.PendingApprovalPOList = pr.GetPOByStatus("pending");
            ViewBag.OrderedPOList = pr.GetPOByStatus("ordered");
            ViewBag.ApprovedPOList = pr.GetPOByStatus("approved");
            return View();
        }


        // GET: PurchaseOrders/Create or PurchaseOrders/Create?supplierId=1
        [Authorize(Roles = "clerk")]
        public ActionResult Create(int? supplierId, string error = null)
        {
            //catch error from redirect
            ViewBag.Error = error;

            PurchaseOrderDTO po = new PurchaseOrderDTO(); //view model
            int countOfLines = 1; //no of purchase detail lines to show
            int countOfStationery = 0; //no ofstationery that belong to supplier

            if (supplierId == null) //select supplier if non-chosen yet
            {
                Supplier selectASupplier = new Supplier();
                selectASupplier.SupplierId = -1;
                selectASupplier.SupplierName = "Select a Supplier";
                ViewBag.Suppliers = sur.GetAll().Concat(new Supplier[] { selectASupplier });
                ViewBag.Supplier = selectASupplier;
                PurchaseOrderDTO nothingToShow = new PurchaseOrderDTO();
                nothingToShow.SupplierId = -1;
                return View(nothingToShow);
            }

            //get supplier
            Supplier supplier = sur.GetById(Convert.ToInt32(supplierId));
            po.Supplier = supplier;
            po.SupplierId = supplier.SupplierId;
            po.CreateDate = DateTime.Today;
            po.SupplierAddress = supplier.Address1 + Environment.NewLine + supplier.Address2 + Environment.NewLine + supplier.Address3;
            po.SupplierContact = supplier.ContactName;

            //set empty Stationery template for dropdown
            Stationery emptyStationery = new Stationery();
            emptyStationery.ItemNum = "select a stationery";
            emptyStationery.Description = "select a stationery";
            emptyStationery.UnitOfMeasure = "-";
            emptyStationery.AverageCost = 0.00;

            //get list of recommended for purchase stationery and put in purchase order details
            var a = sr.GetOutstandingStationeryByAllSupplier();
            foreach (KeyValuePair<Supplier, List<Stationery>> kvp in a)
            {
                if (kvp.Key.SupplierId == supplier.SupplierId)
                {
                    foreach (Stationery stationery in kvp.Value)
                    {
                        if (stationery.CurrentQty < stationery.ReorderLevel && stationery.PrimarySupplier().SupplierId == supplierId)
                        {
                            PurchaseOrderDetailDTO pdetails = new PurchaseOrderDetailDTO();
                            pdetails.OrderQty = Math.Max(Convert.ToInt32(stationery.ReorderLevel - stationery.CurrentQty), Convert.ToInt32(stationery.ReorderQty));
                            pdetails.UnitPrice = stationery.UnitPrice(Convert.ToInt32(supplierId));
                            pdetails.ItemNum = stationery.ItemNum;
                            po.PurchaseOrderDetailsDTO.Add(pdetails);
                        }
                    }
                    break;
                }
            }
            countOfLines = Math.Max(po.PurchaseOrderDetailsDTO.Count, 1);
            countOfStationery = Math.Max(po.PurchaseOrderDetailsDTO.Count, 0);


            //create empty puchase details so user can add up to 100 line items per PO
            for (int i = countOfLines; i < 100; i++)
            {
                PurchaseOrderDetailDTO pdetails = new PurchaseOrderDetailDTO();
                pdetails.OrderQty = 0;
                pdetails.UnitPrice = emptyStationery.AverageCost;
                pdetails.ItemNum = emptyStationery.ItemNum;
                po.PurchaseOrderDetailsDTO.Add(pdetails);
            }

            //fill ViewBag to populate stationery dropdown lists
            StationerySupplier ss = new StationerySupplier();
            ss.ItemNum = emptyStationery.ItemNum;
            ss.Price = emptyStationery.AverageCost;
            ss.Stationery = emptyStationery;
            List<StationerySupplier> sslist = new List<StationerySupplier>() { ss };
            sslist.AddRange(sr.GetStationerySupplierBySupplierId(supplierId).ToList());
            ViewBag.Stationery = sslist;
            ViewBag.Suppliers = sur.GetAll();
            ViewBag.Supplier = supplier;
            ViewBag.countOfStationery = countOfStationery;
            ViewBag.countOfLines = countOfLines;
            ViewBag.GST_RATE = GST_RATE;

            return View(po);
        }


        // POST: PurchaseOrders/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "clerk")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PurchaseOrderDTO purchaseOrderDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                    throw new Exception("IT Error: please contact your administrator");

                //create PO
                PurchaseOrder purchaseOrder = null;

                //fill default values
                var CurrentUser = er.GetCurrentUser();
                purchaseOrderDTO.OrderEmpNum = CurrentUser.EmpNum;
                if (purchaseOrderDTO.CreateDate == null)
                    purchaseOrderDTO.CreateDate = DateTime.Today;

                //create PO
                purchaseOrderDTO.CreatePurchaseOrder(out purchaseOrder);

                //save to database
                pr.Add(purchaseOrder);

                //send email to supervisor
                StringBuilder emailForSupervisor = new StringBuilder("New Purchase Order Created");
                emailForSupervisor.AppendLine("This email is automatically generated and requires no reply to the sender.");
                emailForSupervisor.AppendLine("Purchase Order No " + purchaseOrder.PoNum);
                emailForSupervisor.AppendLine("Created By " + CurrentUser.FullName);
                emailForSupervisor.AppendLine("Created On " + purchaseOrder.CreateDate.ToString("dd-MM-yyyy"));
                string subject = "New Purchase Order";
                Emails.EmailHelper.SendEmail(subject, emailForSupervisor.ToString());

                //send email if using non=primary supplier
                StringBuilder emailBody = new StringBuilder("Non-Primary Suppliers in Purchase Order " + purchaseOrder.PoNum);
                emailForSupervisor.AppendLine("This email is automatically generated and requires no reply to the sender.");
                emailBody.AppendLine("Created for Supplier: " + sur.GetById(purchaseOrder.SupplierId).SupplierName);
                int index = 0;
                foreach (PurchaseOrderDetail pdetail in purchaseOrder.PurchaseOrderDetails)
                {
                    Stationery s = sr.GetById(pdetail.ItemNum);
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
                return RedirectToAction("Create", new { supplierId = purchaseOrderDTO.SupplierId.ToString(), error = e.Message });
            }
        }


        //GET: PurchaseOrders/Receive?p=10001
        [Authorize(Roles = "clerk")]
        [HttpGet]
        public ActionResult Receive(int? p = null, string error = null)
        {
            //catch error from redirect
            ViewBag.Error = error;

            PurchaseOrderDTO po = null;    //to be put in ViewBag to display
            ReceiveTransDTO receive = new ReceiveTransDTO();    //model to bind data

            if (p == null)
            {
                ViewBag.OrderedPO = pr.GetPOByStatus("ordered");
                return View();
            }

            //populate PO and ReceiveTrans if PO number is given
            po = new PurchaseOrderDTO(pr.GetById(Convert.ToInt32(p)));
            if (po.Status != "ordered")
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            for (int i = 0; i < po.PurchaseOrderDetails.Count; i++)
            {
                ReceiveTransDetail rdetail = new ReceiveTransDetail();
                rdetail.ItemNum = po.PurchaseOrderDetails.Skip(i).First().ItemNum;
                rdetail.Quantity = 0;
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
                ReceiveTran receive = receiveModel.ConvertToReceiveTran();
                if (receive.ReceiveDate == null) receive.ReceiveDate = DateTime.Today;

                //check validity
                pr.ValidateReceiveTrans(receive);

                //create receive trans, update PO and stationery
                pr.CreateReceiveTrans(receive);

                return RedirectToAction("Summary");
            }
            catch (Exception e)
            {
                return RedirectToAction("Receive", new { p = receiveModel.PoNum.ToString(), error = e.Message });
            }
        }

       /* public ActionResult PrintPo(int id, double? orderDate)
        {

            DateTime OrderDate;
            
            DataSet ds = new DataSet();
            //ReportDocument rd = new ReportDocument();
            //rd.Load(Path.Combine(Server.MapPath("~/Reports/PoCrystalReport.rpt")));
            if (orderDate != null)
            {
                OrderDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(Convert.ToDouble(orderDate)).ToLocalTime();
                ds.Tables.Add(GetPo(id, OrderDate));
            }
            else
            {
                ds.Tables.Add(GetPo(id));
            }
            rd.SetDataSource(ds);
            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();
            Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            stream.Seek(0, SeekOrigin.Begin);
            return File(stream, "application/pdf");
        }*/



        //GET: PurchaseOrders/Order?p=10001
        [HttpGet]
        public async Task<ActionResult> Order(int? p = null, string error = null)
        {
            //catch error from redirect
            ViewBag.Error = error;

            PurchaseOrderDTO po = null;
            if (p == null)
            {
                ViewBag.ApprovedPO = pr.GetPOByStatus("approved");
                return View();
            }

            //populate PO DTO if PO number is given
            PurchaseOrder purchaseOrder = await pr.GetByIdAsync(Convert.ToInt32(p));
            if (purchaseOrder.Status != "approved")
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            po = new PurchaseOrderDTO(purchaseOrder);
            po.OrderDate = DateTime.Today;

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
                PurchaseOrder purchaseorder = pr.GetById(po.PoNum);
                purchaseorder.Status = "ordered";
                purchaseorder.OrderDate = po.OrderDate;
                if (po.OrderDate < po.CreateDate)
                    throw new Exception("Record not saved, ordered date cannot be before created date");
                pr.Update(purchaseorder);
                return RedirectToAction("Summary");
            }
            catch (Exception e)
            {
                return RedirectToAction("Order", new { p = po.PoNum.ToString(), error = e.Message });
            }
        }
        [Authorize(Roles = "supervisor")]
        public ActionResult PendingPO()
        {

            return View(pr.GetPendingApprovalPODTO());

        }
        [Authorize(Roles = "supervisor")]
        [HttpGet]
        public ActionResult ApproveRejectPO(String List, String Status)
        {

            ViewBag.checkList = List;
            ViewBag.status = Status;
            return PartialView("ApproveRejectPO");
        }
        [Authorize(Roles = "supervisor")]
        [HttpPost]
        public ActionResult ApproveRejectPO(String checkList, String status, String a)
        {
            String[] list = checkList.Split(',');
            int[] idList = new int[list.Length];
            for (int i = 0; i < idList.Length; i++)
            {
                idList[i] = Int32.Parse(list[i]);
            }
            foreach (int i in idList)
            {
                pr.UpDatePOStatus(i, status);
            }
            return PartialView();
        }

        DataTable GetPo(int id, DateTime? orderDate=null)
        {
            string orderdatequery = "p.orderdate";
            //handle orderdate
            if (orderDate >= new DateTime(1971, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToLocalTime())
                orderdatequery = "'"+Convert.ToDateTime(orderDate).ToString("yyyy/MM/dd") + "'as orderdate";
            string connString = System.Configuration.ConfigurationManager.ConnectionStrings["LUSSISContext"].ConnectionString;
            DataTable table = new DataTable();
            //get sql
            using (SqlConnection sqlConn = new SqlConnection(connString))
            {
                string sqlQuery =
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
                using (SqlCommand cmd = new SqlCommand(sqlQuery, sqlConn))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int);
                    cmd.Parameters["@id"].Value = id;
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
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
        double price = 0;
        foreach (StationerySupplier ss in s.StationerySuppliers)
        {
            if (ss.SupplierId == supplierId)
            {
                price = ss.Price;
                break;
            }
        }
        //return null;
        return price;
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

