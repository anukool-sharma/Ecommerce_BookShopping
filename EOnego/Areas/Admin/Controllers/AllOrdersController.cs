using EOnego.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;

namespace EOnego.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AllOrdersController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public AllOrdersController(IUnitOfWork unitOfWork)
        {
            _unitOfWork= unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }

        #region APIs
        public IActionResult GetAll()
        {
            return Json(new { data = _unitOfWork.OrderHeader.GetAll(IncludeProperties: "ApplicationUser") });
        }

        public IActionResult orderAll(int id)
        {
            var userOrdDetail = _unitOfWork.OrderDetails.GetAll(filter: o => o.OrderHeaderId == id, IncludeProperties: "OrderHeader,Product");
            return Json(new { data = userOrdDetail });
        }

        #endregion

        public IActionResult Details(int id)
        {
            ViewBag.orderId = id;
            return View();
        }


    }
}
