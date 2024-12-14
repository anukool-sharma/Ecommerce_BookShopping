//using AspNetCore;
using EOnego.DataAccess.Repository.IRepository;
using EOnego.Models;
using EOnego.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol;

namespace EOnego.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CompanyController(IUnitOfWork unitOfWork)
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
            return Json(new { data = _unitOfWork.Company.GetAll() });
        }
        //Delete Action
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var companyIndb = _unitOfWork.Company.Get(id);
            if (companyIndb == null)
                return Json(new { success = false, message = "something worng" });
            _unitOfWork.Company.Remove(companyIndb);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Data Deleted Successfully" });
        }

        #endregion
        
        //upsert Action
        public IActionResult upsert(int? id)
        {
            Company company = new Company();
            if (id == null) return View(company);
            //Edit
            company = _unitOfWork.Company.Get(id.GetValueOrDefault());
            if (company == null) return NotFound();
            return View(company);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult upsert(Company company)
        {
            if (company == null) return NotFound();
            if (!ModelState.IsValid) return View(company);
            if (company.Id == 0)
                _unitOfWork.Company.Add(company);
            else
                _unitOfWork.Company.Update(company);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

    }
}
