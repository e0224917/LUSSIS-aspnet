using LUSSIS.Models.WebDTO;
using LUSSIS.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LUSSIS.Controllers
{
    [Authorize(Roles = "rep")]
    public class CollectionController : Controller
    {
        private readonly DisbursementRepository _disbursementRepo = new DisbursementRepository();
        private readonly CollectionRepository _collectionRepo = new CollectionRepository();
        private readonly DepartmentRepository _departmentRepo = new DepartmentRepository();

        // GET: /Collection/
        public ActionResult Index()
        {
            var deptCode = Request.Cookies["Employee"]?["DeptCode"];
            var disbursement = _disbursementRepo.GetByDateAndDeptCode(DateTime.Now, deptCode);
            return View(disbursement);
        }

        // GET: /Collection/SetCollection
        public ActionResult SetCollection()
        {
            var manageCollectionDto = new ManageCollectionDTO();
            var deptCode = Request.Cookies["Employee"]?["DeptCode"];
            manageCollectionDto.CollectionPoint = _disbursementRepo.GetCollectionPointByDeptCode(deptCode);
            manageCollectionDto.CollectionPoints = _collectionRepo.GetAll();

            return View(manageCollectionDto);
        }

        // POST: /Collection/UpdateCollection
        [HttpPost]
        public ActionResult UpdateCollection(ManageCollectionDTO manageCollectionDto)
        {
            if (!ModelState.IsValid) return RedirectToAction("SetCollection");

            var deptCode = Request.Cookies["Employee"]?["DeptCode"];
            var department = _departmentRepo.GetById(deptCode);

            department.CollectionPointId = manageCollectionDto.DeptCollectionPointId;
            _departmentRepo.Update(department);

            return RedirectToAction("SetCollection");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _disbursementRepo.Dispose();
                _collectionRepo.Dispose();
                _departmentRepo.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}