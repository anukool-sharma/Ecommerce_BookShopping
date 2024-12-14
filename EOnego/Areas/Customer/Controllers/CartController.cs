using EOnego.DataAccess.Repository.IRepository;
using EOnego.Models;
using EOnego.Models.ViewModels;
using EOnego.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Stripe;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text;

namespace EOnego.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        private static bool IsEmailConfirm = false;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SmsSender _smsSender;

        public CartController(IUnitOfWork unitOfWork, IEmailSender emailSender, UserManager<IdentityUser> userManager, SmsSender smsSender)
        {
            _unitOfWork = unitOfWork;
            _emailSender = emailSender;
            _userManager = userManager;
            _smsSender = smsSender;
        }
        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }
        public IActionResult Index()
        {   
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if(claims == null)
            {
                ShoppingCartVM = new ShoppingCartVM()
                {
                    ListCart = new List<ShoppingCart>()
                };
                return View(ShoppingCartVM);
            }
            
            // now pass data
            ShoppingCartVM = new ShoppingCartVM()
            {
                ListCart = _unitOfWork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value, IncludeProperties: "Product"), orderHeader = new OrderHeader()
            };

            //Price Calculation
            ShoppingCartVM.orderHeader.OrderTotal = 0;
            ShoppingCartVM.orderHeader.ApplicationUser = _unitOfWork.ApplicationUser.FirstOrDefault(au => au.Id == claims.Value);

            // for price
            foreach (var list in ShoppingCartVM.ListCart)
            {
                list.Price = SD.GetPriceBasedOnQuantity(list.Count, list.Product.Price, list.Product.Price50, list.Product.Price100);
                ShoppingCartVM.orderHeader.OrderTotal += (list.Count*list.Price);
                if (list.Product.Description.Length > 100)
                {
                    list.Product.Description = list.Product.Description.Substring(0, 99) + "...";
                }
            }

            // Email
            if (!IsEmailConfirm)
            {
                ViewBag.EmailMessage = "Email has been sent kindly verify your Email";
                ViewBag.EmailCSS = "text-success";
                IsEmailConfirm = false;
            }
            else
            {
                ViewBag.EmailMessage = "Email must be confirm for authorize customer";
                ViewBag.EmailCSS = "text-danger";
            }
            return View(ShoppingCartVM);
        }

        // For Email Confirmation button  
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [ActionName("Index")]
        public async Task<IActionResult> IndexPost()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var user = _unitOfWork.ApplicationUser.FirstOrDefault(au => au.Id == claims.Value);
            if(user == null)
            {
                ModelState.AddModelError(string.Empty, "Email Empty");
            }
            else
            {
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = user.Id, code = code },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
                    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
                IsEmailConfirm = true;
            }
            return RedirectToAction("Index");
        }
        public IActionResult plus(int id)
        {
            var cart = _unitOfWork.ShoppingCart.Get(id);
            if (cart == null) return NotFound();
            cart.Count += 1;
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }
        public IActionResult minus(int id)
        {
            var cart = _unitOfWork.ShoppingCart.Get(id);
            if (cart == null) return NotFound();
            if (cart.Count == 1)
                cart.Count = 1;
            else
                cart.Count -= 1;
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }
      public IActionResult delete(int id)
        {
            var cart = _unitOfWork.ShoppingCart.Get(id);
            if (cart == null) return NotFound();
            _unitOfWork.ShoppingCart.Remove(cart);
            _unitOfWork.Save();
            // to update in session 
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if(claims != null)
            {
                var count = _unitOfWork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value).ToList().Count();
                HttpContext.Session.SetInt32(SD.Ss_CartSessionCount,count);
            }
            return RedirectToAction("Index");
        }
        // summary Action
        [HttpGet]
        public IActionResult summary(string checkBoxInput, int inputAddress)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var userOrders = _unitOfWork.OrderHeader.GetAll(o => o.ApplicationUserId == claims.Value);

            var RD = userOrders.GroupBy(p => p.PostalCode).Select(s => s.First()).ToList(); // removing duplicate postalCode

            // DropdownList
            List<SelectListItem> Address = new List<SelectListItem>();
            foreach (var order in RD)
            {
                Address.Add(new SelectListItem
                {
                    Text =  order.PostalCode, /*$"{order.PostalCode}",*/
                    Value = order.Id.ToString()
                });
            }
          

            ViewBag.DeliveryAddress = new SelectList(Address,"Value","Text");


            ShoppingCartVM = new ShoppingCartVM()
            {
                ListCart=_unitOfWork.ShoppingCart.GetAll(sc=>sc.ApplicationUserId==claims.Value && checkBoxInput.Contains(sc.Id.ToString()),IncludeProperties:"Product"),orderHeader=new OrderHeader()
            };



            ShoppingCartVM.orderHeader.ApplicationUser = _unitOfWork.ApplicationUser.FirstOrDefault(au => au.Id == claims.Value); // in case when page reload inputaddress is null
            if (inputAddress > 0)
            {
                var orderDetail = _unitOfWork.OrderHeader.FirstOrDefault(o => o.Id == inputAddress && o.ApplicationUserId == claims.Value);

                var result = new
                {
                     orderDetail.Name,
                     orderDetail.StreetAddress,
                     orderDetail.City,
                     orderDetail.State,
                     orderDetail.PostalCode,
                     orderDetail.PhoneNumber
                     
                };
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {

                    return Json(result);
                }
            }
            else
            {
                ShoppingCartVM.orderHeader.Name = ShoppingCartVM.orderHeader.ApplicationUser.Name;
                ShoppingCartVM.orderHeader.StreetAddress = ShoppingCartVM.orderHeader.ApplicationUser.StreetAddress;
                ShoppingCartVM.orderHeader.State = ShoppingCartVM.orderHeader.ApplicationUser.State;
                ShoppingCartVM.orderHeader.City = ShoppingCartVM.orderHeader.ApplicationUser.City;
                ShoppingCartVM.orderHeader.PostalCode = ShoppingCartVM.orderHeader.ApplicationUser.PostalCode;
                ShoppingCartVM.orderHeader.PhoneNumber = ShoppingCartVM.orderHeader.ApplicationUser.PhoneNumber;
            }


            foreach (var list in ShoppingCartVM.ListCart)
            {
                list.Price = SD.GetPriceBasedOnQuantity(list.Count, list.Product.Price, list.Product.Price50, list.Product.Price100);
                ShoppingCartVM.orderHeader.OrderTotal += (list.Price * list.Count);
                if (list.Product.Description.Length>100)
                {
                    list.Product.Description = list.Product.Description.Substring(0, 99) + "....";
                }
            }
            return View(ShoppingCartVM);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("summary")]
        public IActionResult SummaryPost(string checkBoxInput, string stripeToken)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            //storing into orderHeader table
            ShoppingCartVM.orderHeader.ApplicationUser = _unitOfWork.ApplicationUser.FirstOrDefault(au => au.Id == claims.Value);
            ShoppingCartVM.ListCart = _unitOfWork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value && checkBoxInput.Contains(sc.Id.ToString()), IncludeProperties: "Product");

            ShoppingCartVM.orderHeader.OrderStatus = SD.OrderStatusPending;
            ShoppingCartVM.orderHeader.PaymentStatus = SD.PaymentStatusPending;
            ShoppingCartVM.orderHeader.OrderDate = DateTime.Now;
            ShoppingCartVM.orderHeader.ApplicationUserId = claims.Value;
            _unitOfWork.OrderHeader.Add(ShoppingCartVM.orderHeader);
            _unitOfWork.Save();

            //store in  orderDetails table
            foreach (var list in ShoppingCartVM.ListCart)
            {
                list.Price = SD.GetPriceBasedOnQuantity(list.Count, list.Product.Price, list.Product.Price50, list.Product.Price100);
                OrderDetails orderDetails = new OrderDetails()
                {
                    ProductId = list.ProductId,
                    OrderHeaderId = ShoppingCartVM.orderHeader.Id,
                    Price = list.Price,
                    Count = list.Count
                };
                ShoppingCartVM.orderHeader.OrderTotal += (list.Price*list.Count);
                _unitOfWork.OrderDetails.Add(orderDetails);
                _unitOfWork.Save();
            }

            // delete record in shoppingcart table when user place the order

            //_unitOfWork.ShoppingCart.RemoveRange(ShoppingCartVM.ListCart);

           foreach(var cart in ShoppingCartVM.ListCart)
            {
                _unitOfWork.ShoppingCart.Remove(cart);
            }
            _unitOfWork.Save();

            // session count set zero
            var count = _unitOfWork.ShoppingCart.GetAll(sc => sc.ApplicationUserId == claims.Value).ToList().Count();

            HttpContext.Session.SetInt32(SD.Ss_CartSessionCount, count);

            // stripe payment 
            if(stripeToken == null)
            {
                ShoppingCartVM.orderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
                ShoppingCartVM.orderHeader.PaymentStatus = SD.PaymentStatusDelayPayment;
                ShoppingCartVM.orderHeader.OrderStatus = SD.OrderStatusApproved;
            }
            else
            {
                var options = new ChargeCreateOptions()
                {
                    Amount = Convert.ToInt32(ShoppingCartVM.orderHeader.OrderTotal),
                    Currency = "usd",
                    Description="orderId:"+ ShoppingCartVM.orderHeader.Id.ToString(),
                    Source=stripeToken
                };
                var service = new ChargeService();
                Charge charge = service.Create(options);
                if (charge.BalanceTransactionId == null)
                    ShoppingCartVM.orderHeader.PaymentStatus = SD.PaymentStatusRejected;
                else
                    ShoppingCartVM.orderHeader.TranscationId = charge.BalanceTransactionId;

                if (charge.Status.ToLower() == "succeeded")
                {
                    // update column
                    ShoppingCartVM.orderHeader.OrderStatus = SD.OrderStatusApproved;
                    ShoppingCartVM.orderHeader.PaymentStatus = SD.PaymentStatusApproved;
                    ShoppingCartVM.orderHeader.OrderDate = DateTime.Now;
                    ShoppingCartVM.orderHeader.PaymentDate = DateTime.Now;
                    _unitOfWork.Save();
                }
            }


            //*******
            return RedirectToAction("OrderConfirmation", "Cart", new { id = ShoppingCartVM.orderHeader.Id });
        }
        public async Task<IActionResult> OrderConfirmation(int id)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var user = _unitOfWork.ApplicationUser.FirstOrDefault(au => au.Id == claims.Value);
            if(user == null)
            {
                ModelState.AddModelError(string.Empty, "Email Empty");
                return View(id);
            }

            var order = _unitOfWork.OrderHeader.FirstOrDefault(o => o.Id == id);

            //order detail we will send 
            string orderNumber = order.Id.ToString();
            string orderTotal = order.OrderTotal.ToString("0.00");

            // email message body 

            string toEmail = $@"<h2>Thank You for your order</h2>
              <p>Order Number:{orderNumber}</p>
              <p>Order Total:{orderTotal}</p>
              <p>Thank you for shopping with us</p>";

            await _emailSender.SendEmailAsync(user.Email, "Order Confirmation", toEmail);

            // send sms 
            string SMSMessage = $"Thank You for your order!! Your Order Number is{orderNumber}and the total amount is{orderTotal}.";
            _smsSender.SendSms(user.PhoneNumber, SMSMessage);

            // send call
            _smsSender.VoiceCalling(user.PhoneNumber);
            return View(id);
        }

       


    }
}
