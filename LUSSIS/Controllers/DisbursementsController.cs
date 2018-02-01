using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using LUSSIS.Constants;
using LUSSIS.Emails;
using LUSSIS.Models;
using LUSSIS.Models.WebDTO;
using LUSSIS.Repositories;
using PagedList;
using QRCoder;
using static LUSSIS.Constants.DisbursementStatus;

namespace LUSSIS.Controllers
{
    //Authors: Tang Xiaowen
    [Authorize(Roles = Role.Clerk)]
    public class DisbursementsController : Controller
    {
        private readonly DisbursementRepository _disbursementRepo = new DisbursementRepository();
        private readonly EmployeeRepository _employeeRepo = new EmployeeRepository();
        private readonly CollectionRepository _collectionRepo = new CollectionRepository();
        private readonly StationeryRepository _stationeryRepo = new StationeryRepository();

        // GET: Upcoming Disbursement
        public ActionResult Upcoming()
        {
            return View(_disbursementRepo.GetDisbursementByStatus(InProcess).ToList());
        }
        
        // GET: Disbursement/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var disbursement = await _disbursementRepo.GetByIdAsync((int) id);
            if (disbursement == null)
            {
                return HttpNotFound();
            }

            ViewBag.CollectionPointId = new SelectList(_collectionRepo.GetAll(), "CollectionPointId",
                "CollectionName", disbursement.CollectionPointId);

            return View(disbursement);
        }

        //POST: Disbursement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include =
                "DisbursementId, CollectionDate, CollectionPointId, AcknowledgeEmpNum, DeptCode, Status")]
            Disbursement disbursement)
        {
            if (ModelState.IsValid)
            {
                _disbursementRepo.Update(disbursement);

                var repEmail = _employeeRepo.GetRepByDeptCode(disbursement.DeptCode).EmailAddress;
                var collectionPoint = _collectionRepo.GetById((int)disbursement.CollectionPointId);
                var email = new LUSSISEmail.Builder().From(User.Identity.Name).To(repEmail)
                    .ForUpdateDisbursement(disbursement, collectionPoint).Build();
                new System.Threading.Thread(delegate () { EmailHelper.SendEmail(email); }).Start();

                return RedirectToAction("Upcoming");
            }

            ViewBag.CollectionPointId = new SelectList(_collectionRepo.GetAll(), "CollectionPointId",
                "CollectionName", disbursement.CollectionPointId);
            return View(disbursement);
        }
        
        // GET: Disbursement/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var disbursement = _disbursementRepo.GetById((int)id);

            if (disbursement == null)
            {
                return HttpNotFound();
            }

            var disDetailDto = new DisbursementDetailDTO
            {
                CurrentDisbursement = disbursement,
                DisDetailList = disbursement.DisbursementDetails.ToList()
            };
            
            return View(disDetailDto);
        }

        [OverrideAuthorization]
        [Authorize(Roles = "clerk, rep")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Acknowledge(DisbursementDetailDTO disbursementDTO, string update)
        {
            if (disbursementDTO == null)
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            { 
                var disbursement = _disbursementRepo.GetById(disbursementDTO.CurrentDisbursement.DisbursementId);

                //set the actual qty of its detail list items to the updated qty, and update it in database
                foreach (var disbursementDetail in disbursement.DisbursementDetails)
                {
                    disbursementDetail.ActualQty = disbursementDTO.DisDetailList
                        .First(ddEdited => ddEdited.ItemNum == disbursementDetail.ItemNum)
                        .ActualQty;
                }
                _disbursementRepo.Update(disbursement);

                switch (update)
                {
                    //case 1: confirm items disbursed and update stationery qty
                    case "Acknowledge Manually":
                        _disbursementRepo.Acknowledge(disbursement);
                        foreach (var dd in disbursement.DisbursementDetails)
                        {
                            var stationery = _stationeryRepo.GetById(dd.Stationery.ItemNum);
                            stationery.CurrentQty -= dd.ActualQty;
                            _stationeryRepo.Update(stationery);
                        }
                        return RedirectToAction("Upcoming");
                    
                    //case 2: only updates actual qty and leave changing status and deduct stock qty to WebAPI
                    case "Generate QR Code":
                        break;
                }
                return Json("Ok");
            }

            return View("Details", disbursementDTO);
        }

        public PartialViewResult _QR(string id)
        {
            var qrCodeData = new QRCodeGenerator().CreateQrCode(id, QRCodeGenerator.ECCLevel.Q);
            var qr = new Base64QRCode(qrCodeData).GetGraphic(20);
            ViewBag.generatedQrCode = qr;
            return PartialView();
        }

        public ActionResult History(string searchString, string currentFilter, int? page)
        {
            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            var disbursements = string.IsNullOrEmpty(searchString)? 
                _disbursementRepo.GetAll().OrderByDescending(d => d.CollectionDate).ToList():
                _disbursementRepo.GetDisbursementsByDeptName(searchString).ToList();

            var disHistory = disbursements.ToPagedList(pageNumber: page ?? 1, pageSize: 15);

            if (Request.IsAjaxRequest())
            {
                return PartialView("_History", disHistory);
            }

            return View(disHistory);
        }

        public ActionResult HistoryDetails(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var d = _disbursementRepo.GetById((int) id);
            if (d == null)
            {
                return HttpNotFound();
            }

            return View(d.DisbursementDetails);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _disbursementRepo.Dispose();
                _collectionRepo.Dispose();
                _employeeRepo.Dispose();
                _stationeryRepo.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}