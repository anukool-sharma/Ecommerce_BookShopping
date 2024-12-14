using EOnego.DataAccess.Data;
using EOnego.DataAccess.Repository.IRepository;
using EOnego.Models;
using EOnego.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EOnego.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        public UserController(IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        #region APIs
        [HttpGet]
        public IActionResult GetAll()
        {
            var userList = _context.users.ToList();  // aspNetUser via applicationUser
            var roles = _context.Roles.ToList(); // aspNetRole
            var userRoles = _context.UserRoles.ToList(); // aspNetUserRoles
            foreach (var user in userList)
            {
                var roleId = userRoles.FirstOrDefault(u => u.UserId == user.Id).RoleId; // fetch roleId
                user.Role = roles.FirstOrDefault(r => r.Id == roleId).Name; // fetch roleName
                if (user.CompanyId == null)
                {
                    user.Company = new Company()
                    {
                        Name=""
                    };
                }
                if(user.CompanyId != null)
                {
                    user.Company = new Company()
                    {
                        Name = _unitOfWork.Company.Get(Convert.ToInt32(user.CompanyId)).Name
                    };
                }
            }
            // Remove admin user
            var adminUser = userList.FirstOrDefault(u => u.Role == SD.Role_Admin);
            userList.Remove(adminUser);
            return Json(new { data = userList });
        }

        // for lock and unlock user
        [HttpPost]
        public IActionResult LockUnlock([FromBody]string id)
        {
            bool isLocked = false;
            var userIndb = _context.users.FirstOrDefault(au => au.Id == id);
            if (userIndb == null)
                return Json(new { success = false, message = "something went wrong while lock and unlock user" });
            if(userIndb !=null && userIndb.LockoutEnd > DateTime.Now)
            {
                userIndb.LockoutEnd = DateTime.Now;
                isLocked = false;
            }
            else
            {
                userIndb.LockoutEnd = DateTime.Now.AddYears(100);
                isLocked = true;
            }
            _context.SaveChanges();
            return Json(new { success = true, message = isLocked == true ? "user successfully locked" : "user successfully unlocked" });
        }
        #endregion
    }
}
