using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using LUSSIS.Models;
using LUSSIS.Models.WebDTO;
using LUSSIS.Repositories;
using QRCoder;

namespace LUSSIS.Controllers
{
    public class DisbursementsController : Controller
    {
        private LUSSISContext db = new LUSSISContext();
        private DisbursementRepository disRepo = new DisbursementRepository();
        // GET: Disbursement
        public ActionResult Index()
        {
            var disbursements = disRepo.GetInProcessDisbursements();
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
            disDetailDTO.CurrentDisbursement = disRepo.GetById((int)id);
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
            Disbursement disbursement = await disRepo.GetByIdAsync((int)id);
            if (disbursement == null)
            {
                return HttpNotFound();
            }
            ViewBag.CollectionPointId = new SelectList(disRepo.GetAllCollectionPoint(), "CollectionPointId", "CollectionName", disbursement.CollectionPointId);

            return View(disbursement);
        }

        //POST: Disbursement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "DisbursementId, CollectionDate, CollectionPointId, AcknowledgeEmpNum, DeptCode, Status")] Disbursement disbursement)
        {
            if (ModelState.IsValid)
            {
                await disRepo.UpdateAsync(disbursement);
                return RedirectToAction("Index");
            }
            ViewBag.CollectionPointId = new SelectList(disRepo.GetAllCollectionPoint(), "CollectionPointId", "CollectionName", disbursement.CollectionPointId);
            return View(disbursement);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Acknowledge(DisbursementDetailDTO disbursementDTO)
        {
            if (disbursementDTO == null)
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                //Disbursement d = disbursementDTO.CurrentDisbursement;
                Disbursement d = disRepo.GetById(disbursementDTO.DisDetailList.First().DisbursementId);
                d.DisbursementDetails = disbursementDTO.DisDetailList;
                disRepo.Acknowledge(d);
                return RedirectToAction("Index");
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        
    }
}
