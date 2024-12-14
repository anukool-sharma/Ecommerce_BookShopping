using EOnego.DataAccess.Repository.IRepository;
using EOnego.Models;
using EOnego.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Security.Cryptography.Pkcs;

namespace EOnego.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles=SD.Role_Admin +","+ SD.Role_Employee)]
    public class CoverTypeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CoverTypeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }
        #region APIs
        [HttpGet]
        public IActionResult GetAll()
        {
            return Json(new { data = _unitOfWork.coverType.GetAll() });
        }
        // delete api 
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var coverTypeindb = _unitOfWork.coverType.Get(id);
            if (coverTypeindb == null)
                return Json(new { success = false, message = "Went Wrong" });
            _unitOfWork.coverType.Remove(coverTypeindb);
            _unitOfWork.Save();
            return Json(new { success = true, message = "successfully deleted" });
        }
        #endregion
        public IActionResult upsert(int? id)
        {
            CoverType coverType = new CoverType();
            if (id == null) return View(coverType);
            coverType = _unitOfWork.coverType.Get(id.GetValueOrDefault());
            if (coverType == null) return NotFound();
            return View(coverType);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult upsert(CoverType coverType)
        {
            if (coverType == null) return NotFound();
            if (!ModelState.IsValid) return View(coverType);
            if (coverType.Id == 0)
                _unitOfWork.coverType.Add(coverType);
            else
                _unitOfWork.coverType.Update(coverType);
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }
    }
}
