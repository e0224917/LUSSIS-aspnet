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
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using CrystalDecisions.CrystalReports.Engine;
using LUSSIS.Constants;
using LUSSIS.Emails;
using LUSSIS.Models;
using LUSSIS.Models.WebDTO;
using LUSSIS.Repositories;
using PagedList;
using static LUSSIS.Constants.POStatus;


namespace LUSSIS.Controllers
{
    //Authors: Douglas Lee Kiat Hui Authors: May Zin Ko 
    [Authorize(Roles = "clerk, supervisor")]
    public class PurchaseOrdersController : Controller
    {
        private readonly PORepository _poRepo = new PORepository();
        private readonly StationeryRepository _stationeryRepo = new StationeryRepository();
        private readonly SupplierRepository _supplierRepo = new SupplierRepository();
        private const double GstRate = 0.07;
        private readonly EmployeeRepository _employeeRepo = new EmployeeRepository();
        private readonly StationerySupplierRepository _stationerySupplierRepo = new StationerySupplierRepository();


        // GET: PurchaseOrders 
        public ActionResult Index(int? page = 1)
        {
            var purchaseOrders = _poRepo.GetAll();
            ViewBag.page = page;
            //get full list of purchase ordered by most recently created
            return View(purchaseOrders.ToList().OrderByDescending(x => x.PoNum)
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
        [Authorize(Roles = Role.Clerk)]
        public ActionResult Summary()
        {
            //each viewbag is for one section.
            //the sections are 1)items recommended for purchase, pending POs, ordered POs, approved POs 
            ViewBag.OutstandingStationeryList = _stationeryRepo.GetOutstandingStationeryByAllSupplier();
            ViewBag.PendingApprovalPOList = _poRepo.GetPOByStatus(Pending);
            ViewBag.OrderedPOList = _poRepo.GetPOByStatus(Ordered);
            ViewBag.ApprovedPOList = _poRepo.GetPOByStatus(Approved);
            return View();
        }


        // GET: PurchaseOrders/Create or PurchaseOrders/Create?supplierId=1
        [Authorize(Roles = Role.Clerk)]
        public ActionResult Create(int? supplierId, string error = null)
        {
            //catch error from redirect (from POST) and display back into page
            ViewBag.Error = error;

            if (supplierId == null) //allow user to select supplier if non is chosen yet
            {
                var emptySupplier = new Supplier
                {
                    SupplierId = -1,
                    SupplierName = "Select a Supplier"
                };

                ViewBag.Suppliers = _supplierRepo.GetAll().Concat(new[] { emptySupplier });
                ViewBag.Supplier = emptySupplier;

                var emptyPo = new PurchaseOrderDTO { SupplierId = -1 };
                return View(emptyPo);
            }

            //get supplier
            var supplier = _supplierRepo.GetById(Convert.ToInt32(supplierId));
            var po = new PurchaseOrderDTO()
            {   //view model
                Supplier = supplier,
                SupplierId = supplier.SupplierId,
                CreateDate = DateTime.Today,
                SupplierAddress = supplier.Address1 + Environment.NewLine
                    + supplier.Address2 + Environment.NewLine
                    + supplier.Address3,
                SupplierContact = supplier.ContactName
            };

            //set empty Stationery template for dropdown
            var emptyStationery = new Stationery
            {
                ItemNum = "select a stationery",
                Description = "select a stationery",
                UnitOfMeasure = "-",
                AverageCost = 0.00
            };

            //get list of recommended for purchase stationery and put in purchase order details
            foreach (KeyValuePair<Supplier, List<Stationery>> kvp in _stationeryRepo.GetOutstandingStationeryByAllSupplier())
            {
                if (kvp.Key.SupplierId == supplierId)
                {
                    foreach (Stationery stationery in kvp.Value)
                    {
                        PurchaseOrderDetailDTO pdetails = new PurchaseOrderDetailDTO();
                        pdetails.OrderQty =
                                            Math.Max(Convert.ToInt32(stationery.ReorderLevel - stationery.AvailableQty),
                                                Convert.ToInt32(stationery.ReorderQty));
                        pdetails.UnitPrice = stationery.UnitPrice(Convert.ToInt32(supplierId));
                        pdetails.ItemNum = stationery.ItemNum;
                        pdetails.ReorderQty = stationery.ReorderQty;
                        po.PurchaseOrderDetailsDTO.Add(pdetails);
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
            var sslist = new List<StationerySupplier> { stationerySupplier };
            sslist.AddRange(_stationerySupplierRepo.GetStationerySupplierBySupplierId(supplierId).ToList());
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
        [Authorize(Roles = Role.Clerk)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PurchaseOrderDTO purchaseOrderDto)
        {
            try
            {

                //validate PO
                if (purchaseOrderDto.SupplierContact == null)
                    throw new Exception("Please input the supplier contact");
               
                else if (!ModelState.IsValid)
                    throw new Exception("IT Error: please contact your administrator");

                //fill any missing data with default values
                var empNum = Convert.ToInt32(Request.Cookies["Employee"]?["EmpNum"]);
                var fullName = Request.Cookies["Employee"]?["Name"];
                purchaseOrderDto.OrderEmpNum = empNum;
                if (purchaseOrderDto.CreateDate == new DateTime())
                    purchaseOrderDto.CreateDate = DateTime.Today;

                //create PO
                purchaseOrderDto.CreatePurchaseOrder(out var purchaseOrder);

                //save to database po and the updated available qty
                _poRepo.Add(purchaseOrder);
                foreach (PurchaseOrderDetail pdetail in purchaseOrder.PurchaseOrderDetails)
                {
                    Stationery stationery = _stationeryRepo.GetById(pdetail.ItemNum);
                    stationery.AvailableQty += pdetail.OrderQty;
                    _stationeryRepo.Update(stationery);
                }

                //send email to supervisor
                var supervisorEmail = new EmployeeRepository().GetStoreSupervisor().EmailAddress;
                var email = new LUSSISEmail.Builder().From(User.Identity.Name)
                .To(supervisorEmail).ForNewPo(purchaseOrder, fullName).Build();
                //start new thread to send email
                new Thread(delegate () { EmailHelper.SendEmail(email); }).Start();


                //send email if using non=primary supplier
                var stationerys = purchaseOrder.PurchaseOrderDetails
                    .Select(orderDetail => _stationeryRepo.GetById(orderDetail.ItemNum))
                    .Where(stationery => stationery.PrimarySupplier().SupplierId != purchaseOrder.SupplierId).ToList();
                if (stationerys.Count > 0)
                {
                    var supplierName = _supplierRepo.GetById(purchaseOrder.SupplierId).SupplierName;
                    var email2 = new LUSSISEmail.Builder().From(User.Identity.Name).To(supervisorEmail)
                        .ForNonPrimaryNewPo(supplierName, purchaseOrder, stationerys).Build();
                    new Thread(delegate () { EmailHelper.SendEmail(email2); }).Start();
                }


                return RedirectToAction("Summary");
            }
            catch (Exception e)
            {
                return RedirectToAction("Create",
                    new { supplierId = purchaseOrderDto.SupplierId.ToString(), error = e.Message });
            }
        }


        //GET: PurchaseOrders/Receive?p=10001
        [Authorize(Roles = Role.Clerk)]
        [HttpGet]
        public ActionResult Receive(int? p = null, string error = null)
        {
            //catch error from redirect (from POST) and display back into page
            ViewBag.Error = error;

            var receive = new ReceiveTransDTO(); //model to bind data

            if (p == null)
            {
                ViewBag.OrderedPO = _poRepo.GetPOByStatus(Ordered);
                return View();
            }

            //populate PO and ReceiveTrans if PO number is given
            var po = new PurchaseOrderDTO(_poRepo.GetById(Convert.ToInt32(p)));
            if (po.Status != Ordered)
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
        [Authorize(Roles = Role.Clerk)]
        [HttpPost]
        public ActionResult Receive(ReceiveTransDTO receiveModel)
        {
            try
            {
                //validate receive trans
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
                return RedirectToAction("Receive", new { p = receiveModel.PoNum.ToString(), error = e.Message });
            }
        }

        public ActionResult PrintPo(int id, double? orderDate)
        {

            //prepare crystal report to be published in pdf, using datatable format
            DataSet ds = new DataSet();
            ReportDocument rd = new ReportDocument();
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

            //allow user to pick any 'pending order' POs
            if (p == null)
            {
                ViewBag.ApprovedPO = _poRepo.GetPOByStatus(Approved);
                return View();
            }

            //populate PO DTO if PO number is given
            var purchaseOrder = await _poRepo.GetByIdAsync(Convert.ToInt32(p));
            if (purchaseOrder.Status != Approved)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var po = new PurchaseOrderDTO(purchaseOrder) { OrderDate = DateTime.Today };

            return View(po);
        }

        [Authorize(Roles = Role.Clerk)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Order(PurchaseOrderDTO po)
        {
            try
            {

                if (!ModelState.IsValid)
                    throw new Exception("IT Error: please contact your administrator");

                //get PO
                var purchaseorder = _poRepo.GetById(po.PoNum);
                //update status and order date
                purchaseorder.Status = Ordered;
                purchaseorder.OrderDate = po.OrderDate;
                if (po.OrderDate < po.CreateDate)
                    throw new Exception("Record not saved, ordered date cannot be before created date");

                //persist data
                _poRepo.Update(purchaseorder);
                return RedirectToAction("Summary");
            }
            catch (Exception e)
            {
                return RedirectToAction("Order", new { p = po.PoNum.ToString(), error = e.Message });
            }
        }

        [Authorize(Roles = Role.Supervisor)]
        public ActionResult PendingPO()
        {
            return View(_poRepo.GetPendingApprovalPODTO());
        }

        [Authorize(Roles = Role.Supervisor)]
        [HttpGet]
        public ActionResult _ApproveRejectPO(string list, string status)
        {
            ViewBag.checkList = list;
            ViewBag.status = status;
            return PartialView("_ApproveRejectPO");
        }

        [Authorize(Roles = Role.Supervisor)]
        [HttpPost]
        public ActionResult _ApproveRejectPO(string status, string checkList, string a)
        {
            var list = checkList.Split(',');
            var idList = new int[list.Length];
            for (int i = 0; i < idList.Length; i++)
            {
                idList[i] = int.Parse(list[i]);
            }

            foreach (int id in idList)
            {
                HttpCookie cookie = HttpContext.Request.Cookies.Get("Employee");
                String empNum = cookie["EmpNum"];
                _poRepo.UpDatePO(id, status.ToUpper() == "APPROVE" ? Approved : Rejected, empNum);

                if (status.ToUpper() == "APPROVE")
                {
                    List<PurchaseOrderDetail> pDetail = _poRepo.GetPurchaseOrderDetailsById(id).ToList();

                    foreach (var p in pDetail)
                    {
                        Stationery st = new Stationery();
                        st = _stationeryRepo.GetById(p.ItemNum);
                        st.AvailableQty = p.OrderQty + st.AvailableQty;
                        _stationeryRepo.Update(st);
                    }
                }
            }

            return PartialView("_ApproveRejectPO");
        }
        //[Authorize(Roles = Role.Supervisor)]
        //[HttpPost]

        //public ActionResult DeleteItem(string id)
        //{

        //    return View();
        //}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _poRepo.Dispose();
                _stationeryRepo.Dispose();
                _supplierRepo.Dispose();
                _employeeRepo.Dispose();
                _stationerySupplierRepo.Dispose();
            }

            base.Dispose(disposing);
        }

        public void ValidateReceiveTrans(ReceiveTran receive)
        {
            var po = _poRepo.GetById(receive.PoNum);
            int? totalQty = 0;

            //check that received qty is between 0 to ordered qty
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

            //check that at least one line item is received
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
            if (fulfilled) po.Status = Fulfilled;
            po.ReceiveTrans.Add(receive);
            _poRepo.Update(po);
        }


        //to get all the purchase order and line item details to publish to pdf/crystal report
        //returns data in a datatable class
        private static DataTable GetPo(int id, DateTime? orderDate = null)
        {
            var orderdatequery = "p.orderdate";
            //handle orderdate, because the order date in the server is not updated yet when client requests for pdf
            if (orderDate >= new DateTime(1971, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToLocalTime())
                orderdatequery = "'" + Convert.ToDateTime(orderDate).ToString("yyyy/MM/dd") + "'as orderdate";
            var connString = System.Configuration.ConfigurationManager.ConnectionStrings["LUSSISContext"]
                .ConnectionString;
            var table = new DataTable();
            //create sql query with parameter 'id' for purchase order number
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
                    "left join employee f on f.empnum=p.approvalempnum " +
                    "where p.ponum=@id";
                using (var cmd = new SqlCommand(sqlQuery, sqlConn))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int);
                    cmd.Parameters["@id"].Value = id;
                    var da = new SqlDataAdapter(cmd);
                    da.Fill(table);
                }
            }

            //set table name because that is the name crystal report is expecting
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