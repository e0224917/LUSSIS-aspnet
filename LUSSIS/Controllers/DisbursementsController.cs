using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using LUSSIS.Models;
using LUSSIS.Models.WebDTO;
using LUSSIS.Repositories;
using PagedList;
using QRCoder;

namespace LUSSIS.Controllers
{
    [Authorize(Roles = "clerk")]
    public class DisbursementsController : Controller
    {

        private DisbursementRepository _disbursementRepo = new DisbursementRepository();


        // GET: Upcoming Disbursement
        public ActionResult Upcoming()
        {
            var disbursements = _disbursementRepo.GetInProcessDisbursements();
            return View(disbursements.ToList());
        }


        // GET: Disbursement/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DisbursementDetailDTO disDetailDTO = new DisbursementDetailDTO();
            disDetailDTO.CurrentDisbursement = _disbursementRepo.GetById((int)id);
            if (disDetailDTO.CurrentDisbursement == null)
            {
                return HttpNotFound();
            }
            disDetailDTO.DisDetailList = disDetailDTO.CurrentDisbursement.DisbursementDetails.ToList();
            return View(disDetailDTO);
        }


        // GET: Disbursement/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Disbursement disbursement = await _disbursementRepo.GetByIdAsync((int)id);
            if (disbursement == null)
            {
                return HttpNotFound();
            }
            ViewBag.CollectionPointId = new SelectList(_disbursementRepo.GetAllCollectionPoint(), "CollectionPointId", "CollectionName", disbursement.CollectionPointId);

            return View(disbursement);
        }

        //POST: Disbursement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "DisbursementId, CollectionDate, CollectionPointId, AcknowledgeEmpNum, DeptCode, Status")] Disbursement disbursement)
        {
            if (ModelState.IsValid)
            {
                _disbursementRepo.UpdateAndNotify(disbursement);
                return RedirectToAction("Upcoming");
            }
            ViewBag.CollectionPointId = new SelectList(_disbursementRepo.GetAllCollectionPoint(), "CollectionPointId", "CollectionName", disbursement.CollectionPointId);
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
                int disId = disbursementDTO.CurrentDisbursement.DisbursementId;
                Disbursement d = _disbursementRepo.GetById(disId);

                foreach (var dd in d.DisbursementDetails)
                {
                    dd.ActualQty = disbursementDTO.DisDetailList.First(ddEdited => ddEdited.ItemNum == dd.ItemNum)
                        .ActualQty;

                }
                _disbursementRepo.Update(d);
                
                switch (update)
                {
                    case "Acknowledge Manually":
                        _disbursementRepo.Acknowledge(d);
                        return RedirectToAction("Upcoming");
                    case "Generate QR Code":
                        break;
                }
                return Json("Ok");
            }
            return View("Details", disbursementDTO);

        }

        public PartialViewResult _QR(String id)
        {
            QRCodeGenerator qrGen = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGen.CreateQrCode(id, QRCodeGenerator.ECCLevel.Q);
            Base64QRCode qrCode = new Base64QRCode(qrCodeData);
            string QR = qrCode.GetGraphic(20);
            ViewBag.generatedQrCode = QR;
            return PartialView();
        }

        public ActionResult History(string searchString, string currentFilter, int? page)
        {
            List<Disbursement> disbursements = new List<Disbursement>();
            if (searchString != null)
            { page = 1; }
            else
            {
                searchString = currentFilter;
            }

            if (!String.IsNullOrEmpty(searchString))
            {
                disbursements = _disbursementRepo.GetDisbursementsByDeptName(searchString).ToList();
            }
            else
            {
                disbursements = _disbursementRepo.GetAll().OrderByDescending(d=>d.CollectionDate).ToList();
            }
            int pageSize = 15;
            int pageNumber = (page ?? 1);

            var disHistory = disbursements.ToPagedList(pageNumber, pageSize);

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
            Disbursement d = _disbursementRepo.GetById((int)id);
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
