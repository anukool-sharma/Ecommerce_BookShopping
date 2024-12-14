using EOnego.DataAccess.Repository.IRepository;
using EOnego.Models;
using EOnego.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace EOnego.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =SD.Role_Admin +","+ SD.Role_Employee)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryController(IUnitOfWork unitOfWork)
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
            return Json(new { data = _unitOfWork.category.GetAll() });
        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var categoryIndb = _unitOfWork.category.Get(id);
            if (categoryIndb == null)
                return Json(new { success = false, message = "something went wrong" });
            _unitOfWork.category.Remove(categoryIndb);
            _unitOfWork.Save();
            return Json(new { success = true, message = "data succesfully delete" });

        }
        #endregion
        
        // upsert code start from here----
        public IActionResult upsert(int? id)
        {
            Category category = new Category();
            //for create
            if (id == null) return View(category);
            //for Edit
            category = _unitOfWork.category.Get(id.GetValueOrDefault());
            if (category == null) return NotFound();
            return View(category);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult upsert(Category category)
        {
            if (category == null) NotFound();
            if (!ModelState.IsValid) return View(category);
            if (category.Id == 0)
                _unitOfWork.category.Add(category);
            else
                _unitOfWork.category.Update(category);
            _unitOfWork.Save();
            return RedirectToAction("index");
        }
    }
}
