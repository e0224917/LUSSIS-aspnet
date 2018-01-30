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

        // GET: Upcoming Disbursement
        public ActionResult Upcoming()
        {
            var disbursements = _disbursementRepo.GetDisbursementByStatus(InProcess);
            return View(disbursements.ToList());
        }

        // GET: Disbursement/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var disbursement = _disbursementRepo.GetById((int) id);

            var disDetailDto = new DisbursementDetailDTO {CurrentDisbursement = disbursement};
            if (disDetailDto.CurrentDisbursement == null)
            {
                return HttpNotFound();
            }

            disDetailDto.DisDetailList = disDetailDto.CurrentDisbursement.DisbursementDetails.ToList();
            return View(disDetailDto);
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
                EmailHelper.SendEmail(email);

                return RedirectToAction("Upcoming");
            }

            ViewBag.CollectionPointId = new SelectList(_collectionRepo.GetAll(), "CollectionPointId",
                "CollectionName", disbursement.CollectionPointId);
            return View(disbursement);
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
                //Disbursement d = disbursementDTO.CurrentDisbursement;
                var disbursementId = disbursementDTO.CurrentDisbursement.DisbursementId;
                var disbursement = _disbursementRepo.GetById(disbursementId);
                foreach (var disbursementDetail in disbursement.DisbursementDetails)
                {
                    disbursementDetail.ActualQty = disbursementDTO.DisDetailList
                        .First(ddEdited => ddEdited.ItemNum == disbursementDetail.ItemNum)
                        .ActualQty;
                }
                _disbursementRepo.Update(disbursement);
                
                switch (update)
                {
                    case "Acknowledge Manually":
                        _disbursementRepo.Acknowledge(disbursement);
                        return RedirectToAction("Upcoming");
                    case "Generate QR Code":
                        break;
                }
                return Json("Ok");
            }

            return View("Details", disbursementDTO);
        }

        public PartialViewResult _QR(string id)
        {
            var qrGen = new QRCodeGenerator();
            var qrCodeData = qrGen.CreateQrCode(id, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new Base64QRCode(qrCodeData);
            var qr = qrCode.GetGraphic(20);
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

            var disbursements = !string.IsNullOrEmpty(searchString)
                ? _disbursementRepo.GetDisbursementsByDeptName(searchString).ToList()
                : _disbursementRepo.GetAll().OrderByDescending(d => d.CollectionDate).ToList();

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
            }

            base.Dispose(disposing);
        }
    }
}