using EOnego.DataAccess.Repository.IRepository;
using EOnego.Models;
using EOnego.Models.ViewModels;
using EOnego.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EOnego.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin +","+ SD.Role_Employee)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }
        #region APIs
        [HttpGet]
        public IActionResult GetAll()
        {
            return Json(new { data = _unitOfWork.Product.GetAll() });
        }
        public IActionResult Delete(int id)
        {
            var ProductIndb = _unitOfWork.Product.Get(id);

            if (ProductIndb == null)
                return Json(new { success = false, message = "something went wrong" });
            // delete image from folder
            var w3RootFind = _webHostEnvironment.WebRootPath; // wwwroot path get
            var fImgPath = Path.Combine(w3RootFind, ProductIndb.ImageUrl.Trim('\\')); //imageurl from productindb 

            if (System.IO.File.Exists(fImgPath))
            {
                System.IO.File.Delete(fImgPath);
            }

            //Delete image from database
            _unitOfWork.Product.Remove(ProductIndb);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Data Deleted Successfully" });

        }
        #endregion
        public IActionResult upsert(int? id)
        {
            ProductVM productVM = new ProductVM()
            {
                Product = new Product(),
                CategoryList = _unitOfWork.category.GetAll().Select(cl => new SelectListItem()
                {
                    Text = cl.Name,
                    Value = cl.Id.ToString()
                }),

                CoverTypeList = _unitOfWork.coverType.GetAll().Select(ct => new SelectListItem()
                {
                    Text = ct.Name,
                    Value = ct.Id.ToString()
                })
            };
            if (id == null) return View(productVM); //create

            //Edit 
            productVM.Product = _unitOfWork.Product.Get(id.GetValueOrDefault());
            if (productVM.Product == null) return NotFound();
            return View(productVM);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult upsert(ProductVM productVM)
        {
            if (ModelState.IsValid)
            {
                var webRootPath = _webHostEnvironment.WebRootPath; //Get Web Root Path
                var files = HttpContext.Request.Form.Files; // File Input Control Access

                //Check if User Uploaded a File
                if (files.Count() > 0)
                {
                    var fileName = Guid.NewGuid().ToString(); // Generate Unique File Name
                    var extension = Path.GetExtension(files[0].FileName); //Get File Extension
                    var uploads = Path.Combine(webRootPath, @"Images\Products"); //Define Upload Path

                    //Handling Image for Edit Operation

                    if (productVM.Product.Id != 0)
                    {
                        var imageExists = _unitOfWork.Product.Get(productVM.Product.Id).ImageUrl;
                        productVM.Product.ImageUrl = imageExists;
                    }

                    //Delete Existing Image if New Image is Uploaded
                    if (productVM.Product.ImageUrl != null)
                    {
                        var imagePath = Path.Combine(webRootPath, productVM.Product.ImageUrl.Trim('\\'));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }
                    // this is for create file in products folder & Save New Image to Disk
                    using (var fileStream = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        files[0].CopyTo(fileStream);
                    }
                    //Save Image Path to Database
                    productVM.Product.ImageUrl = @"\Images\Products\" + fileName + extension;
                }
                else
                {
                    if (productVM.Product.Id != 0)
                    {
                        var imageExists = _unitOfWork.Product.Get(productVM.Product.Id).ImageUrl;
                        productVM.Product.ImageUrl = imageExists;
                    }
                }

                if (productVM.Product.Id == 0)
                    _unitOfWork.Product.Add(productVM.Product);
                else
                    _unitOfWork.Product.Update(productVM.Product);
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            // if it doesnot valid

            else
            {
                productVM = new ProductVM()
                {
                    Product = new Product(),
                    CategoryList = _unitOfWork.category.GetAll().Select(ct => new SelectListItem()
                    {
                        Text = ct.Name,
                        Value = ct.Id.ToString()
                    }),

                    CoverTypeList = _unitOfWork.coverType.GetAll().Select(cl => new SelectListItem()
                    {
                        Text = cl.Name,
                        Value = cl.Id.ToString()
                    })
                };

                // Edit 
                if (productVM.Product.Id != 0)
                {
                    productVM.Product = _unitOfWork.Product.Get(productVM.Product.Id);
                }
                return View(productVM);
            }

        }
    }
}
