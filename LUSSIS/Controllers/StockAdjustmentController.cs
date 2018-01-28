using LUSSIS.Models;
using LUSSIS.Models.WebDTO;
using LUSSIS.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using PagedList;
using System.Text;
using LUSSIS.Emails;

namespace LUSSIS.Controllers
{
    [Authorize(Roles = "clerk, supervisor, manager")]
    public class StockAdjustmentController : Controller
    {
        private LUSSISContext db = new LUSSISContext();
        private StationeryRepository _stationeryRepo = new StationeryRepository();
        private StockAdjustmentRepository _stockadjustmentRepo = new StockAdjustmentRepository();
        private EmployeeRepository _employeeRepo = new EmployeeRepository();

        // GET: StockAdjustment
        public ActionResult Index()
        {
            return RedirectToAction("History");
        }

        public ActionResult History(string searchString, string currentFilter, int? page)
        {
            List<AdjVoucher> adjustments = new List<AdjVoucher>();
            if (searchString != null)
            { page = 1; }
            else
            {
                searchString = currentFilter;
            }

            if (!String.IsNullOrEmpty(searchString))
            {
                adjustments = _stockadjustmentRepo.GetAllAdjVoucherSearch(searchString);
            }
            else
            {
                adjustments = _stockadjustmentRepo.GetAll().ToList();
            }
            int pageSize = 15;
            int pageNumber = (page ?? 1);

            var reqAll = adjustments.ToPagedList(pageNumber, pageSize);

            if (Request.IsAjaxRequest())
            {
                return PartialView("_History", reqAll);
            }

            return View(reqAll);
        }











        public ActionResult Details(int id)
        {
            return View();
        }

        [Authorize(Roles = "supervisor, manager")]
        public ActionResult Approve(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AdjVoucher adjVoucher = _stockadjustmentRepo.GetById((int)id);
            if (adjVoucher == null)
            {
                return HttpNotFound();
            }
            ViewBag.ApprovalEmpNum = new SelectList(db.Employees, "EmpNum", "Title", adjVoucher.ApprovalEmpNum);
            ViewBag.RequestEmpNum = new SelectList(db.Employees, "EmpNum", "Title", adjVoucher.RequestEmpNum);
            ViewBag.ItemNum = new SelectList(db.Stationeries, "ItemNum", "Description", adjVoucher.ItemNum);
            return View(adjVoucher);
        }

        [Authorize(Roles = "supervisor, manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Approve([Bind(Include = "AdjVoucherId,ItemNum,ApprovalEmpNum,Quantity,Reason,CreateDate,ApprovalDate,RequestEmpNum,Status,Remark")] AdjVoucher adjVoucher)
        {
            if (ModelState.IsValid)
            {
                db.Entry(adjVoucher).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("History");
            }
            ViewBag.ApprovalEmpNum = new SelectList(db.Employees, "EmpNum", "Title", adjVoucher.ApprovalEmpNum);
            ViewBag.RequestEmpNum = new SelectList(db.Employees, "EmpNum", "Title", adjVoucher.RequestEmpNum);
            ViewBag.ItemNum = new SelectList(db.Stationeries, "ItemNum", "Description", adjVoucher.ItemNum);
            return View(adjVoucher);
        }

        [Authorize(Roles = "supervisor, manager")]
        public ActionResult AdjustmentApproveReject()
        {
            return View(_stockadjustmentRepo.GetPendingAdjustmentList());
        }




        // GET: StockAdjustment/Delete/5
        //autogen?
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AdjVoucher adjVoucher = await db.AdjVouchers.FindAsync(id);
            if (adjVoucher == null)
            {
                return HttpNotFound();
            }
            return View(adjVoucher);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _stationeryRepo.Dispose();
                _stockadjustmentRepo.Dispose();
                _employeeRepo.Dispose();
            }
            base.Dispose(disposing);
        }
        [Authorize(Roles = "clerk")]
        [HttpGet]
        public ActionResult CreateAdjustments()
        {
            AdjVoucherColView aVCV = new AdjVoucherColView();
            List<AdjustmentVoucherDTO> aVlist = new List<AdjustmentVoucherDTO>();
            AdjustmentVoucherDTO aV = new AdjustmentVoucherDTO();
            aVlist.Add(aV);
            aVCV.MyList = aVlist;
            return View("CreateAdjustments", aVCV);
        }
        [Authorize(Roles = "clerk")]
        [HttpPost]
        public ActionResult CreateAdjustments(AdjVoucherColView kk)
        {
            Employee self = _employeeRepo.GetCurrentUser();
            int ENum = self.EmpNum;
            DateTime todayDate = DateTime.Today;
            if (ModelState.IsValid)
            {
                if (kk.MyList != null)
                {
                    //Although there is a threshold of $250, both supervisor and manager will be informed of all adjustments regardless of price
                    //If desired, the threshold can be applied by getting price * quantity and setting if (total price > 250) 
                    string destinationEmail = _employeeRepo.GetStoreManager().EmailAddress;     
                    string destinationEmail2 = _employeeRepo.GetStoreSupervisor().EmailAddress;
                    string subject = "A new adjustment of stationeries has been made by " + self.FullName;
                    StringBuilder body = new StringBuilder();
                    body.AppendLine(self.FullName + " has made the following adjustment: ");
                    foreach (AdjustmentVoucherDTO AVDTO in kk.MyList)
                    {
                        if (AVDTO.Sign == false)
                        {
                            AVDTO.Quantity = AVDTO.Quantity * -1;
                        }
                        AdjVoucher Adj = _stockadjustmentRepo.ConvertDTOAdjVoucher(AVDTO);
                        Adj.RequestEmpNum = ENum;
                        Adj.CreateDate = todayDate;
                        _stockadjustmentRepo.Add(Adj);
                        Stationery st = _stationeryRepo.GetById(AVDTO.ItemNum);

                        body.AppendLine("Stationery: " + st.Description);
                        body.AppendLine("Quantity: " + AVDTO.Quantity);
                        body.AppendLine();
                                              
                    }
                    body.AppendLine("by " + self.FullName + "on" + DateTime.Now.ToString());
                    EmailHelper.SendEmail(destinationEmail, subject, body.ToString());
                    EmailHelper.SendEmail(destinationEmail2, subject, body.ToString());
                    return RedirectToAction("History");
                }
                else { return View(kk); }

            }
            return View(kk);
        }

        [Authorize(Roles = "clerk")]
        public PartialViewResult _CreateAdjustments()
        {
            return PartialView("_CreateAdjustments", new AdjustmentVoucherDTO());

        }

        [Authorize(Roles = "clerk")]
        [HttpGet]
        public ActionResult CreateAdjustment(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                AdjustmentVoucherDTO adj = new AdjustmentVoucherDTO();
                adj.ItemNum = id;
                adj.Stationery = _stationeryRepo.GetById(id);
                return View(adj);
            }
        }
        [Authorize(Roles = "clerk")]
        [HttpPost]
        public ActionResult CreateAdjustment([Bind(Include = "Quantity,Reason,ItemNum,Sign")]AdjustmentVoucherDTO adjVoucher)
        {
            if (ModelState.IsValid)
            {
                Employee self = _employeeRepo.GetCurrentUser();
                if (adjVoucher.Sign == false)
                { adjVoucher.Quantity = adjVoucher.Quantity * -1; }
                AdjVoucher adj = _stockadjustmentRepo.ConvertDTOAdjVoucher(adjVoucher);
                adj.CreateDate = DateTime.Today;
                adj.RequestEmpNum = self.EmpNum;
                _stockadjustmentRepo.Add(adj);
                string destinationEmail = _employeeRepo.GetStoreManager().EmailAddress;     
                string destinationEmail2 = _employeeRepo.GetStoreSupervisor().EmailAddress;
                string subject = "A new adjustment of stationeries has been made by " + self.FullName;
                StringBuilder body = new StringBuilder();
                Stationery st = _stationeryRepo.GetById(adjVoucher.ItemNum);

                body.AppendLine(self.FullName + " has made the following adjustment: ");
                body.AppendLine("Stationery: " + st.Description);
                body.AppendLine("Quantity: " + adjVoucher.Quantity.ToString());
                body.AppendLine("by " + self.FullName + " on " + DateTime.Now.ToString());
                EmailHelper.SendEmail(destinationEmail, subject, body.ToString());
                EmailHelper.SendEmail(destinationEmail2, subject, body.ToString());
                return RedirectToAction("History");
            }
            else
            {
                adjVoucher.Stationery = _stationeryRepo.GetById(adjVoucher.ItemNum);
                return View(adjVoucher);
            }

        }

        [Authorize(Roles = "clerk")]
        [HttpGet]
        public JsonResult GetItemNum(string term)
        {
            List<String> itemList;
            if (string.IsNullOrEmpty(term))
            {
                itemList = _stationeryRepo.GetAllItemNum().ToList();
            }
            else
            {

                itemList = _stationeryRepo.GetAllItemNum().ToList().FindAll(x => x.StartsWith(term, StringComparison.OrdinalIgnoreCase));
            }
            return Json(itemList, JsonRequestBehavior.AllowGet);
        }


        [Authorize(Roles = "manager,supervisor")]
        public ActionResult ViewPendingStockAdj()
        {
            var user = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            ViewBag.Message = user.GetRoles(System.Web.HttpContext.Current.User.Identity.GetUserId()).First().ToString();
           
                return View(_stockadjustmentRepo.ViewPendingStockAdj(ViewBag.Message));
            
          

        }
        [Authorize(Roles = "manager,supervisor")]
        [HttpGet]
        public ActionResult ApproveReject(String List, String Status)
        {
            //  List<AdjVoucher> list = _stockadjustmentRepo.GetAdjustmentById(List);
            ViewBag.checkList = List;
            ViewBag.status = Status;
            return PartialView("ApproveReject");
        }
        [Authorize(Roles = "manager,supervisor")]
        [HttpPost]
        public ActionResult ApproveReject(String checkList, String comment, String status)
        {
            String[] list = checkList.Split(',');
            int[] idList = new int[list.Length];
            for (int i = 0; i < idList.Length; i++)
            {
                idList[i] = Int32.Parse(list[i]);
            }
            foreach (int i in idList)
            {
                _stockadjustmentRepo.UpDateAdjustmentStatus(i, status, comment);
            }
            return PartialView();
        }






    }
    }
