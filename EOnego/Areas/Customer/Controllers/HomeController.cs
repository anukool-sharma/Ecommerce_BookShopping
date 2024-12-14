using EOnego.DataAccess.Repository.IRepository;
using EOnego.Models;
using EOnego.Models.ViewModels;
using EOnego.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

namespace EOnego.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger,IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index(string listOption, string DataSearch)
        {
            //session Count
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if(claims != null)
            {
                var count = _unitOfWork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value).ToList().Count();
                HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, count);
            }

            //*****
            var productList = _unitOfWork.Product.GetAll(IncludeProperties: "Category,CoverType");

            List<string> filterOption = new List<string>
            {
                "Title",
                "Author",
                "All"
            };
 
            ViewBag.filterOption = new SelectList(filterOption);

            if(string.IsNullOrEmpty(listOption) == false && string.IsNullOrEmpty(DataSearch) == false)
            {
                DataSearch = DataSearch.ToLower();

                if (listOption == "Title")
                {
                    productList = productList.Where(p => p.Title !=null && p.Title.ToLower().Contains(DataSearch)).ToList();
                }
                else if(listOption == "Author")
                {
                    productList = productList.Where(p =>p.Author !=null && p.Author.ToLower().Contains(DataSearch)).ToList();
                }
                else if (listOption == "All")
                {
                    // search by both Title and Author
                    productList = productList.Where(p =>
                        (p.Title != null && p.Title.ToLower().Contains(DataSearch)) || (p.Author != null && p.Author.ToLower().Contains(DataSearch))
                    ).ToList();
                }
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_ProductPartialView", productList);
            }

            return View(productList);


        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult Details(int id)
        {
            // for session
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claims != null)
            {
                var count = _unitOfWork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value).ToList().Count();
                HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, count);
            }

            //******** 
            var productIndb = _unitOfWork.Product.FirstOrDefault(p => p.Id == id, IncludeProperties: "Category,CoverType");
            if (productIndb == null) return NotFound();
            var shoppingCart = new ShoppingCart()
            {
                Product=productIndb,
                ProductId = productIndb.Id
            };
            return View(shoppingCart);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            shoppingCart.Id = 0;
            if (ModelState.IsValid)
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                if (claims == null) return NotFound();
                shoppingCart.ApplicationUserId = claims.Value; // userId fetch

                var shoppingCartIndb = _unitOfWork.ShoppingCart.FirstOrDefault(sc => sc.ApplicationUserId == claims.Value && sc.ProductId == shoppingCart.ProductId);
                if (shoppingCartIndb == null)
                {
                    _unitOfWork.ShoppingCart.Add(shoppingCart);
                }
                else
                    shoppingCartIndb.Count += shoppingCart.Count;
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            else
            {
                var productIndb = _unitOfWork.Product.FirstOrDefault(p => p.Id == shoppingCart.Id, IncludeProperties: "Category,CoverType");
                if (productIndb == null) return NotFound();
                var shoppingCartEdit = new ShoppingCart()
                {
                    Product=productIndb,
                    ProductId=productIndb.Id
                };
            }
            return View(shoppingCart); 
        }
    }
}


