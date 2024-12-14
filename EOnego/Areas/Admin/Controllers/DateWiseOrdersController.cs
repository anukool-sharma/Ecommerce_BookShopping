using EOnego.DataAccess.Repository.IRepository;
using EOnego.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EOnego.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DateWiseOrdersController : Controller
    {
        public readonly IUnitOfWork _unitOfWork;
        public readonly UserManager<IdentityUser> _userManager;
         public DateWiseOrdersController(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }
        public IActionResult Index(string ListOrder, DateTime? fDate, DateTime? toDate)
        {
            var OrderList = _unitOfWork.OrderHeader.GetAll(IncludeProperties: "ApplicationUser");
            if(fDate.HasValue && toDate.HasValue)
            {
                OrderList = OrderList.Where(o => o.OrderDate >= fDate.Value && o.OrderDate <= toDate.Value);
            }
            if (string.IsNullOrEmpty(ListOrder) == false)
            {
                OrderList = OrderList.Where(o => (ListOrder == "Pending" && o.OrderStatus == SD.OrderStatusPending)||(ListOrder == "Cancelled" && o.OrderStatus == SD.OrderStatusCancelled)||(ListOrder=="Approved" && o.OrderStatus == SD.OrderStatusApproved));
            }
            List<string> ListOption = new List<string>
            {
                "Pending",
                "Cancelled",
                "Approved"
            };
            ViewBag.ListOption = new SelectList(ListOption);

            return View(OrderList.ToList());
        }
    }
}
